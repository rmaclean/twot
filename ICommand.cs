namespace twot
{
    using System.CommandLine;

    interface ICommand
    {
        void AddCommand(Command rootCommand);
    }
}
