"""Command-line interface: argument parsing and the process entry point (main)."""

from __future__ import annotations

import argparse
import platform as plat
import sys
from pathlib import Path

from buildtools.config import BuildError, RUNTIME_IDENTIFIERS
from buildtools.pipeline import build
from buildtools.process import log


def detect_default_platform() -> str:
    system = plat.system().lower()
    if system == "windows":
        return "windows"
    if system == "darwin":
        return "mac"
    return "linux"


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Build Embedix in the required project order.")
    parser.add_argument(
        "--platform",
        choices=sorted(RUNTIME_IDENTIFIERS.keys()),
        default=detect_default_platform(),
        help="Target platform. Determines the publish runtime identifier and whether "
             "the WinForms GUI is built. Defaults to the OS you're running this on.",
    )
    parser.add_argument(
        "--configuration",
        default="Release",
        help="Build configuration passed to 'dotnet build'/'dotnet publish' (e.g. Release, Debug). "
             "Defaults to Release.",
    )
    parser.add_argument(
        "--root",
        type=Path,
        default=Path(__file__).resolve().parent.parent,
        help="Repository root containing the project folders "
             "(default: the repo root this script lives in).",
    )
    parser.add_argument(
        "--publish",
        action="store_true",
        help="Use 'dotnet publish' (with a runtime identifier) for CLI/GUI instead of "
             "'dotnet build'. Useful for producing a distributable output.",
    )
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)

    try:
        build(args.root.resolve(), args.platform, args.configuration, args.publish)
    except BuildError as error:
        log(f"\nBUILD FAILED: {error}")
        return 1
    except FileNotFoundError:
        log("\nBUILD FAILED: 'dotnet' was not found on PATH. Install the .NET SDK first.")
        return 1

    return 0


if __name__ == "__main__":
    sys.exit(main())
