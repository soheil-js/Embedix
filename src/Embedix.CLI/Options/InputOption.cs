using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class InputOption : Option<string>
    {
        public InputOption()
            : base("--input", "-i")
        {
            Description = "Path to the input assembly.";
            Required = true;
        }
    }
}
