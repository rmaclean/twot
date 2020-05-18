namespace twot
{
    using System;
    class ConsoleHelper
    {
        public static void Writeln(ConsoleColor colour, string message)
        {
            var existingColour = Console.ForegroundColor;
            Console.ForegroundColor = colour;
            Console.WriteLine(message);
            Console.ForegroundColor = existingColour;
        }
    }
}