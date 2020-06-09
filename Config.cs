namespace twot
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Microsoft.Extensions.Configuration;
    using Tweetinvi;
    using Tweetinvi.Events;
    using static System.ConsoleColor;
    using static ConsoleHelper;

    internal class Config
    {
        private Config()
        {
            // no-op
        }

        [ConfigInfo(DisplayName = "API Key", ConfigProperty = "apikey")]
        public string APIKey { get; private set; } = string.Empty;

        [ConfigInfo(DisplayName = "API Secret", ConfigProperty = "apisecret")]
        public string APISecret { get; private set; } = string.Empty;

        [ConfigInfo(DisplayName = "Access Token", ConfigProperty = "accesstoken")]
        public string AccessToken { get; private set; } = string.Empty;

        [ConfigInfo(DisplayName = "Access Secret", ConfigProperty = "accesssecret")]
        public string AccessSecret { get; private set; } = string.Empty;

        public static (bool Success, Config Config) Load()
        {
            var configuration = new ConfigurationBuilder()
                                .AddUserSecrets(typeof(Program).Assembly)
                                .AddJsonFile("./secrets.json", true, false)
                                .Build();

            var section = configuration.GetSection("twot");
            var result = new Config();

            var propertiesToSet = result.GetType().GetProperties()
                .Select(propertyInfo =>
                    (PropertyInfo: propertyInfo, Config: propertyInfo.GetCustomAttribute<ConfigInfoAttribute>()))
                .Where(property => property.Config != null);

            foreach (var propInfo in propertiesToSet)
            {
                var configValue = section.GetValue<string>(propInfo.Config!.ConfigProperty);
                if (string.IsNullOrWhiteSpace(configValue))
                {
                    Writeln(Red, $"🚨 {propInfo.Config.DisplayName} not set");
                    return (false, new Config());
                }

                propInfo.PropertyInfo.SetValue(result, configValue);
            }

            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;
            TweetinviEvents.QueryBeforeExecute += RateLimitCheck;
            Auth.SetUserCredentials(result.APIKey, result.APISecret, result.AccessToken, result.AccessSecret);
            return (true, result);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, this.GetType().GetProperties()
                .Select(propertyInfo =>
                    (PropertyInfo: propertyInfo, Config: propertyInfo.GetCustomAttribute<ConfigInfoAttribute>()))
                .Where(property => property.Config != null)
                .Select(property => $"{property.Config!.DisplayName} = {property.PropertyInfo.GetValue(this)}"));
        }

        private static void RateLimitCheck(object? sender, QueryBeforeExecuteEventArgs args)
        {
            var queryRateLimits = RateLimit.GetQueryRateLimit(args.QueryURL);

            if (queryRateLimits != null)
            {
                if (queryRateLimits.Remaining > 0)
                {
                    return;
                }

                using (var spinner = new Spinner("You have been rate limited by Twitter until " +
                    $"{queryRateLimits.ResetDateTime.ToLongTimeString()}. Please wait or Ctrl+C to quit"))
                {
                    Thread.Sleep((int)queryRateLimits.ResetDateTimeInMilliseconds);
                    spinner.Done();
                }
            }
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class ConfigInfoAttribute : System.Attribute
    {
        public string DisplayName { get; set; } = string.Empty;

        public string ConfigProperty { get; set; } = string.Empty;
    }
}
