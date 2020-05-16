namespace twot
{
    using System;
    using System.CommandLine;
    using System.Reflection;
    using Tweetinvi;
    using System.Linq;
    using System.Collections.Generic;

    class Program
    {
        static IEnumerable<ICommand> GetCommands()
        {
            return Assembly.GetAssembly(typeof(Program))!.GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t))
                .Where(t => t.IsClass)
                .Select(commandInterface => Activator.CreateInstance(commandInterface) as ICommand)
                .Select(commandObject => commandObject!);
        }

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
            foreach (var command in GetCommands())
            {
                command.AddCommand(rootCommand);
            }

            Console.WriteLine();
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
