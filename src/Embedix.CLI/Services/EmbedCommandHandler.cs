namespace Embedix.CLI.Services
{
    public sealed class EmbedCommandHandler
    {
        private readonly IEmbedService _embedService;

        public EmbedCommandHandler(IEmbedService embedService)
        {
            _embedService = embedService;
        }

        public async Task<int> ExecuteAsync(string? input, string? output, string[] libraries, bool noPdb, bool overwrite, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Error.WriteLine("Error: Input path is required.");
                return 2;
            }

            input = Path.GetFullPath(input);

            if (!File.Exists(input))
            {
                Console.Error.WriteLine($"Error: Input file was not found: {input}");

                return 2;
            }


            var resolvedLibraries = new List<string>();
            foreach (var library in libraries)
            {
                var path = Path.GetFullPath(library);
                if (!File.Exists(path))
                {
                    Console.Error.WriteLine($"Error: Library was not found: {path}");
                    return 3;
                }
                resolvedLibraries.Add(path);
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                output = Path.GetFullPath(output);
                if (string.Equals(input, output, StringComparison.OrdinalIgnoreCase))
                {
                    Console.Error.WriteLine("Error: Input and output paths must be different.");
                    return 2;
                }

                if (File.Exists(output) && !overwrite)
                {
                    Console.Error.WriteLine($"Error: Output already exists. Use --overwrite to replace it: {output}");
                    return 4;
                }
            }


            var request = new EmbedRequest
            {
                InputPath = input,
                OutputPath = output,
                Libraries = resolvedLibraries,
                AutoResolve = resolvedLibraries.Count == 0,
                WritePdb = !noPdb,
                Overwrite = overwrite
            };

            try
            {
                await _embedService.ExecuteAsync(request, cancellationToken);

                return 0;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                Console.Error.WriteLine("Operation cancelled.");
                return 130;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }
    }
}
