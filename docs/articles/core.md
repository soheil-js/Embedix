# Embedix.Core (NuGet)

`Embedix.Core` is the engine behind the CLI and GUI. Use it to build your own tooling: a custom build step, a packaging tool, a plugin builder, and so on.

- Package: [Embedix.Core on NuGet](https://www.nuget.org/packages/Embedix.Core/)
- API reference: [Embedix.Core](xref:Embedix.Core)
- Built on [dnlib](https://github.com/0xd4d/dnlib), which is brought in as a dependency.

## Table of contents

- [Installation](#installation)
- [Concepts](#concepts)
- [Example 1: embed libraries](#example-1-embed-libraries)
- [Example 2: detect dependencies automatically](#example-2-detect-dependencies-automatically)
- [Example 3: a complete command-line tool](#example-3-a-complete-command-line-tool)
- [Example 4: process many assemblies](#example-4-process-many-assemblies)
- [Example 5: work with embedded resources](#example-5-work-with-embedded-resources)
- [Example 6: inject your own type](#example-6-inject-your-own-type)
- [Error handling](#error-handling)
- [Rules and pitfalls](#rules-and-pitfalls)

## Installation

.NET CLI:

```bash
dotnet add package Embedix.Core
```

Package Manager Console:

```powershell
Install-Package Embedix.Core
```

Or edit the project file:

```xml
<ItemGroup>
  <PackageReference Include="Embedix.Core" Version="1.0.3" />
</ItemGroup>
```

The package targets `netstandard2.0`, `net45` and `net6.0`, so it can be used from .NET Framework 4.5+, .NET Core and modern .NET projects. Check [NuGet](https://www.nuget.org/packages/Embedix.Core/) for the latest version.

> Embedix.Core processes assemblies **after** they are compiled. Reference it from a separate tool project, not from the application you are embedding into.

## Concepts

| Type | Namespace | Role |
| --- | --- | --- |
| `AssemblyLoader` | `Embedix.Core.dnlib.Loading` | Opens an assembly from disk and writes it back. Implements `IDisposable`. |
| `AssemblyContext` | `Embedix.Core.dnlib.Loading` | The editing context of a loaded assembly (module, entry point, injector, resources). |
| `Embedder` | `Embedix.Core` | `Embedder.Execute` embeds libraries and wires up the runtime resolver. |
| `ResourceManagement` | `Embedix.Core.dnlib.Resources` | Add, replace and read embedded resources (`context.Resources`). |
| `Injector` | `Embedix.Core.dnlib.Injection` | Copies types from another module into the target assembly. |
| `CoreVersion` | `Embedix.Core` | `CoreVersion.Get()` returns the Core version. |

The usual flow is always: **load → get context → `Embedder.Execute` → write**.

## Example 1: embed libraries

```csharp
using Embedix.Core;
using Embedix.Core.dnlib.Loading;

string input  = @"C:\app\MyApp.dll";
string output = @"C:\out\MyApp.dll";

string[] libraries =
{
    @"C:\app\Newtonsoft.Json.dll",
    @"C:\app\Serilog.dll"
};

using (var loader = new AssemblyLoader(input))
{
    AssemblyContext context = loader.GetAssemblyContext();

    Embedder.Execute(context, libraries);

    loader.Write(output, writePdb: true);
}
```

What `Execute` does:

1. Compresses each library (Deflate) and stores it as a resource named `Embedded.<file name>`, for example `Embedded.Newtonsoft.Json.dll`.
2. Picks the runtime payload that matches the target's core library (.NET Framework, .NET Standard or modern .NET).
3. Merges the payload's `AssemblyResolver` into the target and calls it at the start of the module static constructor.

> The embedded file name must match the **assembly name**: the resolver looks for `Embedded.<AssemblyName>.dll`.

## Example 2: detect dependencies automatically

The CLI and GUI find dependencies by following the assembly references of the input and looking for `<name>.dll` beside it. You can reproduce the same logic:

```csharp
using dnlib.DotNet;
using Embedix.Core.dnlib.Loading;

static List<string> FindReferences(string assemblyPath)
{
    var result = new List<string>();
    Collect(assemblyPath, result);
    return result;
}

static void Collect(string assemblyPath, List<string> result)
{
    string? directory = Path.GetDirectoryName(Path.GetFullPath(assemblyPath));
    if (directory == null) return;

    using var loader = new AssemblyLoader(assemblyPath);

    foreach (AssemblyRef reference in loader.ModuleDefMD.GetAssemblyRefs())
    {
        string candidate = Path.Combine(directory, reference.Name + ".dll");

        if (!File.Exists(candidate)) continue;                       // not beside the input: skip
        if (result.Contains(candidate, StringComparer.OrdinalIgnoreCase)) continue;

        result.Add(candidate);
        Collect(candidate, result);                                  // follow transitive references
    }
}
```

You can then filter the list before embedding, for example to exclude libraries you want to keep as separate files:

```csharp
var libraries = FindReferences(input)
    .Where(path => !Path.GetFileName(path).StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase))
    .ToArray();
```

## Example 3: a complete command-line tool

```csharp
using Embedix.Core;
using Embedix.Core.dnlib.Loading;

if (args.Length < 3)
{
    Console.Error.WriteLine("Usage: mytool <input> <outputDir> <lib1> [lib2 ...]");
    return 2;
}

string input     = Path.GetFullPath(args[0]);
string outputDir = Path.GetFullPath(args[1]);
string[] libs    = args.Skip(2).Select(Path.GetFullPath).ToArray();

string output = Path.Combine(outputDir, Path.GetFileName(input));

if (string.Equals(input, output, StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Output must be different from input.");
    return 2;
}

try
{
    Directory.CreateDirectory(outputDir);

    using var loader = new AssemblyLoader(input);
    var context = loader.GetAssemblyContext();

    Embedder.Execute(context, libs);
    loader.Write(output);

    Console.WriteLine($"Done: {output} (Embedix.Core v{CoreVersion.Get()})");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed: {ex.Message}");
    return 1;
}
```

## Example 4: process many assemblies

Create one `AssemblyLoader` per file and dispose it before moving on. Loaders are not meant to be shared between threads.

```csharp
foreach (string input in Directory.EnumerateFiles(@"C:\plugins", "*.dll"))
{
    string output = Path.Combine(@"C:\plugins-embedded", Path.GetFileName(input));

    using var loader = new AssemblyLoader(input);
    Embedder.Execute(loader.GetAssemblyContext(), new[] { @"C:\shared\Newtonsoft.Json.dll" });
    loader.Write(output, writePdb: false);
}
```

## Example 5: work with embedded resources

`context.Resources` (type `ResourceManagement`) is a small helper over the module's resource list.

```csharp
using var loader = new AssemblyLoader(@"C:\app\MyApp.dll");
var context = loader.GetAssemblyContext();

// Add: from bytes, or from a file (the file name becomes the resource name)
context.Resources.Add("config.default.json", File.ReadAllBytes(@"C:\app\config.json"));
context.Resources.Add(@"C:\app\license.txt");

// Replace: removes the resource with that name and appends the new one.
// Does nothing if no resource has that name.
context.Resources.Replace("config.default.json", new byte[] { 1, 2, 3 });

// Read back: returns null if the resource does not exist.
byte[]? data = context.Resources.GetByteResource("license.txt");
MemoryStream? stream = context.Resources.GetStreamResource("license.txt");

loader.Write(@"C:\out\MyApp.dll");
```

## Bonus: inspect an assembly

```csharp
var context = loader.GetAssemblyContext();

Console.WriteLine(context.Assembly.FullName);
Console.WriteLine(context.EntryPointMethod?.FullName ?? "(no entry point)");

foreach (var reference in context.GetAssemblyRefs())
    Console.WriteLine($"{reference.Name} {reference.Version}");
```

## Example 6: inject your own type

The injection engine copies a type (and its nested types, members, attributes and method bodies) from another module into the target. Embedix uses it to insert the resolver; you can use it for your own helpers.

```csharp
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Embedix.Core.dnlib.Loading;

// 1. Load the module that contains the type to copy.
using ModuleDefMD helpers = ModuleDefMD.Load(@"C:\tools\Helpers.dll");
TypeDef source = helpers.Find("Helpers.StartupHook", isReflectionName: false);

// 2. Load the target and inject.
using var loader = new AssemblyLoader(@"C:\app\MyApp.dll");
AssemblyContext context = loader.GetAssemblyContext();

context.Injector.Inject(source);          // throws InjectionException on failure

// 3. Find the copy and run it from the module static constructor.
TypeDef copy   = context.Module.Find("Helpers.StartupHook", false);
MethodDef init = copy.FindMethod("Init");

context.GlobalTypeStaticConstructor.Body.Instructions
    .Insert(0, Instruction.Create(OpCodes.Call, init));

loader.Write(@"C:\out\MyApp.dll");
```

Other helpers on `AssemblyContext`:

| Member | Description |
| --- | --- |
| `Merge(TypeDef source)` | Injects a type; returns the copy or `null` on failure. |
| `Merge(TypeDef source, string name)` | Injects and renames the copy. |
| `Merge(TypeDef source, string name, string nameSpace)` | Injects and renames name and namespace. |
| `Injector.Inject(IEnumerable<TypeDef>)` | Injects several types that reference each other, in one batch. |
| `Injector.TryInject(source, out copy)` | Non-throwing variant. |

Prefer `Inject` when you want to know *why* something failed; `Merge` and `TryInject` return `null` / `false` without details.

## Error handling

| Operation | Typical failure |
| --- | --- |
| `new AssemblyLoader(path)` | `FileNotFoundException`. Thrown when the file is missing **or cannot be loaded as a managed module** (for example a native DLL or an apphost executable). |
| `Embedder.Execute` | `FileNotFoundException`, `IOException` and similar if a library path is invalid; `InvalidOperationException` if the payload resource is missing. |
| `loader.Write` | `FileNotFoundException` with a generic message if the module cannot be saved. |
| `Injector.Inject` | `InjectionException` (inspect `InnerException`). |

Always validate your inputs (existence, extension, input != output) before calling the library, and wrap the work in `try/catch` as in Example 3.

## Rules and pitfalls

- **Write to a different path** than the input; the input file stays open until the loader is disposed.
- **Do not embed the same file name twice** and do not run Embedix on an assembly it has already processed. Always start from your original build output.
- **Order matters:** call `Embedder.Execute` once, then `Write`.
- **Strong-named assemblies:** the output is not re-signed. Re-sign it with your own tooling if you need a valid strong name.
- **Do not embed native libraries.** Only managed assemblies are supported.
- `Embedder.Execute` expects the parameter name `librarys` if you use named arguments (the spelling is part of the current API).
