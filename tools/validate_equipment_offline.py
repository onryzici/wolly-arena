#!/usr/bin/env python3
"""Compile current runtime/editor scripts and execute economy tests without Unity Editor."""
from pathlib import Path
import json
import os
import re
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "WoollyArenaTest"
VERSION = re.search(r"m_EditorVersion: (\S+)", (PROJECT / "ProjectSettings/ProjectVersion.txt").read_text()).group(1)
# Use the installed compiler only. This never invokes the Unity Editor executable.
SCRIPTING = Path(os.environ.get("WOOLLY_UNITY_DATA", str(
    Path(os.environ.get("ProgramFiles", "C:/Program Files")) / f"Unity/Hub/Editor/{VERSION}/Editor/Data"
    if os.name == "nt" else Path(f"/Applications/Unity/Hub/Editor/{VERSION}/Unity.app/Contents/Resources/Scripting"))))
DOTNET = SCRIPTING / ("DotNetSdk/dotnet.exe" if os.name == "nt" else "DotNetSdk/dotnet")
COMPILERS = sorted((SCRIPTING / "DotNetSdk/sdk").glob("*/Roslyn/bincore/csc.dll"))
if not DOTNET.exists() or not COMPILERS:
    raise SystemExit("Unity's offline compiler was not found; set WOOLLY_UNITY_DATA to its data directory.")
CSC = COMPILERS[-1]
LOG = PROJECT / "Logs/equipment-offline-review.txt"
LOG.parent.mkdir(parents=True, exist_ok=True)
log = []

def execute(args, cwd=PROJECT, env=None):
    result = subprocess.run([str(x) for x in args], cwd=cwd, text=True, encoding="utf-8", errors="replace", capture_output=True, env=env)
    log.append(result.stdout + result.stderr)
    if result.returncode:
        LOG.write_text("\n".join(log), encoding="utf-8")
        raise SystemExit(f"Failed ({result.returncode}); see {LOG}")
    return result.stdout

with tempfile.TemporaryDirectory(prefix="woolly-equipment-") as temporary:
    temp = Path(temporary)
    # Reuse Unity's cached compiler references, without starting an Editor or modifying its binaries.
    candidates = [p for p in (PROJECT / "Library/Bee/artifacts").glob("*/Assembly-CSharp.rsp")
                  if (p.parent / "Assembly-CSharp-Editor.rsp").exists()]
    if not candidates:
        raise SystemExit("No cached Editor compiler references found; offline validation cannot run.")
    source = max(candidates, key=lambda p: p.stat().st_mtime)
    artifact = source.parent
    for name in ("Assembly-CSharp", "Assembly-CSharp-Editor"):
        response = (artifact / (name + ".rsp")).read_text().splitlines()
        lines = []
        for line in response:
            if line.startswith(("-out:", "-refout:", "/analyzer:", "-analyzer:", "/additionalfile:")):
                continue
            if line.startswith('"Assets/'):
                continue
            if name.endswith("-Editor") and "Assembly-CSharp." in line and line.startswith("-r:"):
                line = f'-r:"{temp / "Assembly-CSharp.dll"}"'
            lines.append(line)
        if name.endswith("-Editor"):
            sources = sorted(p for p in (PROJECT / "Assets").rglob("*.cs") if "Editor" in p.relative_to(PROJECT / "Assets").parts)
        else:
            sources = sorted(p for p in (PROJECT / "Assets").rglob("*.cs") if "Editor" not in p.relative_to(PROJECT / "Assets").parts)
        lines.extend('"' + str(p) + '"' for p in sources)
        lines.append(f'-out:"{temp / (name + ".dll")}"')
        rsp = temp / (name + ".rsp"); rsp.write_text("\n".join(lines))
        execute([DOTNET, CSC, "@" + str(rsp)])
        log.append(f"PASS Offline compile: {name} ({len(sources)} source files)")
    # Run the real production model and shared test suite under .NET, with Unity managed math types.
    program = temp / "Program.cs"
    program.write_text('''using System;
class Program {
 static int Main() { Console.OutputEncoding=System.Text.Encoding.UTF8; int checks=0; try {
  WoollyArena.Editor.SurvivalBuildChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.RunCheckpointChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.WaveClearChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.ExpansionChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.BalanceChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.CharacterDefinitionChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.UpgradeSystemChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.MeleeStrikeChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.CareerProgressChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.CombatAccentChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.ArsenalChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  WoollyArena.Editor.SurvivorChecks.Run((ok, text) => { if (!ok) throw new Exception(text); checks++; Console.WriteLine("PASS " + text); });
  Console.WriteLine("ALL PASSED: " + checks + " model checks. No Unity Editor launched."); return 0;
 } catch(Exception e) { Console.WriteLine("FAIL " + e); return 1; } }
}''')
    core = SCRIPTING / "Managed/UnityEngine/UnityEngine.CoreModule.dll"
    refs = sorted((SCRIPTING / "DotNetSdk/packs/Microsoft.NETCore.App.Ref").glob("*/ref/net8.0/*.dll"))
    lines = ["-target:exe", "-nologo", "-langversion:latest", f'-out:"{temp / "ModelChecks.dll"}"']
    lines.extend(f'-r:"{p}"' for p in refs)
    lines.append(f'-r:"{core}"')
    lines.extend(f'"{PROJECT / "Assets/Woolly" / p}"' for p in ["Scripts/EnemyTactics.cs", "Tests/Editor/SurvivorChecks.cs", "Scripts/WeaponArsenal.cs", "Scripts/WeaponAttackCycle.cs", "Scripts/CombatAilments.cs", "Scripts/WavePressure.cs", "Scripts/RepeaterHeat.cs", "Tests/Editor/ArsenalChecks.cs"])
    lines.extend(f'"{p}"' for p in [PROJECT / "Assets/Woolly/Scripts/CombatAccentBudget.cs", PROJECT / "Assets/Woolly/Tests/Editor/CombatAccentChecks.cs", PROJECT / "Assets/Woolly/Scripts/CareerProgress.cs", PROJECT / "Assets/Woolly/Tests/Editor/CareerProgressChecks.cs", PROJECT / "Assets/Woolly/Scripts/MeleeStrike.cs", PROJECT / "Assets/Woolly/Tests/Editor/MeleeStrikeChecks.cs", PROJECT / "Assets/Woolly/Tests/Editor/BalanceChecks.cs", PROJECT / "Assets/Woolly/Tests/Editor/UpgradeSystemChecks.cs", PROJECT / "Assets/Woolly/Tests/Editor/CharacterDefinitionChecks.cs", PROJECT / "Assets/Woolly/Scripts/CharacterDefinition.cs", PROJECT / "Assets/Woolly/Scripts/RunBalance.cs", PROJECT / "Assets/Woolly/Scripts/WaveDifficulty.cs", PROJECT / "Assets/Woolly/Tests/Editor/ExpansionChecks.cs", PROJECT / "Assets/Woolly/Scripts/SurvivalBuild.cs", PROJECT / "Assets/Woolly/Scripts/WaveClearTimeline.cs", PROJECT / "Assets/Woolly/Tests/Editor/WaveClearChecks.cs", PROJECT / "Assets/Woolly/Scripts/RunCheckpoint.cs", PROJECT / "Assets/Woolly/Tests/Editor/SurvivalBuildChecks.cs", PROJECT / "Assets/Woolly/Tests/Editor/RunCheckpointChecks.cs", program])
    rsp = temp / "model.rsp"; rsp.write_text("\n".join(lines))
    execute([DOTNET, CSC, "@" + str(rsp)])
    import shutil
    shutil.copy2(core, temp / core.name)
    (temp / "ModelChecks.runtimeconfig.json").write_text(json.dumps({"runtimeOptions": {"tfm": "net8.0", "framework": {"name": "Microsoft.NETCore.App", "version": "8.0.0"}}}))
    output = execute([DOTNET, temp / "ModelChecks.dll"], cwd=temp)
    print(output)
log.append("NOT RUN: Unity scene/PlayMode, rendered UI, device build, audio or performance review.")
LOG.write_text("\n".join(log), encoding="utf-8")
print(f"Report: {LOG}")
