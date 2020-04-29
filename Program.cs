using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace twot
{
    class Program
    {
        static ICommand[] commands = { new CleanCommand(), new BlockTrain() };

        static int Main(string[] args) 
        {
            var config = Config.Load();
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

            Auth.SetUserCredentials(config.APIKey, config.APISecret, config.AccessToken, config.AccessSecret);

            var rootCommand = new RootCommand("Twot - it makes living with twitter better");
            foreach (var command in commands)
            {
                command.AddCommand(rootCommand);
            }

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
