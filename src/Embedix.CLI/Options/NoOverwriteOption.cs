using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class NoOverwriteOption : Option<bool>
    {
        public NoOverwriteOption() : base("--no-overwrite")
        {
            Description = "Create a numbered output directory instead of overwriting existing output.";
        }
    }
}
