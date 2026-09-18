"""Verify shipped FBXs have the same world-space idle skeleton after reimport."""
from pathlib import Path
import json,bpy
root=Path(__file__).resolve().parents[1]
bpy.ops.wm.read_factory_settings(use_empty=True);s=bpy.context.scene;s.render.fps=24
rigs=[];idles=[]
for path in [root/'WoollyArenaTest/Assets/Woolly/Art/Woolly.fbx',root/'WoollyArenaTest/Assets/Woolly/Resources/Characters/PunkVera.fbx']:
 objects=set(bpy.data.objects);actions=set(bpy.data.actions)
 bpy.ops.import_scene.fbx(filepath=str(path))
 rig=next(o for o in set(bpy.data.objects)-objects if o.type=='ARMATURE')
 clips=list(set(bpy.data.actions)-actions)
 idle=next(a for a in clips if a.name.rsplit('|',1)[-1].split('.')[0]=='Idle')
 rigs.append(rig);idles.append(idle);rig.animation_data.action=idle
 for track in rig.animation_data.nla_tracks:track.mute=True
assert set(rigs[0].pose.bones.keys())==set(rigs[1].pose.bones.keys())
assert tuple(idles[0].frame_range)==tuple(idles[1].frame_range)
a,b=map(int,idles[0].frame_range);error=0
for step in range((b-a)*2+1):
 frame=a+step/2;s.frame_set(int(frame),subframe=frame%1)
 for bone in rigs[0].pose.bones:
  source=rigs[0].matrix_world@bone.matrix
  target=rigs[1].matrix_world@rigs[1].pose.bones[bone.name].matrix
  error=max(error,max(abs(x-y)for row,rr in zip(source,target)for x,y in zip(row,rr)))
assert error<.0001,('exported idle skeleton differs',error)
report={'bone_count':len(rigs[0].pose.bones),'idle_samples':(b-a)*2+1,'maximum_world_matrix_difference':error,'source':'shipped Woolly.fbx','target':'shipped PunkVera.fbx'}
(root/'art/woolly/punk_vera/woolly-export-validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
