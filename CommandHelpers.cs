namespace twot
{
    using System;
    using static System.ConsoleColor;
    using static ConsoleHelper;

    internal static class CommandHelpers
    {
        internal static bool CommandHeader(string title, bool dryRun = false)
        {
            Writeln(Cyan, title);
            if (dryRun)
            {
                Writeln(Yellow, " ⚠ Dry run mode");
            }

            if (!Config.Load().Success)
            {
                return false;
            }

            Console.WriteLine();
            return true;
        }
    }
}
