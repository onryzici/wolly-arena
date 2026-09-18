"""Validate the new binding and refresh exported model/portrait offline."""
from pathlib import Path
import runpy
runpy.run_path(str(Path(__file__).resolve().parents[3]/'tools/publish_vera_woolly.py'))
