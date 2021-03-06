namespace twot
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    internal class ScoreConfig
    {
        public ScoreConfig()
        {
            if (File.Exists("score.json"))
            {
                var properties = this.GetType().GetProperties();
                var overridesJson = File.ReadAllText("score.json");
                var overrides = JsonConvert.DeserializeObject<Dictionary<string, Score<object>>>(overridesJson);
                foreach (var (key, score) in overrides)
                {
                    var property = properties.FirstOrDefault(prop => prop.Name == key);
                    if (property != null)
                    {
                        switch (score.Value)
                        {
                            case bool b:
                                {
                                    property.SetValue(this, new Score<bool>(b, score.Impact, score.Enabled));
                                    break;
                                }

                            case long i:
                                {
                                    property.SetValue(this, new Score<int>(
                                        Convert.ToInt32(i),
                                        score.Impact,
                                        score.Enabled));
                                    break;
                                }

                            default:
                                {
                                    throw new Exception($"Unknown config type {score.Value.GetType()} for {key}");
                                }
                        }
                    }
                }
            }
        }

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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    internal class Score<T>
    {
        public Score(T value, double impact, bool enabled = true)
        {
            this.Enabled = enabled;
            this.Value = value;
            this.Impact = impact;
        }

        public bool Enabled { get; set; }

        public T Value { get; set; }

        public double Impact { get; set; }
    }
}
