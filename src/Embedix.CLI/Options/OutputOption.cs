using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class OutputOption : Option<string>
    {
        public OutputOption()
            : base("--output", "-o")
        {
            Description = "Path to the output assembly.";
        }
    }
}
