namespace twot
{
    using System.Linq;
    using System;
    using System.Threading.Tasks;
    using Tweetinvi.Models;
    using Tweetinvi;
    using System.Collections.Generic;
    using static ConsoleHelper;
    using static System.ConsoleColor;

    class ScoreSettings
    {
        public string mode { get; set; } = "";
        public Action<int>? onComplete { get; set; }
        public Func<IUser?, Task>? onUserAsync { get; set; }
        public Action<IUser?, ThreadedLogger, double>? onUser { get; set; }
        public Action<ThreadedLogger>? beforeRun { get; set; }
    }

    class BaseScoreCommand
    {
        internal async Task<List<(IUser, double)>> GetBotsOrDead(double minScore)
        {
            Writeln(DarkBlue, "Loading people, this may take some time...");
            using (var spinner = new Spinner())
            {
                var me = User.GetAuthenticatedUser();

                var friends = await me.GetFriendIdsAsync(Int32.MaxValue);
                var followers = await me.GetFollowersAsync(Int32.MaxValue);

                var scoringConfig = new ScoreConfig();

                var result = followers
                    .Where(_ => !friends.Contains(_.Id))
                    .Select(Follower => (Follower, Score: Score(Follower, scoringConfig)))
                    .Where(_ => _.Score > minScore)
                    .OrderBy(_ => _.Follower.Name);

                spinner.Done();
                return result.ToList();
            }
        }

        private async Task InvokeAction(IUser follower, double score, ThreadedLogger logger, ProgressBar pbar, ScoreSettings settings)
        {
            if (settings.onUserAsync != null)
            {
                await settings.onUserAsync!.Invoke(follower);
            }

            settings.onUser?.Invoke(follower, logger, score);
            pbar.Tick($"Processed @{follower.UserIdentifier.ScreenName}");
        }

        internal async Task Run(double minScore, bool log, ScoreSettings settings)
        {
            var botsOrDead = await GetBotsOrDead(minScore);
            using (var logger = new ThreadedLogger($"{settings.mode}.log", log))
            using (var pbar = new ProgressBar(botsOrDead.Count))
            {
                logger.LogMessage($"# {settings.mode} started {DateTime.Now.ToLongDateString()} " +
                    $"{DateTime.Now.ToLongTimeString()}");

                var actions = botsOrDead
                    .Select(item =>
                    {
                        var (follower, score) = item;
                        return InvokeAction(follower, score, logger, pbar, settings);
                    }).ToArray();

                Task.WaitAll(actions);
            }

            settings.onComplete!.Invoke(botsOrDead.Count);
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
