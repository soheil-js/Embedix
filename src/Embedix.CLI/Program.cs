using Embedix.CLI.Commands;
using Embedix.CLI.Services;
using Serilog;
using System.CommandLine;

ILogger log = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

RootCommand root = new("A modular .NET assembly embedding toolkit with a Core library, GUI, and CLI");
root.Subcommands.Add(new EmbedCommand(new EmbedCommandHandler(new CoreEmbedService(log))));
return await root.Parse(args).InvokeAsync();