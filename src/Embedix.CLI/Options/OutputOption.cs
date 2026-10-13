using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class OutputOption : Option<string>
    {
        public OutputOption() : base("--output", "-o")
        {
            Description = "Output directory. Defaults to 'Embedix' beside the input assembly.";
        }
    }
}
