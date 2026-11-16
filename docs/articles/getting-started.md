# Getting Started

Embedix takes a compiled .NET assembly and its managed dependencies and produces **a single assembly that carries its dependencies inside**. The dependencies are compressed, stored as embedded resources and loaded from memory at runtime. Nothing is extracted to disk.

## Choose your tool

| Tool | Platform | Use it when |
| --- | --- | --- |
| [**GUI**](gui.md) | Windows | You want a point-and-click workflow. |
| [**CLI**](cli.md) | Windows, macOS, Linux | You want to script it, run it in CI/CD, or call it from a build. |
| [**Core**](core.md) | Anything that runs .NET | You want to embed the engine in your own tool. |

> Need a cross-platform desktop GUI? See [EmbedixStudio](https://github.com/soheil-js/EmbedixStudio).

## Requirements

- A **compiled** managed .NET assembly (`.exe` or `.dll`) built from C#, VB.NET or F#.
- The managed `.dll` dependencies you want to embed.
- To run the CLI: the **.NET 10 runtime** (the CLI is published framework-dependent).
- To run the GUI: Windows with the .NET 10 Desktop Runtime.

Not supported: native (C/C++) dependencies and Native AOT.

## Download

Prebuilt CLI and GUI archives are attached to every [GitHub Release](https://github.com/soheil-js/Embedix/releases/latest):

| Archive | Contents |
| --- | --- |
| `Embedix.CLI-v<version>-windows-x64.zip` | CLI for Windows |
| `Embedix.CLI-v<version>-mac-x64.zip` | CLI for macOS |
| `Embedix.CLI-v<version>-linux-x64.zip` | CLI for Linux |
| `Embedix.GUI-v<version>-windows-x64.zip` | Windows GUI |
| `Embedix.Core.<version>.nupkg` | The library (also on [NuGet](https://www.nuget.org/packages/Embedix.Core/)) |

Extract the archive anywhere. On Windows run `Embedix.CLI.exe`; on any platform you can run `dotnet Embedix.CLI.dll`.

## Quick start (3 minutes)

### 1. Build your application

```bash
dotnet build -c Release
```

Suppose the output folder `bin/Release/net10.0/` now contains `MyApp.dll`, `Newtonsoft.Json.dll` and other files.

### 2. Run Embedix

```bash
dotnet Embedix.CLI.dll embed -i bin/Release/net10.0/MyApp.dll
```

Because no `-l` option was given, Embedix looks for the DLLs your assembly references **next to the input file** and embeds all it finds. To choose manually:

```bash
dotnet Embedix.CLI.dll embed -i bin/Release/net10.0/MyApp.dll -l bin/Release/net10.0/Newtonsoft.Json.dll
```

The result is written to an `Embedix` folder beside the input (`bin/Release/net10.0/Embedix/`).

### 3. Verify

Copy **only the files from the `Embedix` folder** to an empty directory and run the program there. If it starts, the dependencies are being loaded from the embedded resources.

## Typical project layouts

### .NET Framework executable (`MyApp.exe`)

```powershell
Embedix.CLI.exe embed -i .\bin\Release\MyApp.exe
```

The output `Embedix\MyApp.exe` is self-sufficient.

### Modern .NET application (`MyApp.dll` + `MyApp.exe` apphost)

Give Embedix the **`.dll`**, not the apphost `.exe` (the apphost is a native launcher and cannot be edited as a managed assembly):

```powershell
Embedix.CLI.exe embed -i .\bin\Release\net10.0\MyApp.dll
```

When `MyApp.exe` and `MyApp.runtimeconfig.json` both exist beside the input, the CLI copies them next to the output so that `Embedix\MyApp.exe` works directly. Things to know:

- On Linux and macOS the apphost has no `.exe` extension, so this automatic copy does **not** happen. Copy `MyApp.runtimeconfig.json` to the output yourself and start the app with `dotnet MyApp.dll`.
- Embedix only writes the assembly (and the two companion files above). Content files such as `appsettings.json` must be copied by you.

### Class library (`MyLib.dll`)

Libraries work the same way: the dependencies are embedded into `MyLib.dll`. The resolver is installed in the library's module initializer, so it becomes active as soon as the library is first used.

## Next steps

- [CLI reference](cli.md) and [scripting guide](cli-scripting.md)
- [Using Embedix.Core from code](core.md)
- [GitHub Actions examples](github-actions.md)
- [How it works](how-it-works.md)
- [FAQ and troubleshooting](faq.md)
