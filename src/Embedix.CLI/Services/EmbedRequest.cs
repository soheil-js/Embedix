namespace Embedix.CLI.Services
{
    public sealed class EmbedRequest
    {
        public required string InputPath { get; init; }
        public string? OutputPath { get; init; }
        public IReadOnlyList<string> Libraries { get; init; } = Array.Empty<string>();
        public bool AutoResolve { get; init; }
        public bool WritePdb { get; init; } = true;
        public bool Overwrite { get; init; } = true;
    }
}
