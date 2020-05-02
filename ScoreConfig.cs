namespace twot
{
    class ScoreConfig
    {
        public Score<bool> DefaultProfileImage { get; private set; } = new Score<bool>(true, 1);
        public Score<int> DescriptionLength { get; private set; } = new Score<int>(0, 0.3);
        public Score<int> Favourites { get; private set; } = new Score<int>(0, 0.8);
        public Score<int> FriendsLessThan { get; private set; } = new Score<int>(20, 0.5);
        public Score<int> LocationLength { get; private set; } = new Score<int>(0, 0.2);
        public Score<int> FollowersLarge { get; private set; } = new Score<int>(10000, 0.2);
        public Score<int> FollowersExtraLarge { get; private set; } = new Score<int>(25000, 0.7);
        public Score<int> TweetsLessThan { get; private set; } = new Score<int>(10, 0.3);
        public Score<int> ZeroTweets { get; private set; } = new Score<int>(0, 0.5);
        public Score<bool> TweetedInLastWeek { get; private set; } = new Score<bool>(true, -0.4);
        public Score<bool> TweetedMoreThan90Days { get; private set; } = new Score<bool>(true, 0.5);
        public Score<bool> TweetedMoreThan1Year { get; private set; } = new Score<bool>(true, 1);
        public Score<int> FollowingMoreThan { get; private set; } = new Score<int>(5000, 0.3);
    }

    class Score<T>
    {
        public bool Enabled { get; private set; }

        public T Value { get; private set; }

        public double Impact { get; private set; }

        public Score(T value, double impact, bool enabled = true)
        {
            this.Enabled = enabled;
            this.Value = value;
            this.Impact = impact;
        }
    }
}