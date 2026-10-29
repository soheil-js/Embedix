using Embedix.Core;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Reflection;

namespace Embedix.CLI.Actions
{
    public sealed class CustomVersionAction : SynchronousCommandLineAction
    {
        public override int Invoke(ParseResult parseResult)
        {
            var cliVersion = (Assembly.GetExecutingAssembly().GetName().Version ?? new Version()).ToString(3);
            var coreVersion = CoreVersion.Get();
            Console.WriteLine($"Embedix CLI v{cliVersion}");
            Console.WriteLine($"Embedix Core v{coreVersion}");
            return 0;
        }
    }
}
