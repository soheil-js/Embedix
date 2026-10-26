"""Static configuration for the Embedix build: project paths and platform mappings.

Edit this file (not the other modules) if the solution's folder layout changes.
"""

from __future__ import annotations

# --- Project layout -------------------------------------------------------
# Adjust these if your solution structure differs.
PAYLOAD_PROJECT = "src/Embedix.Core.Payload/Embedix.Core.Payload.csproj"
CORE_PROJECT = "src/Embedix.Core/Embedix.Core.csproj"
CLI_PROJECT = "src/Embedix.CLI/Embedix.CLI.csproj"
GUI_PROJECT = "src/Embedix.GUI/Embedix.GUI.csproj"

# Runtime identifiers used for `dotnet publish` of the CLI/GUI per target platform.
RUNTIME_IDENTIFIERS: dict[str, str] = {
    "windows": "win-x64",
    "mac": "osx-x64",
    "linux": "linux-x64",
}

# Platforms whose GUI build is skipped (Embedix.GUI is WinForms, Windows-only).
GUI_SUPPORTED_PLATFORMS = frozenset({"windows"})


class BuildError(Exception):
    """Raised when a build step fails; caught at the CLI boundary to print a clean error."""
