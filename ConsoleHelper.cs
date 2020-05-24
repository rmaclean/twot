namespace twot
{
    using System;

    internal static class ConsoleHelper
    {
        internal static void Writeln(ConsoleColor colour, string message)
        {
            var existingColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = existingColour;
        }
    }
}
