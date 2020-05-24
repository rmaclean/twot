namespace twot
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Reflection;

    public class Program
    {
        public static int Main(string[] args)
        {
            EnableUTFConsole();

            var rootCommand = new RootCommand("Twot: Making Twitter Better");
            foreach (var command in GetCommands())
            {
                command.AddCommand(rootCommand);
            }

            Console.WriteLine();
            return rootCommand.InvokeAsync(args).Result;
        }

        private static IEnumerable<ICommand> GetCommands()
        {
#pragma warning disable SA1009
            return Assembly.GetAssembly(typeof(Program))!.GetTypes()
#pragma warning restore SA1009
                .Where(t => typeof(ICommand).IsAssignableFrom(t))
                .Where(t => t.IsClass)
                .Select(commandInterface => Activator.CreateInstance(commandInterface) as ICommand)
                .Select(commandObject => commandObject!);
        }

        private static void EnableUTFConsole()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Write("\xfeff"); // bom = byte order mark
        }
    }
}
