namespace twot
{
    using System;
    using Tweetinvi;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;

    class ReadyCommand : ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Ready", "Checks your config is ready to go");
            cmd.AddAlias("ready");
            cmd.AddAlias("r");

            cmd.Handler = CommandHandler.Create(Execute);
            rootCommand.Add(cmd);
        }

        private void Execute()
        {
            Console.WriteLine("Ready ❔");

            var me = User.GetAuthenticatedUser();
            if (me == null)
            {
                Console.WriteLine("🛑 Config incorrect");
                var latestException = ExceptionHandler.GetLastException();
                Console.WriteLine($"  {latestException.TwitterDescription}");
                return;
            }

            Console.WriteLine($"✅ Config correct for @{me.ScreenName}");

            if (File.Exists("score.json"))
            {
                try
                {
                    new ScoreConfig();
                    Console.WriteLine($"✅ Score config correct for Clean command");
                }
                catch
                {
                    Console.WriteLine("🛑 Score config incorrect");
                }
            }
        }
    }
}
