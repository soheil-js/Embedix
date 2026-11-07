# Building Documentation Locally

Embedix uses [DocFX](https://dotnet.github.io/docfx/) to generate its documentation.

## Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- Git

## Setup

Clone the repository:

```bash
git clone https://github.com/soheil-js/Embedix.git
cd Embedix
```

Restore the local .NET tools:

```bash
dotnet tool restore
```

## Build Documentation

Before generating the documentation, follow the [Build Guide](build.md) to compile the required projects.

Once the build is complete, generate the API metadata:

```bash
dotnet tool run docfx metadata docs/docfx.json
```

Build the documentation website:

```bash
dotnet tool run docfx build docs/docfx.json
```

The generated website will be available in:

```text
docs/_site/
```

## Preview Documentation

Start the local DocFX server:

```bash
dotnet tool run docfx serve docs/_site
```

Open the local URL displayed in the terminal to preview the documentation.

Press `Ctrl+C` to stop the server.

## Clean Generated Files

To remove generated documentation files before rebuilding:

```powershell
Remove-Item docs/_site, docs/api -Recurse -Force -ErrorAction SilentlyContinue
```

Then repeat the build steps.

## Notes

- The generated API metadata is stored in `docs/api/`.
- The generated website is stored in `docs/_site/`.
- These directories are generated automatically and should not be committed to the repository.