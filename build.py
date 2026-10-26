#!/usr/bin/env python3
"""
Build script for Embedix.

This file is just the entry point - all logic lives in the buildtools/ package
next to it (config, dotnet wrappers, pipeline orchestration, CLI parsing).

Usage:
    python build.py --platform windows
    python build.py --platform mac --configuration Debug
    python build.py --platform linux --root /path/to/repo
"""

import sys

from buildtools.cli import main

if __name__ == "__main__":
    sys.exit(main())
