# Build Script

`build.py`, in the repository root, builds all Embedix projects in the order required by
the solution's dependencies, and lets you target Windows, macOS or Linux.

## Why a script instead of just `dotnet build`

The projects can't be built independently or in any order:

1. **`Embedix.Core.Payload`** must build first. It's a small project containing just
   `AssemblyResolver.cs`, multi-targeted (`netstandard2.0`, `net45`, `net6.0`) so each
   target framework gets its own correctly-compiled IL.
2. **`Embedix.Core`** builds next. It embeds `Embedix.Core.Payload`'s build output
   (one DLL per target framework) as resources, and picks the matching one at runtime
   based on the target assembly's framework (`mscorlib` → .NET Framework,
   `System.Private.CoreLib`/`System.Runtime` → modern .NET). If `Embedix.Core.Payload`
   hasn't been built yet, this step fails.
3. **`Embedix.CLI`** and **`Embedix.GUI`** build last, in any order, since both depend
   on `Embedix.Core`.

`build.py` encodes this order so you don't have to remember it or get it wrong.

## Requirements

- [.NET SDK](https://dotnet.microsoft.com/download) with `dotnet` on your `PATH`.
- Python 3.10+.
- No extra Python packages — the script only uses the standard library.

## Usage

```bash
python build.py --platform windows
python build.py --platform mac
python build.py --platform linux
```

If you omit `--platform`, it's inferred from the OS you're running the script on.

## Options

| Option | Default | Description |
|---|---|---|
| `--platform` | OS the script runs on | `windows`, `mac` or `linux`. Selects the runtime identifier used for publishing, and whether `Embedix.GUI` is built (see [GUI and platform support](#gui-and-platform-support)). |
| `--configuration` | `Release` | Passed straight to `dotnet build`/`dotnet publish -c`. Typically `Release` or `Debug`. |
| `--root` | The repo root `build.py` lives in | Path to the repository root, if you're running the script from somewhere else or against a different clone. |
| `--publish` | off | Builds `Embedix.CLI`/`Embedix.GUI` with `dotnet publish -r <runtime> --self-contained false` instead of a plain `dotnet build`, producing a distributable, platform-specific output. `Embedix.Core` and `Embedix.Core.Payload` are always built with `dotnet build` — they're libraries, not something you publish. |

Run `python build.py --help` for the same reference from the script itself.

## GUI and platform support

`Embedix.GUI` is built with Windows Forms and only runs on Windows. The script only
builds it when `--platform windows`; for `mac` and `linux` it's skipped automatically,
with a message explaining why. `Embedix.CLI` always builds, on every platform.

For a cross-platform GUI, see [EmbedixStudio](https://github.com/soheil-js/EmbedixStudio).

## Examples

```bash
# Local development build, current OS, Debug
python build.py --configuration Debug

# Release build for Linux, ready to distribute
python build.py --platform linux --publish

# Build against a different checkout of the repo
python build.py --platform windows --root /path/to/other/Embedix
```

## Troubleshooting

- **`BUILD FAILED: 'dotnet' was not found on PATH.`**
  Install the .NET SDK and make sure `dotnet --version` works in your terminal.

- **`BUILD FAILED: Project not found: ...`**
  Either `--root` isn't pointing at the repository root, or the solution's folder
  layout no longer matches the paths hardcoded in `buildtools/config.py`
  (`PAYLOAD_PROJECT`, `CORE_PROJECT`, `CLI_PROJECT`, `GUI_PROJECT`) — update them there
  if you've moved or renamed a project.

- **A step fails partway through.**
  The script stops immediately — it never continues past a failed step, since every
  later step depends on the ones before it succeeding.