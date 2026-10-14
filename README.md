# Embedix

**A modular .NET assembly embedding toolkit with a Core library, GUI and CLI.**

## ✨ Features

- 📦 **Assembly Embedding** — Embed managed .NET dependencies into a target assembly.
- 🗜️ **Compression** — Compress embedded assemblies to reduce their resource size.
- 🧠 **In-Memory Loading** — Resolve and load embedded managed assemblies without extracting them to disk.
- 🔍 **Automatic Dependency Detection** — Detect referenced libraries automatically.
- 🎯 **Manual Library Selection** — Choose which dependencies to embed.
- 🖥️ **Graphical Interface** — Process assemblies through a desktop GUI.
- ⌨️ **Command-Line Interface** — Run assembly embedding from a terminal or automation script.
- 🧩 **Modular Architecture** — Separate the core functionality from the GUI and CLI.
- 🔧 **Post-Build Processing** — Process compiled assemblies without requiring a dedicated build-time integration.

## 🏗️ Architecture

Embedix separates its core assembly-processing functionality from its user-facing applications.

| Component | Purpose |
|---|---|
| **Embedix.Core** | Core assembly-processing and embedding functionality |
| **Embedix.GUI** | Graphical interface for configuring and executing embedding operations |
| **Embedix.CLI** | Command-line interface for scripting and automated workflows |

The Core library contains the underlying assembly manipulation logic, including code injection, member handling, assembly loading and related utilities. The GUI and CLI provide different ways to access this functionality.

### Project Structure

The repository is organized around the following components:

```text
Embedix/
├── src/
│   ├── Embedix.Core/
│   │   └── dnlib/
│   │       ├── Injection/
│   │       ├── Loading/
│   │       └── Resources/
│   ├── Embedix.GUI/
│   │   ├── Utility/
│   │   └── View/
│   └── Embedix.CLI/
│       ├── Commands/
│       ├── Options/
│       └── Services/
├── README.md
├── .gitignore
└── LICENSE
```

*Note: This is a high-level representation of the project layout. Refer to the repository for the complete and current directory structure.*

## ⚙️ How It Works

Embedix operates on compiled .NET assemblies rather than source code.

1. **Select the Target Assembly** — Choose the compiled executable or assembly to process.
2. **Identify Dependencies** — Select the target's dependencies manually or let Embedix detect them automatically.
3. **Compress Dependencies** — Compress the selected managed assemblies before embedding them.
4. **Embed Dependencies as Resources** — Store the compressed assemblies as embedded resources in the target assembly.
5. **Inject Runtime Loading Logic** — Inject the required loading logic into the target assembly so it can resolve and load the embedded dependencies into memory at runtime.

The result is a modified assembly that can load and use its embedded managed dependencies at runtime without extracting those DLLs to disk.

## 🖥️ Available Interfaces

### GUI

The graphical interface provides a desktop workflow for selecting an input assembly, configuring dependency selection and running the embedding process.

It is suitable for developers who prefer working with a graphical application instead of command-line arguments.

### CLI

The command-line interface allows you to embed managed .NET dependencies into compiled assemblies and automate the process through scripts and CI/CD pipelines.

Features include automatic dependency resolution, manual library selection, configurable output directories, optional PDB generation and output overwrite control.

For installation instructions, available options, command examples and usage details, see the [CLI Usage Guide](docs/cli.md).

### Core Library

The Core component contains the reusable assembly-processing functionality used by the application interfaces.

Keeping this logic separate makes it easier to maintain the embedding engine independently of the GUI and CLI and provides a foundation for additional interfaces or integrations.

## 🌐 Compatibility

Embedix is designed for compiled, managed .NET assemblies.

### Target Frameworks

- **.NET Framework**
- **.NET Core**
- **Modern .NET**

Actual compatibility depends on the target runtime, the injected loading logic and the embedded dependencies. Not every framework version or deployment configuration is necessarily supported.

### Supported Assembly Types

Embedix is intended for managed .NET assemblies compiled from languages such as C#, Visual Basic .NET and F#, subject to runtime and dependency compatibility.

Target architectures such as `Any CPU`, `x86`, `x64` and `ARM64` depend on the target application, its runtime and the compatibility of its dependencies.

### Limitations

- **Managed Assemblies Only:** Native C/C++ DLLs cannot be loaded through the same managed assembly-resolution mechanism. Native dependencies may require separate handling.
- **Native AOT:** Native AOT deployments are not supported by the dynamic managed-assembly loading approach.

## 🔄 EmbedLibrary Migration

Embedix is the successor to [EmbedLibrary](https://github.com/soheil-js/EmbedLibrary).

The original project focused on embedding managed .NET dependencies into executables. Embedix continues that goal with a more modular architecture, separating the core implementation from its graphical and command-line interfaces.

New development takes place in Embedix.

## 🛠️ Development

Embedix is developed in C# and uses [dnlib](https://github.com/0xd4d/dnlib) for .NET assembly manipulation.

To build or contribute to the project:

1. Clone the repository.
2. Open the solution in Visual Studio or your preferred .NET development environment.
3. Restore the required dependencies.
4. Build the solution.
5. Run the GUI or CLI project as appropriate.

Refer to the project files for the exact SDK and target framework requirements.

## 📌 Credits

- [EmbedLibrary](https://github.com/soheil-js/EmbedLibrary) — Embed your .NET dependencies directly into your executable.
- [dnlib](https://github.com/0xd4d/dnlib) — Reads and writes .NET PE assemblies and modules.

## 📄 License

See the [LICENSE](LICENSE) file for the applicable license terms.