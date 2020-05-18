namespace twot
{
    using System.Linq;
    using System;
    using System.Threading.Tasks;
    using Tweetinvi;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Tweetinvi.Models;
    using System.Collections.Generic;
    using static ConsoleHelper;
    using static System.ConsoleColor;

    class BlockTrain : ICommand
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("BlockTrain", "Blocks someone, and everyone which follows them.");
            cmd.AddAlias("blocktrain");
            cmd.AddAlias("bt");

            var dryRunOption = new Option<bool>("--dryrun", "Does not actually make the changes");
            cmd.Add(dryRunOption);

            var targetOption = new Option<string>("--target", "Person to block");
            targetOption.AddAlias("-t");
            targetOption.Name = "targetUsername";
            targetOption.Required = true;
            cmd.Add(targetOption);

            var logOption = new Option<bool>("--log", "Creates (or appends if the file exists) a log of all the " +
                "accounts impacted by the BlockTrain");
            logOption.Name = "log";
            logOption.AddAlias("-l");
            cmd.Add(logOption);

            cmd.Handler = CommandHandler.Create<bool, string, bool>(Execute);
            rootCommand.Add(cmd);
        }

        private async Task<List<IUser>> GetTargets(string targetUsername)
        {
            using (var spinner = new Spinner())
            {
                var result = new List<IUser>();
                var me = User.GetAuthenticatedUser();
                var friends = await me.GetFriendsAsync(Int32.MaxValue);

                var target = User.GetUserFromScreenName(targetUsername);
                result.Add(target);

                var enermies = await target.GetFollowersAsync(Int32.MaxValue);

                var targetsForBlock = enermies.Where(enermy => !friends.Contains(enermy));
                result.AddRange(targetsForBlock);
                spinner.Done();
                return result;
            }
        }

        private async Task Execute(bool dryRun, string targetUsername, bool log)
        {
            Writeln(Cyan, "Block Train 🚂");
            if (dryRun)
            {
                Writeln(Yellow, " ⚠ Dry run mode");
            }

            Console.WriteLine();

            Writeln(DarkBlue, "Loading people to block, this may take some time...");
            var targets = await GetTargets(targetUsername).ConfigureAwait(false);

            using (var logger = new ThreadedLogger("BlockTrain.log", log))
            using (var pbar = new ProgressBar(targets.Count))
            {
                logger.LogMessage($"# BlockTrain started {DateTime.Now.ToLongDateString()} " +
                    $"{DateTime.Now.ToLongTimeString()}");

                foreach (var targetUser in targets)
                {
                    if (!dryRun)
                    {
                        await targetUser!.BlockAsync();
                    }

                    pbar.Tick($"Blocked @{targetUser.ScreenName}");
                    logger.LogMessage(targetUser!.ScreenName);
                }
            }

            Writeln(Green, $"Blocked a total of { targets.Count } people");
        }
    }
}
