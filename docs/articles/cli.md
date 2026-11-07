# Embedix CLI

Embedix CLI is the command-line interface for embedding managed .NET library assemblies into a compiled .NET assembly.

## Table of Contents

- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Command and Options](#command-and-options)
- [Usage Examples](#usage-examples)
- [Dependency Resolution](#dependency-resolution)
- [Output Behavior](#output-behavior)
- [CI/CD Example](#cicd-example)
- [Exit Codes](#exit-codes)
- [Logging and Errors](#logging-and-errors)
- [Limitations and Notes](#limitations-and-notes)

## Requirements

- A compiled managed .NET assembly that Embedix can process.
- The Embedix CLI executable and its required runtime files.
- The managed library assemblies you want to embed, unless you use automatic dependency resolution.

The examples below use PowerShell. Run them from the directory containing the CLI executable, or replace `.\Embedix.CLI.exe` with the path to your executable. If your build produces an executable without the `.exe` extension, use that filename instead.

## Getting Started

### Show general help

```powershell
.\Embedix.CLI.exe --help
```

### Show `embed` command help

```powershell
.\Embedix.CLI.exe embed --help
```

### Show the CLI version

```powershell
.\Embedix.CLI.exe --version
```

## Command and Options

The `embed` command processes an input assembly:

```powershell
.\Embedix.CLI.exe embed [options]
```

| Option | Description |
| --- | --- |
| `-i`, `--input <input>` | **Required.** Path to the input assembly. |
| `-o`, `--output <output>` | Output directory. If omitted, the CLI uses an `Embedix` directory beside the input assembly. |
| `-l`, `--libraries <libraries>` | One or more library paths to embed. Multiple paths can be supplied after one `-l` or `--libraries` option. If omitted, automatic dependency resolution is used. |
| `--no-pdb` | Disable PDB generation. PDB generation is enabled by default. |
| `--no-overwrite` | Do not reuse an existing output directory. Select the first available numbered directory instead. |
| `-?`, `-h`, `--help` | Display help for the command. |

`--version` is a global option; use it as shown above.

Paths supplied to the CLI are normalized with `Path.GetFullPath`, so relative input, library, and output paths are resolved from the process's current working directory.

## Usage Examples

Replace sample filenames and paths with paths from your environment.

### 1. Embed an assembly using automatic resolution

```powershell
.\Embedix.CLI.exe embed --input ".\MyApp.dll"
```

Because no libraries are supplied, Embedix attempts to resolve referenced DLLs automatically. The output is written to an `Embedix` directory beside `MyApp.dll`. PDB generation and output overwriting are enabled by default.

### 2. Use the short input option

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll"
```

This is equivalent to the previous command.

### 3. Choose an output directory

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll" --output ".\artifacts\Release"
```

The output filename is based on the input filename, so this example writes `MyApp.dll` into the selected output directory. `--output` accepts a **directory**, not an output filename.

A relative output path is resolved from the current working directory, not automatically from the input assembly's directory.

### 4. Use an absolute output directory

```powershell
.\Embedix.CLI.exe embed -i "D:\Projects\MyApp.dll" -o "D:\Build\Release"
```

The output assembly path is:

```text
D:\Build\Release\MyApp.dll
```

### 5. Specify one library explicitly

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll" -l ".\Newtonsoft.Json.dll"
```

When at least one library is supplied, Embedix embeds the explicitly supplied libraries and does not run automatic dependency resolution.

### 6. Specify multiple libraries

Supply multiple library paths after a single `-l` or `--libraries` option:

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll" -l ".\Newtonsoft.Json.dll" ".\Serilog.dll"
```

The long option works the same way:

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll" --libraries ".\Newtonsoft.Json.dll" ".\Serilog.dll"
```

Check `.\Embedix.CLI.exe embed --help` if the option syntax differs in your installed build.

### 7. Disable PDB generation

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll" --no-pdb
```

### 8. Avoid reusing an existing output directory

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll" --no-overwrite
```

If the default `Embedix` directory already exists, Embedix selects the first unused numbered directory, such as `Embedix1`, `Embedix2`, and so on. If you specify `Release` and that directory already exists, alternatives may be named `Release1`, `Release2`, and so on.

Without `--no-overwrite`, the selected output directory is reused and existing output files may be overwritten.

### 9. Combine options

```powershell
.\Embedix.CLI.exe embed `
    --input ".\MyApp.dll" `
    --output ".\artifacts\Release" `
    --no-pdb `
    --no-overwrite
```

This chooses the output directory, disables PDB generation, and avoids reusing an existing output directory.

## Dependency Resolution

Embedix supports two modes:

- **Automatic resolution:** If no library paths are supplied, Embedix examines the input assembly's references and searches for matching `.dll` files beside the input assembly. When it finds a referenced DLL, it recursively examines that DLL's references as well.
- **Explicit selection:** If one or more library paths are supplied through `-l` or `--libraries`, only those supplied paths are selected for embedding; automatic resolution is disabled.

Automatic resolution is limited: it searches beside each assembly being examined. References that cannot be found there are skipped. It does not establish that every runtime dependency has been discovered, and it does not search arbitrary directories for missing references.

At least one library must be selected or resolved. If the resulting library list is empty, processing fails.

## Output Behavior

- **Default output directory:** `Embedix`, beside the input assembly.
- **Output filename:** The input assembly's filename is retained.
- **Relative `--output` path:** Resolved from the CLI process's current working directory.
- **Overwrite enabled by default:** The requested output directory is reused.
- **`--no-overwrite`:** If the requested directory or a file with that name already exists, Embedix selects a numbered alternative. It does not overwrite the existing directory as its output destination.
- **PDB generation:** Enabled by default; `--no-pdb` disables it.

### Runtime companion files

When the input assembly is a `.dll`, Embedix copies companion files only if **both** of these files exist beside the input DLL:

- A same-name `.exe` file.
- A same-name `.runtimeconfig.json` file.

Both are copied to the selected output directory. If the input is not a DLL, or either companion file is missing, this copy step is skipped. These companion files are copied with overwrite enabled inside the selected output directory.

## CI/CD Example

This PowerShell example can be adapted for a build or release pipeline:

```powershell
.\Embedix.CLI.exe embed `
    --input ".\bin\Release\net10.0\MyApp.dll" `
    --output ".\artifacts\Embedix" `
    --no-pdb `
    --no-overwrite

if ($LASTEXITCODE -ne 0) {
    throw "Embedix CLI failed with exit code $LASTEXITCODE."
}
```

The input must exist in the pipeline's working environment. Check `$LASTEXITCODE` immediately after the CLI process finishes to determine whether it succeeded.

## Exit Codes

The current command handler returns these exit codes:

| Exit code | Meaning |
| ---: | --- |
| `0` | The embedding operation completed successfully. |
| `1` | Processing failed with an exception, including failures during embedding or output writing. |
| `2` | Input is missing or invalid, the input file does not exist, or output-path normalization fails. |
| `3` | A library path is empty or invalid, or a specified library file does not exist. |
| `130` | The operation was cancelled through the cancellation token. |

For example, inspect the exit code in PowerShell:

```powershell
.\Embedix.CLI.exe embed -i ".\MyApp.dll"
$LASTEXITCODE
```

A missing input file returns `2`; a missing explicitly supplied library returns `3`. A processing failure such as resolving no libraries automatically is handled as an exception and returns `1`.

## Logging and Errors

The command handler writes input/library validation errors and cancellation messages to standard error. The embedding service emits structured log events through Serilog; where those log events appear depends on the CLI's Serilog configuration.

## Limitations and Notes

- Input and library files must be managed assemblies that Embedix can process.
- Automatic resolution only searches beside the input assembly and the recursively discovered DLLs; missing references are skipped.
- Explicit library selection disables automatic dependency resolution.
- Embedding managed libraries does not automatically embed native C/C++ DLLs.
- Runtime compatibility depends on the target assembly, its dependencies, and the injected loading logic.
- Use `.\Embedix.CLI.exe --help` and `.\Embedix.CLI.exe embed --help` to confirm the options exposed by your current build.
