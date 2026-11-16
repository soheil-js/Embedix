# Using Embedix in GitHub Actions

This guide shows how to run Embedix CLI in a workflow: download the CLI from the latest release, embed dependencies after your build, and upload the result.

> The workflows below pin actions to major versions (`@v4`). Check the Marketplace for current versions, and consider pinning to a commit SHA for supply-chain safety.

## What you need

- The **.NET 10 runtime** on the runner (the CLI is framework-dependent). `actions/setup-dotnet` installs it.
- The CLI archive from [GitHub Releases](https://github.com/soheil-js/Embedix/releases): `Embedix.CLI-v<version>-<windows|mac|linux>-x64.zip`.
- The GitHub CLI (`gh`) is preinstalled on GitHub-hosted runners and can download release assets.

## Example 1: build on Linux, embed, upload

```yaml
name: Build with Embedix

on:
  push:
    branches: [main]
  pull_request:

jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Build application
        run: dotnet build src/MyApp/MyApp.csproj -c Release

      - name: Install Embedix CLI
        env:
          GH_TOKEN: ${{ github.token }}
        run: |
          mkdir -p "$RUNNER_TEMP/embedix"
          gh release download --repo soheil-js/Embedix \
            --pattern 'Embedix.CLI-*-linux-x64.zip' \
            --dir "$RUNNER_TEMP/embedix"
          unzip -q "$RUNNER_TEMP"/embedix/Embedix.CLI-*.zip -d "$RUNNER_TEMP/embedix/cli"

      - name: Embed dependencies
        run: |
          dotnet "$RUNNER_TEMP/embedix/cli/Embedix.CLI.dll" embed \
            --input "$GITHUB_WORKSPACE/src/MyApp/bin/Release/net10.0/MyApp.dll" \
            --output "$GITHUB_WORKSPACE/artifacts/embedded" \
            --no-pdb
          # no .exe apphost on Linux, so copy the runtime config yourself
          cp src/MyApp/bin/Release/net10.0/MyApp.runtimeconfig.json artifacts/embedded/

      - name: Smoke test (run without the original dependencies)
        run: dotnet artifacts/embedded/MyApp.dll --version

      - uses: actions/upload-artifact@v4
        with:
          name: myapp-embedded
          path: artifacts/embedded
          if-no-files-found: error
```

## Example 2: Windows runner

On Windows the CLI is an `.exe`, and the `.exe` + `.runtimeconfig.json` companion files are copied for you.

```yaml
name: Build with Embedix (Windows)

on:
  workflow_dispatch:
  push:
    tags: ['v*']

jobs:
  build:
    runs-on: windows-latest

    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - run: dotnet build src\MyApp\MyApp.csproj -c Release

      - name: Install Embedix CLI
        shell: pwsh
        env:
          GH_TOKEN: ${{ github.token }}
        run: |
          gh release download --repo soheil-js/Embedix --pattern 'Embedix.CLI-*-windows-x64.zip' --dir "$env:RUNNER_TEMP\embedix"
          Expand-Archive "$env:RUNNER_TEMP\embedix\Embedix.CLI-*.zip" -DestinationPath "$env:RUNNER_TEMP\embedix\cli"

      - name: Embed dependencies
        shell: pwsh
        run: |
          & "$env:RUNNER_TEMP\embedix\cli\Embedix.CLI.exe" embed `
            --input "$env:GITHUB_WORKSPACE\src\MyApp\bin\Release\net10.0\MyApp.dll" `
            --output "$env:GITHUB_WORKSPACE\artifacts\embedded" `
            --no-pdb
          if ($LASTEXITCODE -ne 0) { throw "Embedix failed ($LASTEXITCODE)" }

      - uses: actions/upload-artifact@v4
        with:
          name: myapp-embedded-windows
          path: artifacts\embedded
```

## Example 3: pin a specific Embedix version

Using "latest" can change your build unexpectedly. Pin a release tag instead:

```yaml
env:
  EMBEDIX_TAG: v3.0.4        # a tag from https://github.com/soheil-js/Embedix/releases

steps:
  - name: Install Embedix CLI
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      gh release download "$EMBEDIX_TAG" --repo soheil-js/Embedix \
        --pattern 'Embedix.CLI-*-linux-x64.zip' --dir "$RUNNER_TEMP/embedix"
      unzip -q "$RUNNER_TEMP"/embedix/Embedix.CLI-*.zip -d "$RUNNER_TEMP/embedix/cli"
```

Cache the download to avoid fetching it every run:

```yaml
- uses: actions/cache@v4
  id: embedix-cache
  with:
    path: ${{ runner.temp }}/embedix/cli
    key: embedix-${{ runner.os }}-${{ env.EMBEDIX_TAG }}

- name: Install Embedix CLI
  if: steps.embedix-cache.outputs.cache-hit != 'true'
  # ... the download step from above
```

## Example 4: reusable composite action

Put this in your repository as `.github/actions/embedix/action.yml` to reuse the installation in many workflows.

```yaml
name: Embed dependencies
description: Run Embedix CLI on an assembly

inputs:
  input:
    description: Path to the input assembly
    required: true
  output:
    description: Output directory
    required: true
  version:
    description: Embedix release tag (empty = latest)
    default: ''
  extra-args:
    description: Extra CLI arguments, for example --no-pdb
    default: --no-pdb

runs:
  using: composite
  steps:
    - uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Download Embedix CLI
      shell: bash
      env:
        GH_TOKEN: ${{ github.token }}
      run: |
        case "$RUNNER_OS" in
          Windows) platform=windows ;;
          macOS)   platform=mac ;;
          *)       platform=linux ;;
        esac
        mkdir -p "$RUNNER_TEMP/embedix"
        gh release download ${{ inputs.version }} --repo soheil-js/Embedix \
          --pattern "Embedix.CLI-*-${platform}-x64.zip" --dir "$RUNNER_TEMP/embedix"
        unzip -q "$RUNNER_TEMP"/embedix/Embedix.CLI-*.zip -d "$RUNNER_TEMP/embedix/cli"

    - name: Run Embedix
      shell: bash
      run: |
        dotnet "$RUNNER_TEMP/embedix/cli/Embedix.CLI.dll" embed \
          --input "${{ inputs.input }}" \
          --output "${{ inputs.output }}" \
          ${{ inputs.extra-args }}
```

Use it:

```yaml
- uses: ./.github/actions/embedix
  with:
    input: ${{ github.workspace }}/src/MyApp/bin/Release/net10.0/MyApp.dll
    output: ${{ github.workspace }}/artifacts/embedded
```

## Example 5: attach the result to a GitHub Release

```yaml
- name: Zip embedded build
  run: cd artifacts/embedded && zip -r ../MyApp-embedded.zip .

- name: Publish release asset
  if: startsWith(github.ref, 'refs/tags/')
  env:
    GH_TOKEN: ${{ github.token }}
  run: gh release upload "${{ github.ref_name }}" artifacts/MyApp-embedded.zip --clobber
```

Give the job `permissions: contents: write` for this step.

## Tips

- Use **absolute paths** (`$GITHUB_WORKSPACE`), because a relative `--output` is resolved from the current directory.
- The step fails automatically if Embedix returns a non-zero exit code (bash `set -e` is the default in `run` steps; in PowerShell check `$LASTEXITCODE`).
- Run a **smoke test** of the embedded output in a clean folder to make sure it starts without the original DLLs.
- Embedix CLI itself needs the .NET 10 runtime even if your application targets another framework.
