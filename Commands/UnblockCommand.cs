namespace twot
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;
    using Tweetinvi;
    using Tweetinvi.Models;
    using System.IO;
    using static ConsoleHelper;
    using static System.ConsoleColor;

    class UnblockCommand : ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Unblock", "Unblocks one person, a group of people or everyone");
            cmd.AddAlias("unblock");
            cmd.AddAlias("un");

            var dryRunOption = new Option<bool>("--dryrun", "Does not actually make the changes");
            cmd.Add(dryRunOption);

            var targetOption = new Option<string>("--target", "If unblocking one person, this is their name");
            targetOption.AddAlias("-t");
            targetOption.Name = "targetUsername";
            cmd.Add(targetOption);

            var allOption = new Option<bool>("--all", "Unblocks everyone you have blocked");
            allOption.Name = "all";
            cmd.Add(allOption);

            var fileOption = new Option<string>("--file", "Unblocks from a file");
            fileOption.AddAlias("-f");
            fileOption.Name = "file";
            cmd.Add(fileOption);

            var logOption = new Option<bool>("--log", "Creates (or appends if the file exists) a log of all the " +
                "accounts impacted by the unblocking");
            logOption.Name = "log";
            logOption.AddAlias("-l");
            cmd.Add(logOption);

            var unmuteOption = new Option<bool>("--unmute", "Rather than unblocking, this command will unmute the targets");
            cmd.Add(unmuteOption);

            cmd.Handler = CommandHandler.Create<bool, string, bool, string, bool, bool>(Execute);
            rootCommand.Add(cmd);
        }

        private async Task ProcessUser(bool dryRun, bool unmute, IUser target, ProgressBar pbar, ThreadedLogger logger)
        {
            if (!dryRun)
            {
                if (unmute)
                {
                    Account.UnMuteUser(target.UserIdentifier);
                }
                else
                {
                    await target.UnBlockAsync();
                }
            }

            pbar.Tick($"{(unmute ? "Unmuted" : "Unblocked")} @{target.ScreenName}");
            logger.LogMessage(target!.ScreenName);
        }

        private async Task Execute(bool dryRun, string targetUsername, bool all, string file, bool log, bool unmute)
        {
            Writeln(Cyan, "Unblock 🔁");
            if (dryRun)
            {
                Writeln(Yellow, " ⚠ Dry run mode");
            }

            Console.WriteLine();

            var me = User.GetAuthenticatedUser();

            var accountsToUnblock = new List<IUser>();

            if (all)
            {
                using (var spinner = new Spinner())
                {
                    if (unmute)
                    {
                        accountsToUnblock = Account.GetMutedUsers(Int32.MaxValue).ToList();
                    }
                    else
                    {
                        accountsToUnblock = me.GetBlockedUsers().ToList();
                    }

                    spinner.Done();
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(targetUsername))
                {
                    var individual = User.GetUserFromScreenName(targetUsername);
                    accountsToUnblock.Add(individual!);
                }

                if (!string.IsNullOrEmpty(file) && File.Exists(file))
                {
                    accountsToUnblock.AddRange(
                        (await File.ReadAllLinesAsync(file))
                        .AsParallel()
                        .Where(line => !line.StartsWith("#"))
                        .Select(line => line.Split(',', 1)[0].Trim())
                        .Where(line => !line.Contains(' '))
                        .Select(line => User.GetUserFromScreenName(line))
                    );
                }
            }

            using (var logger = new ThreadedLogger("Unblock.log", log))
            using (var pbar = new ProgressBar(accountsToUnblock.Count))
            {
                logger.LogMessage($"# Unblock started {DateTime.Now.ToLongDateString()} " +
                    $"{DateTime.Now.ToLongTimeString()}");

                var actions = accountsToUnblock
                    .Select(target => ProcessUser(dryRun, unmute, target, pbar, logger))
                    .ToArray();

                Task.WaitAll(actions);
            }

            Writeln(Green, $"{(unmute ? "Unmuted" : "Unblocked")} a total of {accountsToUnblock.Count} people");
        }
    }
}
