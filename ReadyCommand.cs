using System;
using Tweetinvi;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace twot
{
    class ReadyCommand : ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Ready", "Checks your config is ready to go");

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
        }
    }
}