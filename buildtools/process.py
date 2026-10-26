"""Low-level helpers: console logging and running subprocesses with timing/error handling."""

from __future__ import annotations

import subprocess
import time
from pathlib import Path

from buildtools.config import BuildError


def log(message: str, *, indent: int = 0) -> None:
    prefix = "  " * indent
    print(f"{prefix}{message}", flush=True)


def run_step(title: str, args: list[str], *, cwd: Path) -> None:
    """Run a command, logging what it is and how long it took. Raises BuildError on failure."""
    log(f"-> {title}")
    log(f"$ {' '.join(args)}", indent=1)

    start = time.monotonic()
    result = subprocess.run(args, cwd=cwd)
    elapsed = time.monotonic() - start

    if result.returncode != 0:
        raise BuildError(f"'{title}' failed (exit code {result.returncode}) after {elapsed:.1f}s")

    log(f"done in {elapsed:.1f}s", indent=1)
