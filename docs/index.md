# Embedix

**A modern .NET assembly embedding toolkit.**

Embedix embeds managed .NET dependencies into compiled assemblies, compresses them, and loads them directly from memory at runtime without extracting them to disk.

## Features

- **Dependency Embedding:** Automatically detects and embeds referenced assemblies.
- **In-Memory Loading:** Loads embedded assemblies directly from memory.
- **Compression:** Compresses assemblies before embedding.
- **Runtime Injection:** Integrates assembly resolution into the target application.
- **Multiple .NET Versions:** Supports .NET Framework, .NET Core, and modern .NET.

## Projects

- **Embedix.Core:** A reusable core library for developers building their own assembly embedding tools.
- **Embedix.CLI:** The official command-line tool built on Embedix.Core.
- **Embedix.GUI:** A Windows desktop GUI.
- **EmbedixStudio:** A cross-platform desktop GUI for Embedix.

## Getting Started

Install Embedix.Core:

```bash
dotnet add package Embedix.Core
```

[NuGet Package](https://www.nuget.org/packages/Embedix.Core/)

## Documentation

- [**API Reference**](xref:Embedix.Core) — Explore classes, methods, and namespaces.
- [**CLI Usage Guide**](articles/cli.md) — Learn how to use Embedix from the command line.
- [**Build Guide**](articles/build.md) — Learn how to build Embedix for Windows, macOS, and Linux.

## Links

- [GitHub Repository](https://github.com/soheil-js/Embedix)
- [GitHub Releases](https://github.com/soheil-js/Embedix/releases/latest)
- [NuGet Package](https://www.nuget.org/packages/Embedix.Core/)
- [EmbedixStudio](https://github.com/soheil-js/EmbedixStudio)

---

**Coded by Soheil Jashnsaz**