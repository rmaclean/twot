using System;
using System.CommandLine;
using Tweetinvi;

namespace twot
{
    class Program
    {
        static ICommand[] commands = { new CleanCommand(), new BlockTrain() };

        static void EnableUTFConsole()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Write("\xfeff"); // bom = byte order mark
        }

        static int Main(string[] args)
        {
            EnableUTFConsole();
            var config = Config.Load();
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

            Auth.SetUserCredentials(config.APIKey, config.APISecret, config.AccessToken, config.AccessSecret);

            var rootCommand = new RootCommand("Twot - it makes living with twitter better");
            foreach (var command in commands)
            {
                command.AddCommand(rootCommand);
            }

            Console.WriteLine();
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
