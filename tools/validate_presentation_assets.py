"""Offline resource-link and bounded audio checks; does not claim Unity playback QA."""
from pathlib import Path
import re,json,subprocess,hashlib
ROOT=Path(__file__).resolve().parents[1];assets=ROOT/'WoollyArenaTest/Assets/Woolly';resources=assets/'Resources';checks=[]
def check(ok,text):
 if not ok:raise AssertionError(text)
 checks.append(text)
s=(assets/'Scripts/CombatAudio.cs').read_text()
for line in re.findall(r'Add\(CombatCue\.[^;]+;',s):
 for name in re.findall(r'"([^"]+)"',line):
  paths=list((resources/'Audio/SFX').glob(name+'.*'));paths=[p for p in paths if p.suffix!='.meta'];check(len(paths)==1,'Unique sound resource '+name)
  data=json.loads(subprocess.check_output(['ffprobe','-v','quiet','-show_format','-of','json',str(paths[0])]))
  check(0<float(data['format']['duration'])<1.1,'Bounded sound duration '+name)
for name in ['Muzzle','Smoke','Spark','Impact']:check((resources/'VFX'/(name+'.png')).exists(),'Particle texture '+name)
for name in ['Skeleton_Minion','Skeleton_Rogue','Skeleton_Warrior','Skeleton_Mage','Skeleton_Axe','Skeleton_Blade','Skeleton_Staff','barrel_large','crates_stacked','pillar_decorated','banner_red','banner_blue','torch_lit','chest_gold','barrel_small_stack']:check((resources/'KayKit'/(name+'.fbx')).exists(),'KayKit resource '+name)
for texture in ['dungeon_texture','skeleton_texture']:check((resources/'KayKit'/(texture+'.png')).exists(),'Authored palette '+texture)
report={'checks':checks,'count':len(checks),'not_run':['Unity importer and shaders','Unity animation playback and collider fit','audio listening and device performance']}
(ROOT/'WoollyArenaTest/Logs/presentation-assets.json').write_text(json.dumps(report,indent=2));print('PASS',len(checks),'presentation asset checks')
