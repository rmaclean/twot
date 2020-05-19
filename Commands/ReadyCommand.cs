namespace twot
{
    using Tweetinvi;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;
    using static ConsoleHelper;
    using static System.ConsoleColor;

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
            Writeln(Cyan, "Ready ❔");

            var me = User.GetAuthenticatedUser();
            if (me == null)
            {
                Writeln(Red, "🛑 Config incorrect");
                var latestException = ExceptionHandler.GetLastException();
                Writeln(DarkRed, $"  {latestException.TwitterDescription}");
                return;
            }

            Writeln(Green, $"✅ Config correct for @{me.ScreenName}");
            Writeln(Green, $" You have blocked {me.GetBlockedUserIds().Count()} people");

            if (File.Exists("score.json"))
            {
                try
                {
                    new ScoreConfig();
                    Writeln(Green, $"✅ Score config correct for Clean command");
                }
                catch
                {
                    Writeln(Red, "🛑 Score config incorrect");
                }
            }
            else
            {
                Writeln(DarkGreen, $"✅ No score.json found");
            }
        }
    }
}
