# CLI Scripting Guide

This guide shows how to call [Embedix CLI](cli.md) from scripts and build systems. The full option reference is in the [CLI Usage Guide](cli.md).

## Golden rules for scripts

1. **Use absolute paths.** A relative `--output` is resolved from the process's *current working directory*, not from the input file.
2. **Always check the exit code.** `0` means success; `1` processing error; `2` input problem; `3` library problem; `130` cancelled. See [exit codes](cli.md#exit-codes).
3. **Start from the original build output.** Do not feed an already embedded file back into Embedix.
4. **Quote paths** that may contain spaces.
5. **Pass `-l` only when you want manual control.** Without `-l`, dependencies found beside the input are embedded automatically.

How you invoke the CLI depends on how you installed it:

| Platform | Command |
| --- | --- |
| Windows | `Embedix.CLI.exe embed ...` |
| Any OS (needs .NET 10 runtime) | `dotnet /path/to/Embedix.CLI.dll embed ...` |

The examples below use a variable so you can switch easily.

## PowerShell

```powershell
$ErrorActionPreference = 'Stop'

$embedix = 'C:\tools\Embedix\Embedix.CLI.exe'
$input   = Join-Path $PSScriptRoot 'bin\Release\net10.0\MyApp.dll'
$output  = Join-Path $PSScriptRoot 'artifacts\embedded'

& $embedix embed --input $input --output $output --no-pdb

if ($LASTEXITCODE -ne 0) {
    throw "Embedix failed with exit code $LASTEXITCODE"
}

Write-Host "Embedded build: $output"
```

Choosing libraries explicitly:

```powershell
$dir  = Join-Path $PSScriptRoot 'bin\Release\net10.0'
$libs = @('Newtonsoft.Json.dll', 'Serilog.dll') | ForEach-Object { Join-Path $dir $_ }

& $embedix embed -i (Join-Path $dir 'MyApp.dll') -o $output -l @libs
```

Processing several assemblies:

```powershell
Get-ChildItem .\plugins\*.dll | ForEach-Object {
    & $embedix embed -i $_.FullName -o ".\plugins-embedded" --no-pdb
    if ($LASTEXITCODE -ne 0) { throw "Failed: $($_.Name) (exit $LASTEXITCODE)" }
}
```

## Bash (Linux / macOS)

```bash
#!/usr/bin/env bash
set -euo pipefail

EMBEDIX="dotnet /opt/embedix/Embedix.CLI.dll"
INPUT="$PWD/bin/Release/net10.0/MyApp.dll"
OUTPUT="$PWD/artifacts/embedded"

$EMBEDIX embed --input "$INPUT" --output "$OUTPUT" --no-pdb

# On Linux/macOS the runtimeconfig is not copied automatically (no .exe apphost),
# so copy it yourself if you run the result with 'dotnet MyApp.dll'.
cp "$(dirname "$INPUT")/MyApp.runtimeconfig.json" "$OUTPUT/"

echo "Embedded build: $OUTPUT"
```

With `set -e` the script stops as soon as Embedix returns a non-zero exit code. To react to specific codes:

```bash
set +e
$EMBEDIX embed -i "$INPUT" -o "$OUTPUT"
code=$?
set -e

case $code in
  0)   echo "OK" ;;
  2)   echo "Input assembly problem" >&2; exit 2 ;;
  3)   echo "Library problem" >&2; exit 3 ;;
  *)   echo "Embedix failed ($code)" >&2; exit 1 ;;
esac
```

## Windows batch (cmd)

```bat
@echo off
set EMBEDIX=C:\tools\Embedix\Embedix.CLI.exe

"%EMBEDIX%" embed -i "%~dp0bin\Release\net10.0\MyApp.dll" -o "%~dp0artifacts" --no-pdb
if errorlevel 1 (
    echo Embedix failed with exit code %errorlevel%
    exit /b %errorlevel%
)
```

## Copying extra files

Embedix writes the processed assembly and, for a `.dll` input that has both `<name>.exe` and `<name>.runtimeconfig.json` beside it, copies those two files too. Everything else (configuration files, content, native libraries) is your responsibility:

```powershell
Copy-Item .\bin\Release\net10.0\appsettings.json $output
```

## Run Embedix automatically after every build (MSBuild)

Add a post-build target to the **application's** project file. It runs only for Release builds:

```xml
<Target Name="EmbedDependencies" AfterTargets="Build" Condition="'$(Configuration)' == 'Release'">
  <PropertyGroup>
    <EmbedixCli>C:\tools\Embedix\Embedix.CLI.exe</EmbedixCli>
    <EmbedixOut>$(MSBuildProjectDirectory)\artifacts\embedded</EmbedixOut>
  </PropertyGroup>

  <Exec Command="&quot;$(EmbedixCli)&quot; embed -i &quot;$(TargetPath)&quot; -o &quot;$(EmbedixOut)&quot; --no-pdb" />
</Target>
```

`Exec` fails the build automatically when the command returns a non-zero exit code. If `$(TargetPath)` points to a `.dll` of a modern .NET app, the companion `.exe` and `.runtimeconfig.json` are copied automatically on Windows.

## Verifying the result in a script

A reliable smoke test is to run the embedded output from a folder that contains **only** the Embedix output:

```powershell
$test = Join-Path $env:TEMP 'embedix-smoke'
Remove-Item $test -Recurse -Force -ErrorAction SilentlyContinue
Copy-Item $output $test -Recurse

& "$test\MyApp.exe" --self-test
if ($LASTEXITCODE -ne 0) { throw 'Embedded build does not start on its own.' }
```

(Use whatever quick command your application supports.)

## Troubleshooting scripts

| Symptom | Likely cause |
| --- | --- |
| Exit code `2` | Input file not found or invalid path. Use an absolute path. |
| Exit code `3` | A path given to `-l` is empty or does not exist. |
| Exit code `1` with "No libraries selected" | Automatic resolution found no DLLs beside the input. Check the input folder or pass `-l`. |
| Output not where expected | Relative `--output` is resolved from the current directory. Use an absolute path. |
| `Embedix1`, `Embedix2` folders appear | `--no-overwrite` creates a numbered folder when the target exists. |
