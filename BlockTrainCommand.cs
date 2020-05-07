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

            cmd.Handler = CommandHandler.Create<bool, string>(Execute);
            rootCommand.Add(cmd);
        }

        private async Task Execute(bool dryRun, string targetUsername)
        {
            Console.WriteLine("Block Train 🚂");
            if (dryRun)
            {
                Console.WriteLine("  ⚠ Dry run mode");
            }

            Console.WriteLine($"Blocking @{targetUsername} and everyone who follows them.");

            var me = User.GetAuthenticatedUser();
            var friends = await me.GetFriendsAsync(Int32.MaxValue);

            var target = User.GetUserFromScreenName(targetUsername);
            var enermies = await target.GetFollowersAsync(Int32.MaxValue);

            await target.BlockAsync();

            var targetsForBlock = enermies.Where(enermy => !friends.Contains(enermy));

            foreach (var targetName in targetsForBlock.AsParallel())
            {
                Console.WriteLine(targetName);
                if (!dryRun)
                {
                    User.BlockUser(targetName);
                }
            }

            Console.WriteLine($"Blocked {targetsForBlock.Count()}");
        }
    }
}
