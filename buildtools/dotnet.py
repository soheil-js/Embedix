"""Thin wrappers around the `dotnet` CLI used by every build step."""

from __future__ import annotations

from pathlib import Path

from buildtools.config import BuildError
from buildtools.process import run_step


def dotnet_build(root: Path, csproj: str, configuration: str, title: str) -> None:
    project_path = root / csproj
    if not project_path.exists():
        raise BuildError(f"Project not found: {project_path}")

    run_step(title, ["dotnet", "build", str(project_path), "-c", configuration], cwd=root)


def dotnet_publish(root: Path, csproj: str, configuration: str, runtime: str, title: str) -> None:
    project_path = root / csproj
    if not project_path.exists():
        raise BuildError(f"Project not found: {project_path}")

    run_step(
        title,
        [
            "dotnet", "publish", str(project_path),
            "-c", configuration,
            "-r", runtime,
            "--self-contained", "false",
        ],
        cwd=root,
    )
