"""Compatibility entry point for the actual-Woolly-rig validation."""
from pathlib import Path
import runpy
runpy.run_path(str(Path(__file__).resolve().with_name('validate_vera_woolly_rig.py')))
