# Embedix

**Embed .NET dependencies directly into a compiled assembly — no separate DLLs to ship.**

Embedix compresses your managed dependencies, stores them as embedded resources inside the target assembly, and injects the runtime loading logic needed to resolve them in memory. The result: a single, self-contained assembly.

## ✨ Features

- 📦 Embed managed .NET dependencies into any compiled assembly
- 🗜️ Automatic compression to keep the output small
- 🧠 In-memory resolution — no DLLs extracted to disk at runtime
- 🔍 Automatic dependency detection, or pick libraries manually
- 🖥️ GUI for a point-and-click workflow
- ⌨️ CLI for scripting, CI/CD and automation

## ⚙️ How It Works

1. Pick the compiled assembly you want to process.
2. Select its dependencies — automatically or manually.
3. Embedix compresses them and embeds them as resources.
4. It injects the loading logic needed to resolve and load them at runtime.

You get back a single assembly that carries its own dependencies.

## 🖥️ Use It Your Way

| Interface | Best for |
| --- | --- |
| **GUI** | A visual, desktop workflow (Windows Forms — Windows only) |
| **CLI** | Scripts, automation, CI/CD — see the [CLI Usage Guide](docs/cli.md) |
| **Core** | Build your own tooling on top of the embedding engine |

> Need macOS or Linux? **[EmbedixStudio](https://github.com/soheil-js/EmbedixStudio)** is a cross-platform desktop GUI for Embedix, built with Flutter.

## 🌐 Compatibility

Targets .NET Framework, .NET Core and modern .NET — compiled from C#, VB.NET or F#. Actual support depends on your target runtime and dependencies.

**Not supported:** native (C/C++) dependencies, and Native AOT deployments.

## 🔄 Successor to EmbedLibrary

Embedix continues [EmbedLibrary](https://github.com/soheil-js/EmbedLibrary)'s goal — embedding dependencies into executables — with a more modular architecture. All new development happens here.

## 🛠️ Development

Embedix is developed in C# and uses [dnlib](https://github.com/0xd4d/dnlib) for .NET assembly manipulation.

To build or contribute to the project:

1. Clone the repository.
2. Open the solution in Visual Studio or your preferred .NET development environment.
3. Restore the required dependencies.
4. Build the solution.
5. Run the GUI or CLI project as appropriate.

Refer to the project files for the exact SDK and target framework requirements.

## 📦 Release Builds
 
The projects have a required build order (`Embedix.Core.Payload` → `Embedix.Core` → `Embedix.CLI`/`Embedix.GUI`), so release builds are produced with the included script instead of building manually:
 
```bash
python build.py --platform windows   # or mac / linux
```
 
See the [Build Guide](docs/build.md) for all options.

## 📌 Credits

- [EmbedLibrary](https://github.com/soheil-js/EmbedLibrary) — Embed your .NET dependencies directly into your executable.
- [dnlib](https://github.com/0xd4d/dnlib) — Reads and writes .NET PE assemblies and modules.

## 📄 License

See the [LICENSE](LICENSE) file for the applicable license terms.
