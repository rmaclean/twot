namespace twot
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;
    using Tweetinvi;
    using Tweetinvi.Models;
    using static System.ConsoleColor;
    using static CommandHelpers;
    using static ConsoleHelper;

#pragma warning disable CA1812
    internal class ExportCommand : ICommand
#pragma warning restore CA1812
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Export", "Exports your block or mute list");
            cmd.AddAlias("export");
            cmd.AddAlias("e");

            var muteOption = new Option<bool>("--mute", "Rather than export your block list, this command will " +
                "export your mute list");
            muteOption.AddAlias("-m");
            cmd.Add(muteOption);

            var fileOption = new Option<string>("--file", () => "export.log", "The file to export to. Defaults to " +
                "export.log");
            fileOption.AddAlias("-f");
            cmd.Add(fileOption);

            cmd.Handler = CommandHandler.Create<bool, string>(this.Execute);
            rootCommand.Add(cmd);
        }

        private async Task<int> Execute(bool mute, string file)
        {
            if (!CommandHeader("Running export ✍", false))
            {
                return -1;
            }

            using (var logger = new ThreadedLogger($"{file}", true))
            using (var spinner = new Spinner("Loading your details"))
            {
                logger.LogMessage($"# Export started {DateTime.Now.ToLongDateString()} " +
                    $"{DateTime.Now.ToLongTimeString()}");
                logger.LogMessage($"# {(mute ? "Muted accounts" : "Blocked accounts")}");

                var me = User.GetAuthenticatedUser();
                IEnumerable<IUser> users;
                if (mute)
                {
                    spinner.Message = "Loading your muted accounts";
                    users = Account.GetMutedUsers();
                }
                else
                {
                    spinner.Message = "Loading your blocked accounts";
                    users = await me.GetBlockedUsersAsync();
                }

                foreach (var user in users)
                {
                    logger.LogMessage(user.ScreenName);
                }
            }

            Writeln(Green, "Export completed");
            return 0;
        }
    }
}
