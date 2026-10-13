using System.CommandLine;

namespace Embedix.CLI.Options
{
    public sealed class LibraryOption : Option<string[]>
    {
        public LibraryOption()
            : base("--library", "-l")
        {
            Description = "Path to one or more libraries to embed.";
            AllowMultipleArgumentsPerToken = true;
        }
    }
}
