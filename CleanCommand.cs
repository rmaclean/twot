namespace twot
{
    using System.Linq;
    using System;
    using System.Threading.Tasks;
    using Tweetinvi.Models;
    using Tweetinvi;
    using System.CommandLine;
    using System.CommandLine.Invocation;

    class CleanCommand : ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Clean", "Scores everyone who follows the supplied user and if they do not meet a min-score they get blocked and unblocked which forces them to unfollow you.");
            cmd.AddAlias("clean");
            cmd.AddAlias("c");

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

            var scoringConfig = new ScoreConfig();

            var botsOrDead = followers
                .Where(_ => !friends.Contains(_.Id))
                .Select(Follower => (Follower, Score: Score(Follower, scoringConfig)))
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

        double Score(IUser follower, ScoreConfig scoring)
        {
            var result = 0.0;
            if (scoring.DefaultProfileImage.Enabled && follower.DefaultProfileImage == scoring.DefaultProfileImage.Value)
            {
                result += scoring.DefaultProfileImage.Impact;
            }

            if (scoring.DescriptionLength.Enabled && follower.Description.Length <= scoring.DescriptionLength.Value)
            {
                result += scoring.DescriptionLength.Impact;
            }

            if (scoring.Favourites.Enabled && follower.FavouritesCount <= scoring.Favourites.Value)
            {
                result += scoring.Favourites.Impact;
            }

            if (scoring.FriendsLessThan.Enabled && follower.FriendsCount <= scoring.FriendsLessThan.Value)
            {
                result += scoring.FriendsLessThan.Impact;
            }

            if (scoring.LocationLength.Enabled && follower.Location.Length <= scoring.LocationLength.Value)
            {
                result += scoring.LocationLength.Impact;
            }

            if (scoring.FollowersLarge.Enabled && follower.FollowersCount >= scoring.FollowersLarge.Value)
            {
                result += scoring.FollowersLarge.Impact;
            }

            if (scoring.FollowersExtraLarge.Enabled && follower.FollowersCount >= scoring.FollowersExtraLarge.Value)
            {
                result += scoring.FollowersExtraLarge.Impact;
            }

            if (scoring.TweetsLessThan.Enabled && follower.StatusesCount <= scoring.TweetsLessThan.Value)
            {
                result += scoring.TweetsLessThan.Impact;
            }

            if (scoring.ZeroTweets.Enabled && follower.StatusesCount == scoring.ZeroTweets.Value)
            {
                result += scoring.ZeroTweets.Impact;
            }

            if (follower.Status != null)
            {
                if (scoring.TweetedInLastWeek.Enabled && follower.Status.CreatedAt > DateTime.Now.Subtract(TimeSpan.FromDays(7)))
                {
                    result -= scoring.TweetedInLastWeek.Impact;
                }

                if (scoring.TweetedMoreThan90Days.Enabled && follower.Status.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromDays(90)))
                {
                    result += scoring.TweetedMoreThan90Days.Impact;
                }

                if (scoring.TweetedMoreThan1Year.Enabled && follower.Status.CreatedAt < DateTime.Now.Subtract(TimeSpan.FromDays(365)))
                {
                    result += scoring.TweetedMoreThan1Year.Impact;
                }
            }

            if (scoring.FollowingMoreThan.Enabled && follower.FriendsCount >= scoring.FollowingMoreThan.Value)
            {
                result += scoring.FollowingMoreThan.Impact;
            }

            return result;
        }
    }
}
