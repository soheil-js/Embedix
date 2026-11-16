# FAQ and Troubleshooting

## General

### How do I know embedding worked?
Copy **only** the files from the `Embedix` output folder to a clean directory (without the original DLLs) and run the program. You can also open the output in dnSpy or ILSpy and check for `Embedded.*` resources and the `Embedix.AssemblyResolver` type.

### Which assemblies can I embed?
Managed .NET `.dll` files. Native libraries (C/C++) are not supported and must stay beside the application.

### Does it work with .NET Core / .NET 5+ / .NET Framework?
Yes. The tool selects a runtime payload based on the assembly's core library (see [How it works](how-it-works.md)). Test your final output on the runtime versions you target, especially older ones.

### Does it make my application smaller?
The embedded libraries are Deflate-compressed, so the single file is usually smaller than the sum of the originals, but the exact result depends on the libraries.

### Is my code protected or obfuscated?
No. Embedix is not an obfuscator.

### Can I run Embedix twice on the same file?
Do not. Always start from the original build output; embedding twice can create duplicate resources and a duplicate resolver.

## Errors

### `Error for load module` / `FileNotFoundException` while loading the input
The file is missing or cannot be read as a managed module. For a modern .NET application, pass the **`.dll`** rather than the native apphost `.exe`.

### `No libraries selected for embedding`
Automatic resolution found no DLLs next to the input file. Make sure your build output folder contains the dependencies, or specify them with `-l`.

### Exit code 2 or 3
`2` means a problem with the input file or output path; `3` means a path passed with `-l` is empty or missing. See [exit codes](cli.md#exit-codes).

### The result throws `FileNotFoundException` for a library on another machine
- Check that the library was actually embedded (look for the `Embedded.<name>.dll` resource).
- The resource name must match the assembly's *name*, so do not rename the DLL before embedding.
- Confirm the library is managed and not a native dependency of another package.

### The embedded app starts, but a library that uses native code fails
Native DLLs cannot be embedded. Keep them next to the executable.

### My app can't find its `.runtimeconfig.json` on Linux/macOS
The companion files are copied only when both `<name>.exe` and `<name>.runtimeconfig.json` exist beside the input, which is a Windows layout. Copy the runtimeconfig manually and run with `dotnet MyApp.dll`.

### `Assembly.Location` is empty for a library
Embedded assemblies are loaded from memory and have no file location. Use `AppContext.BaseDirectory` for file paths.

### Strong-name or signature errors
Modifying an assembly invalidates its strong-name and Authenticode signatures. Re-sign the output with your own tooling after embedding.

### Antivirus flags the result
Loading assemblies from memory is a pattern some security products inspect. Signing your final binary and submitting it for analysis usually resolves false positives.

## Build and CI

### Do I need .NET 10 to run the CLI?
Yes, the CLI is published framework-dependent for .NET 10. The application you embed into can target a different framework.

### Where does the output go?
By default to an `Embedix` folder beside the input. Use `-o <dir>` to change it and `--no-overwrite` to avoid reusing an existing folder. See the [CLI guide](cli.md).

### How do I use it in CI?
See [GitHub Actions examples](github-actions.md) and the [scripting guide](cli-scripting.md).

### How do I build Embedix from source?
See the [Build Guide](build.md). Build order matters: the payload project must be built before `Embedix.Core`; `python build.py` handles it.

### I found a bug or want a feature
Open an issue at [github.com/soheil-js/Embedix/issues](https://github.com/soheil-js/Embedix/issues) with your target framework, the command you ran and the full error message.
