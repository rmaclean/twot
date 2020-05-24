namespace twot
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;

    using static CommandHelpers;

#pragma warning disable CA1812
    internal class ScoreCommand : BaseScoreCommand, ICommand
#pragma warning restore CA1812
    {
        public void AddCommand(Command rootCommand)
        {
            var cmd = new Command("Score", "Scores everyone who follows the supplied user and logs them to file. " +
                "This uses the same system as clean, but without the blocking and is useful to tweak your scoring. " +
                "Always outputs to score.log");
            cmd.AddAlias("score");
            cmd.AddAlias("s");

            var minScoreOption = new Option<double>(
                "--score",
                () => 0.85,
                "Sets the score for min kicking. Defaults to 0.85");

            minScoreOption.Name = "minscore";
            minScoreOption.AddAlias("-s");
            cmd.Add(minScoreOption);

            cmd.Handler = CommandHandler.Create<double>(this.Execute);
            rootCommand.Add(cmd);
        }

        private async Task<int> Execute(double minScore)
        {
            if (!CommandHeader("Running Score 🥅"))
            {
                return -1;
            }

            await Run(minScore, true, new ScoreSettings
            {
                mode = "Score",
                onComplete = total => Console.WriteLine($"Scored {total} people who were following you."),
                onUser = (user, logger, score) => logger.LogMessage($"{user!.UserIdentifier.ScreenName}, {score:F2}"),
                beforeRun = logger =>
                {
                    logger.LogMessage($"# Score started {DateTime.Now.ToLongDateString()} " +
                                        $"{DateTime.Now.ToLongTimeString()}");
                },
            });

            return 0;
        }
    }
}
