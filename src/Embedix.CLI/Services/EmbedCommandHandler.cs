
namespace Embedix.CLI.Services
{
    public sealed class EmbedCommandHandler
    {
        private readonly IEmbedService _embedService;

        public EmbedCommandHandler(IEmbedService embedService)
        {
            _embedService = embedService ?? throw new ArgumentNullException(nameof(embedService));
        }

        public async Task<int> ExecuteAsync(string? input, string? output, string[] libraries, bool noPdb, bool noOverwrite, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Error.WriteLine("Error: Input path is required.");
                return 2;
            }

            if (libraries is null)
            {
                Console.Error.WriteLine("Error: Libraries cannot be null.");
                return 2;
            }

            string inputPath;

            try
            {
                inputPath = Path.GetFullPath(input);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
            {
                Console.Error.WriteLine($"Error: Invalid input path: {ex.Message}");
                return 2;
            }

            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"Error: Input file was not found: {inputPath}");
                return 2;
            }

            var resolvedLibraries = new List<string>();
            foreach (string library in libraries)
            {
                if (string.IsNullOrWhiteSpace(library))
                {
                    Console.Error.WriteLine("Error: Library path cannot be empty.");
                    return 3;
                }

                string libraryPath;

                try
                {
                    libraryPath = Path.GetFullPath(library);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
                {
                    Console.Error.WriteLine($"Error: Invalid library path: {ex.Message}");
                    return 3;
                }

                if (!File.Exists(libraryPath))
                {
                    Console.Error.WriteLine($"Error: Library was not found: {libraryPath}");
                    return 3;
                }

                resolvedLibraries.Add(libraryPath);
            }

            string? outputPath = null;
            if (!string.IsNullOrWhiteSpace(output))
            {
                try
                {
                    outputPath = Path.GetFullPath(output);
                }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
                {
                    Console.Error.WriteLine($"Error: Invalid output directory: {ex.Message}");
                    return 2;
                }
            }

            var request = new EmbedRequest
            {
                InputPath = inputPath,
                OutputPath = outputPath,
                Libraries = resolvedLibraries,
                AutoResolve = resolvedLibraries.Count == 0,
                WritePdb = !noPdb,
                Overwrite = !noOverwrite
            };

            try
            {
                await _embedService.ExecuteAsync(request, cancellationToken);
                return 0;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
