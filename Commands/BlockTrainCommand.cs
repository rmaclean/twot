namespace twot
{
    using System.Linq;
    using System;
    using System.Threading.Tasks;
    using Tweetinvi;
    using System.CommandLine;
    using System.CommandLine.Invocation;

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

        private async Task Execute(bool dryRun, string targetUsername, bool log)
        {
            Console.WriteLine("Block Train 🚂");
            if (dryRun)
            {
                Console.WriteLine(" ⚠ Dry run mode");
            }

            Console.WriteLine();

            var me = User.GetAuthenticatedUser();
            var friends = await me.GetFriendsAsync(Int32.MaxValue);

            var target = User.GetUserFromScreenName(targetUsername);
            var enermies = await target.GetFollowersAsync(Int32.MaxValue);

            var targetsForBlock = enermies.Where(enermy => !friends.Contains(enermy));

            using (var logger = new ThreadedLogger("BlockTrain.log", log))
            using (var pbar = new ProgressBar(enermies.Count() + 1))
            {
                logger.LogMessage($"# BlockTrain started {DateTime.Now.ToLongDateString()} " +
                    $"{DateTime.Now.ToLongTimeString()}");

                await target.BlockAsync();
                pbar.Tick($"Blocked @{targetUsername}");
                logger.LogMessage(targetUsername);

                foreach (var targetUser in targetsForBlock.AsParallel())
                {
                    if (!dryRun)
                    {
                        await targetUser!.BlockAsync();
                    }

                    pbar.Tick($"Blocked @{targetUser.ScreenName}");
                    logger.LogMessage(targetUser!.ScreenName);
                }
            }

            Console.WriteLine($"Blocked a total of {targetsForBlock.Count() + 1 } people");
        }
    }
}
