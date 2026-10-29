using Embedix.CLI.Actions;
using Embedix.CLI.Commands;
using Embedix.CLI.Services;
using Serilog;
using System.CommandLine;

ILogger log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

RootCommand root = new("Embed .NET assembly dependencies.");
root.Subcommands.Add(new EmbedCommand(new EmbedCommandHandler(new CoreEmbedService(log))));
var versionOption = root.Options.OfType<VersionOption>().Single();
versionOption.Aliases.Add("-v");
versionOption.Action = new CustomVersionAction();
return await root.Parse(args).InvokeAsync();