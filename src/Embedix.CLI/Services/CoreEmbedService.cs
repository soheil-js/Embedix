using dnlib.DotNet;
using Embedix.Core;
using Embedix.Core.dnlib.Loading;

namespace Embedix.CLI.Services
{
    public sealed class CoreEmbedService : IEmbedService
    {
        private readonly Serilog.ILogger _log;

        public CoreEmbedService(Serilog.ILogger logger)
        {
            _log = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task ExecuteAsync(EmbedRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            string? inputPath = null;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (string.IsNullOrWhiteSpace(request.InputPath))
                    throw new ArgumentException("Input path is required.", nameof(request));

                inputPath = Path.GetFullPath(request.InputPath);

                using (Serilog.Context.LogContext.PushProperty("InputAssembly", inputPath))
                {
                    _log.Information("Starting assembly embedding.");
                    _log.Debug("Automatic reference resolution: {AutoResolve}; PDB generation: {WritePdb}.",
                        request.AutoResolve, request.WritePdb);

                    if (!File.Exists(inputPath))
                    {
                        _log.Warning("Input assembly does not exist: {InputPath}", inputPath);
                        throw new FileNotFoundException("Input assembly was not found.", inputPath);
                    }

                    string outputPath = string.IsNullOrWhiteSpace(request.OutputPath)
                        ? Address(inputPath)
                        : Path.GetFullPath(request.OutputPath);

                    if (string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _log.Warning("Input and output paths resolve to the same file: {OutputPath}", outputPath);
                        throw new InvalidOperationException("Input and output paths must be different.");
                    }

                    _log.Information("Output path: {OutputPath}", outputPath);
                    cancellationToken.ThrowIfCancellationRequested();

                    string[] libraries;

                    if (request.AutoResolve)
                    {
                        _log.Information("Resolving assembly references automatically.");
                        List<string> resolvedLibraries = new List<string>();
                        ResolveReferences(inputPath, resolvedLibraries);
                        libraries = resolvedLibraries.ToArray();
                        _log.Information("Automatically resolved {LibraryCount} library/libraries.", libraries.Length);
                    }
                    else
                    {
                        libraries = request.Libraries
                            .Select(Path.GetFullPath)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        _log.Information("Using {LibraryCount} explicitly selected library/libraries.", libraries.Length);
                    }

                    if (libraries.Length == 0)
                    {
                        _log.Warning("No libraries were selected or resolved for embedding.");
                        throw new InvalidOperationException("No libraries selected for embedding.");
                    }

                    foreach (string library in libraries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!File.Exists(library))
                        {
                            _log.Warning("Library file does not exist: {LibraryPath}", library);
                            throw new FileNotFoundException($"Library was not found: {library}", library);
                        }

                        _log.Debug("Library queued for embedding: {LibraryPath}", library);
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    string? outputDirectory = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(outputDirectory))
                    {
                        Directory.CreateDirectory(outputDirectory);
                        _log.Debug("Ensured output directory exists: {OutputDirectory}", outputDirectory);
                    }

                    _log.Information("Loading input assembly.");
                    using (AssemblyLoader assembly = new AssemblyLoader(inputPath))
                    {
                        AssemblyContext context = assembly.GetAssemblyContext();

                        _log.Information("Embedding {LibraryCount} library/libraries.", libraries.Length);
                        Embedder.Execute(context, libraries);
                        _log.Debug("Assembly embedding operation completed in memory.");

                        cancellationToken.ThrowIfCancellationRequested();

                        _log.Information("Writing output assembly. PDB generation: {WritePdb}.", request.WritePdb);
                        assembly.Write(outputPath, request.WritePdb);
                    }

                    _log.Information("Output assembly written successfully: {OutputPath}", outputPath);

                    CopyRuntimeFiles(inputPath, outputPath);

                    _log.Information("Assembly embedding completed successfully.");
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _log.Warning("Assembly embedding was cancelled. Input: {InputPath}", inputPath ?? request.InputPath);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Assembly embedding failed. Input: {InputPath}", inputPath ?? request.InputPath);
                throw;
            }

            return Task.CompletedTask;
        }

        private string Address(string inputPath)
        {
            string directory = Path.GetDirectoryName(inputPath)
                ?? throw new InvalidOperationException("Could not determine the input directory.");

            string outputDirectory = Path.Combine(directory, "Embedix");
            Directory.CreateDirectory(outputDirectory);

            string outputPath = Path.Combine(outputDirectory, Path.GetFileName(inputPath));
            _log.Debug("Using default output path: {OutputPath}", outputPath);

            return outputPath;
        }

        private void CopyRuntimeFiles(string inputPath, string outputPath)
        {
            if (!string.Equals(Path.GetExtension(inputPath), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                _log.Verbose("Skipping runtime sidecar copy because the input is not a DLL.");
                return;
            }

            string inputDirectory = Path.GetDirectoryName(inputPath)!;
            string outputDirectory = Path.GetDirectoryName(outputPath)!;
            string name = Path.GetFileNameWithoutExtension(inputPath);

            string runtimeConfig = Path.Combine(inputDirectory, $"{name}.runtimeconfig.json");
            string executable = Path.Combine(inputDirectory, $"{name}.exe");

            if (!File.Exists(runtimeConfig) || !File.Exists(executable))
            {
                _log.Debug(
                    "Skipping runtime sidecar copy; expected files were not both found. Executable: {ExecutableExists}; Runtime config: {RuntimeConfigExists}.",
                    File.Exists(executable), File.Exists(runtimeConfig));
                return;
            }

            string outputExecutable = Path.Combine(outputDirectory, $"{name}.exe");
            string outputRuntimeConfig = Path.Combine(outputDirectory, $"{name}.runtimeconfig.json");

            _log.Debug("Copying runtime sidecar files to {OutputDirectory}.", outputDirectory);
            File.Copy(executable, outputExecutable, overwrite: true);
            File.Copy(runtimeConfig, outputRuntimeConfig, overwrite: true);
            _log.Debug("Runtime sidecar files copied successfully.");
        }

        private void ResolveReferences(string inputPath, List<string> libraries)
        {
            using AssemblyLoader assembly = new AssemblyLoader(inputPath);
            string? directory = Path.GetDirectoryName(inputPath);
            var references = assembly.ModuleDefMD.GetAssemblyRefs();

            foreach (AssemblyRef assemblyReference in references)
            {
                if (string.IsNullOrEmpty(directory))
                    continue;

                string libraryPath = Path.Combine(directory, $"{assemblyReference.Name}.dll");
                _log.Verbose("Checking reference {ReferenceName} at {ReferencePath}.",
                    assemblyReference.Name, libraryPath);

                if (!File.Exists(libraryPath))
                {
                    _log.Verbose("Reference was not found beside the input assembly: {ReferencePath}", libraryPath);
                    continue;
                }

                libraryPath = Path.GetFullPath(libraryPath);

                if (libraries.Contains(libraryPath, StringComparer.OrdinalIgnoreCase))
                {
                    _log.Verbose("Skipping already resolved reference: {ReferencePath}", libraryPath);
                    continue;
                }

                libraries.Add(libraryPath);
                _log.Debug("Resolved reference: {ReferencePath}", libraryPath);

                ResolveReferences(libraryPath, libraries);
            }
        }
    }
}
