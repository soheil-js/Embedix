"""Build tooling for Embedix. See buildtools.cli.main for the entry point."""

from buildtools.config import BuildError
from buildtools.pipeline import build

__all__ = ["BuildError", "build"]
