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
                    _log.Debug(
                        "Options: AutoResolve={AutoResolve}; WritePdb={WritePdb}; Overwrite={Overwrite}; Output={RequestedOutput}.",
                        request.AutoResolve,
                        request.WritePdb,
                        request.Overwrite,
                        string.IsNullOrWhiteSpace(request.OutputPath) ? "<default>" : request.OutputPath);

                    if (!File.Exists(inputPath))
                    {
                        _log.Warning("Input assembly does not exist: {InputPath}", inputPath);
                        throw new FileNotFoundException("Input assembly was not found.", inputPath);
                    }

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
                        if (request.Libraries is null)
                            throw new InvalidOperationException("The library collection cannot be null when automatic resolution is disabled.");

                        libraries = request.Libraries
                            .Where(path => !string.IsNullOrWhiteSpace(path))
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

                    // Select the destination only after validating the input and all libraries.
                    // The directory is created immediately before writing, so early failures
                    // do not leave an empty output directory behind.
                    string outputDirectory = GetOutputDirectory(inputPath, request.OutputPath, request.Overwrite);
                    string outputPath = Path.Combine(outputDirectory, Path.GetFileName(inputPath));

                    if (string.Equals(inputPath, outputPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _log.Warning("Input and output paths resolve to the same file: {OutputPath}", outputPath);
                        throw new InvalidOperationException("Input and output paths must be different.");
                    }

                    using (Serilog.Context.LogContext.PushProperty("OutputDirectory", outputDirectory))
                    using (Serilog.Context.LogContext.PushProperty("OutputAssembly", outputPath))
                    {
                        _log.Information("Selected output directory: {OutputDirectory}", outputDirectory);
                        _log.Information("Output assembly path: {OutputPath}", outputPath);
                        _log.Debug("Overwrite existing output: {Overwrite}", request.Overwrite);

                        cancellationToken.ThrowIfCancellationRequested();

                        _log.Information("Loading input assembly.");
                        using (AssemblyLoader assembly = new AssemblyLoader(inputPath))
                        {
                            AssemblyContext context = assembly.GetAssemblyContext();

                            _log.Information("Embedding {LibraryCount} library/libraries.", libraries.Length);
                            Embedder.Execute(context, libraries);
                            _log.Debug("Assembly embedding operation completed in memory.");

                            cancellationToken.ThrowIfCancellationRequested();

                            Directory.CreateDirectory(outputDirectory);
                            _log.Debug("Output directory is ready: {OutputDirectory}", outputDirectory);

                            _log.Information("Writing output assembly. PDB generation: {WritePdb}.", request.WritePdb);
                            assembly.Write(outputPath, request.WritePdb);
                        }

                        _log.Information("Output assembly written successfully.");

                        cancellationToken.ThrowIfCancellationRequested();
                        CopyRuntimeFiles(inputPath, outputPath);

                        _log.Information("Assembly embedding completed successfully.");
                    }
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

        private string GetOutputDirectory(string inputPath, string? requestedOutputPath, bool overwrite)
        {
            string inputDirectory = Path.GetDirectoryName(inputPath)
                ?? throw new InvalidOperationException("Could not determine the input directory.");

            string baseDirectory = string.IsNullOrWhiteSpace(requestedOutputPath)
                ? Path.Combine(inputDirectory, "Embedix")
                : Path.IsPathRooted(requestedOutputPath)
                    ? Path.GetFullPath(requestedOutputPath)
                    : Path.GetFullPath(Path.Combine(inputDirectory, requestedOutputPath));

            if (File.Exists(baseDirectory))
                throw new IOException($"The output path points to a file, not a directory: {baseDirectory}");

            if (overwrite)
            {
                _log.Debug("Overwrite is enabled; using the requested output directory: {OutputDirectory}", baseDirectory);
                return baseDirectory;
            }

            string candidate = baseDirectory;
            string parentDirectory = Path.GetDirectoryName(baseDirectory)
                ?? throw new InvalidOperationException("Could not determine the parent output directory.");
            string directoryName = Path.GetFileName(baseDirectory);
            int index = 1;

            while (Directory.Exists(candidate) || File.Exists(candidate))
            {
                candidate = Path.Combine(parentDirectory, $"{directoryName}{index}");
                index++;
            }

            _log.Debug("Overwrite is disabled; selected the first unused output directory: {OutputDirectory}", candidate);
            return candidate;
        }

        private void CopyRuntimeFiles(string inputPath, string outputPath)
        {
            if (!string.Equals(Path.GetExtension(inputPath), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                _log.Verbose("Skipping runtime sidecar copy because the input is not a DLL.");
                return;
            }

            string inputDirectory = Path.GetDirectoryName(inputPath)
                ?? throw new InvalidOperationException("Could not determine the input directory for runtime files.");
            string outputDirectory = Path.GetDirectoryName(outputPath)
                ?? throw new InvalidOperationException("Could not determine the output directory for runtime files.");
            string name = Path.GetFileNameWithoutExtension(inputPath);

            string runtimeConfig = Path.Combine(inputDirectory, $"{name}.runtimeconfig.json");
            string executable = Path.Combine(inputDirectory, $"{name}.exe");
            bool executableExists = File.Exists(executable);
            bool runtimeConfigExists = File.Exists(runtimeConfig);

            if (!executableExists || !runtimeConfigExists)
            {
                _log.Debug(
                    "Skipping runtime sidecar copy because both companion files are required. Executable exists: {ExecutableExists}; runtime config exists: {RuntimeConfigExists}.",
                    executableExists,
                    runtimeConfigExists);
                return;
            }

            string outputExecutable = Path.Combine(outputDirectory, $"{name}.exe");
            string outputRuntimeConfig = Path.Combine(outputDirectory, $"{name}.runtimeconfig.json");

            _log.Information("Copying runtime companion files to {OutputDirectory}.", outputDirectory);
            File.Copy(executable, outputExecutable, overwrite: true);
            _log.Debug("Copied runtime executable: {OutputExecutable}", outputExecutable);

            File.Copy(runtimeConfig, outputRuntimeConfig, overwrite: true);
            _log.Debug("Copied runtime configuration: {OutputRuntimeConfig}", outputRuntimeConfig);
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
                _log.Verbose(
                    "Checking reference {ReferenceName} at {ReferencePath}.",
                    assemblyReference.Name,
                    libraryPath);

                if (!File.Exists(libraryPath))
                {
                    _log.Verbose("Reference was not found beside the current assembly: {ReferencePath}", libraryPath);
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
