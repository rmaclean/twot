using System.Linq;
using System;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Tweetinvi;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace twot
{
    class CleanCommand : ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Clean", "Scores everyone who follows the supplied user and if they do not meet a min-score they get blocked and unblocked which forces them to unfollow you.");
            var dryRunOption = new Option<bool>("--dryrun", "Does not actually make the changes");
            cmd.Add(dryRunOption);

            var minScoreOption = new Option<double>(
                "--score",
                () => 0.85,
                "Sets the score for min kicking. Defaults to 0.85"
            );
            minScoreOption.Name = "minscore";
            minScoreOption.AddAlias("-s");
            cmd.Add(minScoreOption);

            cmd.Handler = CommandHandler.Create<bool, double>(Execute);
            rootCommand.Add(cmd);
        }

        private async Task Execute(bool dryRun, double minScore)
        {
            Console.WriteLine("Running cleanup 🧹");
            if (dryRun)
            {
                Console.WriteLine("  ⚠ Dry run mode");
            }

            var me = User.GetAuthenticatedUser();
            Console.WriteLine($"Cleaning the followers of @{me.ScreenName}.");
            Console.WriteLine($"Min score: {minScore}");

            var friends = await me.GetFriendIdsAsync(Int32.MaxValue);
            var followers = await me.GetFollowersAsync(Int32.MaxValue);

            var botsOrDead = followers
                .Where(_ => !friends.Contains(_.Id))
                .Select(Follower => (Follower, Score: Score(Follower)))
                .Where(_ => _.Score > minScore)
                .OrderBy(_ => _.Follower.Name);

            foreach (var (follower, score) in botsOrDead.AsParallel())
            {
                Console.WriteLine($"{follower} {score}\n\t@{follower.UserIdentifier.ScreenName} {follower.FollowersCount} {follower.Status?.CreatedAt}");
                Console.WriteLine();
                if (!dryRun)
                {
                    await follower.BlockAsync();
                    await follower.UnBlockAsync();
                }
            }

            Console.WriteLine(botsOrDead.Count());
        }

        double Score(IUser follower)
        {
            var result = 0.0;
            if (follower.DefaultProfileImage)
            {
                result += 1;
            }

            if (follower.Description.Length == 0)
            {
                result += 0.3;
            }

            if (follower.FavouritesCount == 0)
            {
                result += 0.8;
            }

            if (follower.FriendsCount < 20)
            {
                result += 0.5;
            }

            if (follower.Location.Length == 0)
            {
                result += 0.2;
            }

            if (follower.FollowersCount > 10000)
            {
                result += 0.2;

                if (follower.FollowersCount > 25000)
                {
                    result += 0.7;
                }
            }

            if (follower.StatusesCount < 10)
            {
                result += 0.3;

                if (follower.StatusesCount == 0)
                {
                    result += 0.5;
                }
            }

            if (follower.Status != null)
            {
                if (follower.Status.CreatedAt > DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                {
                    result -= 0.4;
                }

                if (follower.Status.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromDays(90)))
                {
                    result += 0.5;
                }

                if (follower.Status.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromDays(365)))
                {
                    result += 1;
                }
            }

            if (follower.FriendsCount > 5000)
            {
                result += 0.3;
            }

            return result;
        }
    }
}