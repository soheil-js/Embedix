using Embedix.CLI.Options;
using Embedix.CLI.Services;
using System.CommandLine;

namespace Embedix.CLI.Commands
{
    public sealed class EmbedCommand : Command
    {
        public EmbedCommand(EmbedCommandHandler handler) : base("embed", "Embed .NET assemblies into an executable.")
        {
            var inputOption = new InputOption();
            var outputOption = new OutputOption();
            var libraryOption = new LibraryOption();
            var noPdbOption = new NoPdbOption();
            var overwriteOption = new OverwriteOption();

            Options.Add(inputOption);
            Options.Add(outputOption);
            Options.Add(libraryOption);
            Options.Add(noPdbOption);
            Options.Add(overwriteOption);

            SetAction(async (parseResult, cancellationToken) =>
            {
                var input = parseResult.GetValue(inputOption);
                var output = parseResult.GetValue(outputOption);
                var libraries = parseResult.GetValue(libraryOption) ?? Array.Empty<string>();
                var noPdb = parseResult.GetValue(noPdbOption);
                var overwrite = parseResult.GetValue(overwriteOption);

                return await handler.ExecuteAsync(
                    input,
                    output,
                    libraries,
                    noPdb,
                    overwrite,
                    cancellationToken);
            });
        }
    }
}
