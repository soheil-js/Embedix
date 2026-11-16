# How Embedix Works

This page explains what Embedix changes inside your assembly, so you know what to expect from the output.

## The idea

A normal .NET application ships with its dependencies as separate `.dll` files. When the runtime cannot find one, it raises the `AppDomain.AssemblyResolve` event. Embedix uses this hook:

1. At **embed time**, each dependency is compressed and stored inside your assembly as a resource.
2. A small resolver class is **copied into your assembly** and registered when the module starts.
3. At **run time**, when the runtime asks for a missing assembly, the resolver reads the resource, decompresses it and loads it from memory.

```text
 Before                          After
 ──────                          ─────
 MyApp.exe                       MyApp.exe
 Newtonsoft.Json.dll      ──▶     ├─ resource: Embedded.Newtonsoft.Json.dll (compressed)
 Serilog.dll                      ├─ resource: Embedded.Serilog.dll (compressed)
                                  └─ type: Embedix.AssemblyResolver (injected)
```

## What is changed in your assembly

| Change | Detail |
| --- | --- |
| New resources | One per dependency, named `Embedded.<file name>` (for example `Embedded.Newtonsoft.Json.dll`). Content is Deflate-compressed. |
| New type | `Embedix.AssemblyResolver`, a small internal class (plus compiler-generated helpers). |
| Startup hook | A call to `AssemblyResolver.Resolve()` inserted at the **beginning of the module static constructor** (`<Module>::.cctor`). The constructor is created if it does not exist. |

Your own code, references and metadata are otherwise preserved (metadata is written with dnlib's `PreserveAll` flags).

## What happens at runtime

When the runtime cannot locate an assembly, the resolver:

1. Returns the assembly if one with the same simple name and culture is **already loaded**.
2. Otherwise reads the resource `Embedded.<AssemblyName>.dll`.
3. Decompresses it and calls `Assembly.Load(byte[])`.

Because the assembly is loaded from a byte array:

- No files are written to disk.
- `Assembly.Location` of the embedded assembly is empty. Code that relies on `Location` (for example to find files next to the DLL) needs to use another base path such as `AppContext.BaseDirectory`.

## Matching runtime payloads

The injected resolver is built for several runtime families. Embedix looks at the core library your assembly references and picks the matching one:

| Your assembly references | Payload used |
| --- | --- |
| `mscorlib` (.NET Framework) | `net45` |
| `netstandard` | `netstandard2.0` |
| `System.Runtime`, `System.Private.CoreLib` or others (modern .NET) | `net6.0` |

## Inputs and outputs

| Item | Behavior |
| --- | --- |
| Input | Never modified. The result is written to a different file. |
| Debug symbols | A PDB is written next to the output unless disabled (`--no-pdb`, or unchecking *Write Pdb*). |
| Companion files | For a `.dll` input with a same-name `.exe` and `.runtimeconfig.json`, those two are copied to the output folder. |
| Signature | The output is **not** re-signed. A previous strong-name or Authenticode signature is no longer valid after modification. |

## What Embedix does not do

- It does not merge dependency code into your assembly (like ILMerge); the dependencies stay separate assemblies, loaded from resources.
- It does not obfuscate or protect your code.
- It does not embed native libraries or support Native AOT.
- It does not change `.deps.json` or other content files.

## Under the hood

Embedix is built on [dnlib](https://github.com/0xd4d/dnlib). Its core contains an injection engine that deep-copies a type (members, attributes, nested types and method bodies) from the payload module into the target module in two phases: first an empty skeleton of every type and member is created, then bodies and references are filled in. This ordering guarantees every reference points to the copied member rather than the original payload assembly.

For the code-level API see [Embedix.Core](core.md).
