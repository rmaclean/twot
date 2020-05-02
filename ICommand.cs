using System.CommandLine;

namespace twot
{
    interface ICommand
    {
        void AddCommand(Command rootCommand);
    }
}