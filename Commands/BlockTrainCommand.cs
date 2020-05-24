namespace twot
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Threading.Tasks;

    using Tweetinvi;
    using Tweetinvi.Models;

    using static System.ConsoleColor;
    using static CommandHelpers;
    using static ConsoleHelper;

#pragma warning disable CA1812
    internal class BlockTrain : ICommand
#pragma warning restore CA1812
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

            var muteOption = new Option<bool>("--mute", "Rather than blocking, this command will mute the targets");
            muteOption.AddAlias("-m");
            cmd.Add(muteOption);

            cmd.Handler = CommandHandler.Create<bool, string, bool, bool>(this.Execute);
            rootCommand.Add(cmd);
        }

        private static async Task<List<IUser>> GetTargets(string targetUsername)
        {
            using (var spinner = new Spinner())
            {
                var result = new List<IUser>();
                var me = User.GetAuthenticatedUser();
                var friends = await me.GetFriendsAsync(int.MaxValue);

                var target = User.GetUserFromScreenName(targetUsername);
                result.Add(target);

                var enermies = await target.GetFollowersAsync(int.MaxValue);

                var targetsForBlock = enermies.Where(enermy => !friends.Contains(enermy));
                result.AddRange(targetsForBlock);
                spinner.Done();
                return result;
            }
        }

        private static async Task BlockUser(IUser targetUser, bool dryRun, ProgressBar pbar, ThreadedLogger logger, bool mute)
        {
            if (!dryRun)
            {
                if (mute)
                {
                    Tweetinvi.Account.MuteUser(targetUser.UserIdentifier);
                }
                else
                {
                    await targetUser!.BlockAsync();
                }
            }

            pbar.Tick($"{(mute ? "Muted" : "Blocked")} @{targetUser.ScreenName}");
            logger.LogMessage(targetUser!.ScreenName);
        }

        private async Task<int> Execute(bool dryRun, string targetUsername, bool log, bool mute)
        {
            if (!CommandHeader("Block Train 🚂", dryRun))
            {
                return -1;
            }

            Writeln(DarkBlue, $"Loading people to {(mute ? "mute" : "block")}, this may take some time...");
            var targets = await GetTargets(targetUsername).ConfigureAwait(false);

            using (var logger = new ThreadedLogger("BlockTrain.log", log))
            using (var pbar = new ProgressBar(targets.Count))
            {
                logger.LogMessage($"# BlockTrain started {DateTime.Now.ToLongDateString()} " +
                    $"{DateTime.Now.ToLongTimeString()}");

                var actions = targets
                        .Select(targetUser => BlockUser(targetUser, dryRun, pbar, logger, mute))
                        .ToArray();

                Task.WaitAll(actions);
            }

            Writeln(Green, $"{(mute ? "Muted" : "Blocked")} a total of {targets.Count} people");
            return 0;
        }
    }
}
