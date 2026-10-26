"""Orchestrates the Embedix build in the required order.

Build order (strict):
    1. Embedix.Core.Payload   (must finish first - Embedix.Core embeds its output as resources)
    2. Embedix.Core           (depends on step 1's output being on disk)
    3. Embedix.CLI / Embedix.GUI  (depend on step 2, no ordering between each other)
"""

from __future__ import annotations

from pathlib import Path

from buildtools.config import (
    CLI_PROJECT,
    CORE_PROJECT,
    GUI_PROJECT,
    GUI_SUPPORTED_PLATFORMS,
    PAYLOAD_PROJECT,
    RUNTIME_IDENTIFIERS,
)
from buildtools.dotnet import dotnet_build, dotnet_publish
from buildtools.process import log


def build(root: Path, platform_name: str, configuration: str, publish: bool) -> None:
    runtime = RUNTIME_IDENTIFIERS[platform_name]

    log(f"Building Embedix for platform='{platform_name}', configuration='{configuration}'")
    log("")

    # Step 1 - MUST complete before Core, since Core embeds this build's output as a resource.
    dotnet_build(root, PAYLOAD_PROJECT, configuration, "[1/3] Embedix.Core.Payload")

    # Step 2 - depends on step 1's output existing on disk.
    dotnet_build(root, CORE_PROJECT, configuration, "[2/3] Embedix.Core")

    # Step 3 - CLI always builds; GUI only on platforms that support it (WinForms, Windows-only).
    _build_or_publish(root, CLI_PROJECT, configuration, runtime, publish, "[3/3] Embedix.CLI")

    if platform_name in GUI_SUPPORTED_PLATFORMS:
        _build_or_publish(root, GUI_PROJECT, configuration, runtime, publish, "[3/3] Embedix.GUI")
    else:
        log("")
        log(f"Skipping Embedix.GUI: it's WinForms-based and only runs on Windows "
            f"(platform='{platform_name}').")

    log("")
    log("Build finished successfully.")


def _build_or_publish(
    root: Path, csproj: str, configuration: str, runtime: str, publish: bool, title: str
) -> None:
    if publish:
        dotnet_publish(root, csproj, configuration, runtime, f"{title} (publish)")
    else:
        dotnet_build(root, csproj, configuration, title)
