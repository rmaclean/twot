namespace twot
{
    using System.CommandLine;

    internal interface ICommand
    {
        void AddCommand(Command rootCommand);
    }
}
