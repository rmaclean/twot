namespace twot
{
    using System;
    using System.CommandLine;
    using Tweetinvi;

    class Program
    {
        static ICommand[] commands = {
            new CleanCommand(),
            new BlockTrain(),
            new ReadyCommand(),
            new InitCommand(),
            new ScoreCommand(),
        };

        static void EnableUTFConsole()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Write("\xfeff"); // bom = byte order mark
        }

        static int Main(string[] args)
        {
            EnableUTFConsole();
            var configResult = Config.Load();
            if (!configResult.Success)
            {
                return -1;
            }

            var config = configResult.Config;
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

            Auth.SetUserCredentials(config.APIKey, config.APISecret, config.AccessToken, config.AccessSecret);

            var rootCommand = new RootCommand("Twot: Making Twitter Better");
            foreach (var command in commands)
            {
                command.AddCommand(rootCommand);
            }

            Console.WriteLine();
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
