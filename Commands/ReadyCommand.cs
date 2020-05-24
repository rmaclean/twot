namespace twot
{
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.IO;
    using System.Linq;

    using Tweetinvi;

    using static System.ConsoleColor;
    using static CommandHelpers;
    using static ConsoleHelper;

#pragma warning disable CA1812
    internal class ReadyCommand : ICommand
#pragma warning restore CA1812
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Ready", "Checks your config is ready to go");
            cmd.AddAlias("ready");
            cmd.AddAlias("r");

            cmd.Handler = CommandHandler.Create(this.Execute);
            rootCommand.Add(cmd);
        }

        private int Execute()
        {
            if (!CommandHeader("Ready ❔"))
            {
                return -1;
            }

            var me = User.GetAuthenticatedUser();
            if (me == null)
            {
                Writeln(Red, "🛑 Config incorrect");
                var latestException = ExceptionHandler.GetLastException();
                Writeln(DarkRed, $"  {latestException.TwitterDescription}");
                return -1;
            }

            Writeln(Green, $"✅ Config correct for @{me.ScreenName}");
            Writeln(Green, $" You have blocked {me.GetBlockedUserIds().Count()} people");

            if (File.Exists("score.json"))
            {
                try
                {
                    #pragma warning disable CA1806
                    new ScoreConfig();
                    #pragma warning restore CA1806
                    Writeln(Green, $"✅ Score config correct for Clean command");
                }
            #pragma warning disable CA1031
                catch
            #pragma warning restore CA1031
                {
                    Writeln(Red, "🛑 Score config incorrect");
                    return -1;
                }
            }
            else
            {
                Writeln(DarkGreen, $"✅ No score.json found");
            }

            return 0;
        }
    }
}
