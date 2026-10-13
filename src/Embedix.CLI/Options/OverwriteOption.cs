using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class OverwriteOption : Option<bool>
    {
        public OverwriteOption() : base("--overwrite")
        {
            Description = "Allow replacing an existing output file.";
        }
    }
}
