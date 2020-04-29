using System.CommandLine;
using System.Threading.Tasks;

namespace twot
{
    interface ICommand
    {
        void AddCommand(Command rootCommand);
    }
}