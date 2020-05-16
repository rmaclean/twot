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

            cmd.Handler = CommandHandler.Create<bool, string, bool, string, bool>(Execute);
            rootCommand.Add(cmd);
        }


        private async Task Execute(bool dryRun, string targetUsername, bool all, string file, bool log)
        {
            Console.WriteLine("Unblock 🔁");
            if (dryRun)
            {
                Console.WriteLine(" ⚠ Dry run mode");
            }

            Console.WriteLine();

            var me = User.GetAuthenticatedUser();

            var accountsToUnblock = new List<IUser>();

            if (all)
            {
                accountsToUnblock = me.GetBlockedUsers().ToList();
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

                foreach (var target in accountsToUnblock.AsParallel())
                {
                    if (!dryRun)
                    {
                        await target.UnBlockAsync();
                    }

                    pbar.Tick($"Unblocked @{target.ScreenName}");
                    logger.LogMessage(target!.ScreenName);
                }
            }

            Console.WriteLine($"Unblocked a total of {accountsToUnblock.Count} people");
        }
    }
}
