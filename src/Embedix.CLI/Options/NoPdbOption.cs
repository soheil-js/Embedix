using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class NoPdbOption : Option<bool>
    {
        public NoPdbOption() : base("--no-pdb")
        {
            Description = "Disable PDB generation.";
        }
    }
}
