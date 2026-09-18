"""Rebuild Vera on the actual Woolly skeleton (offline Blender only)."""
from pathlib import Path
import runpy
root=Path(__file__).resolve().parents[3]
runpy.run_path(str(root/'tools/bind_vera_to_woolly.py'))
runpy.run_path(str(root/'tools/publish_vera_woolly.py'))
