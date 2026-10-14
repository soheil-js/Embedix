using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class LibrariesOption : Option<string[]>
    {
        public LibrariesOption() : base("--libraries", "-l")
        {
            Description = "Path to one or more libraries to embed.";
            AllowMultipleArgumentsPerToken = true;
        }
    }
}
