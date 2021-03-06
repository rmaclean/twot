namespace twot
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;

    using static System.ConsoleColor;
    using static CommandHelpers;
    using static ConsoleHelper;

#pragma warning disable CA1812
    internal class CleanCommand : BaseScoreCommand, ICommand
#pragma warning restore CA1812
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Clean", "Scores everyone who follows the supplied user and if they do not meet a min-score they get blocked and unblocked which forces them to unfollow you.");
            cmd.AddAlias("clean");
            cmd.AddAlias("c");

            var dryRunOption = new Option<bool>("--dryrun", "Does not actually make the changes");
            cmd.Add(dryRunOption);

            var minScoreOption = new Option<double>(
                "--score",
                () => 0.85,
                "Sets the score for min kicking. Defaults to 0.85");

            minScoreOption.Name = "minscore";
            minScoreOption.AddAlias("-s");
            cmd.Add(minScoreOption);

            var logOption = new Option<bool>("--log", "Creates (or appends if the file exists) a log of all the " +
                "accounts impacted by the clean");
            logOption.Name = "log";
            logOption.AddAlias("-l");
            cmd.Add(logOption);

            cmd.Handler = CommandHandler.Create<bool, double, bool>(this.Execute);
            rootCommand.Add(cmd);
        }

        private async Task<int> Execute(bool dryRun, double minScore, bool log)
        {
            if (!CommandHeader("Running clean 🧹", dryRun))
            {
                return -1;
            }

            await Run(minScore, log, new ScoreSettings
            {
                mode = "Clean",
                onComplete = total => Writeln(Green, $"Kicked {total} people who were following you."),
                onUserAsync = async (user) =>
                {
                    if (!dryRun)
                    {
                        await user!.BlockAsync();
                        await user!.UnBlockAsync();
                    }
                },
                onUser = (user, logger, score) => logger.LogMessage(user!.UserIdentifier.ScreenName),
                beforeRun = logger =>
                {
                    logger.LogMessage($"# Clean started {DateTime.Now.ToLongDateString()} " +
                                        $"{DateTime.Now.ToLongTimeString()}");
                    if (dryRun)
                    {
                        logger.LogMessage("# DryRun Mode");
                    }
                },
            });

            return 0;
        }
    }
}
