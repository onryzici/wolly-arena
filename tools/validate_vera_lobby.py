"""Blender-only regression: boxing must preserve the approved planted lower body."""
import bpy,json
from pathlib import Path
ART=Path(__file__).resolve().parents[1]/'art/woolly/punk_vera'
bones=['Hips']+[side+part for side in ['Left','Right']for part in ['UpLeg','Leg','Foot','ToeBase','Toe_End']]
def poses(file):
 bpy.ops.wm.open_mainfile(filepath=str(ART/file));rig=bpy.data.objects['PunkVera_Rig'];rig.animation_data.action=bpy.data.actions['Idle'];scene=bpy.context.scene
 names=[b.name for b in rig.pose.bones if b.name.removeprefix('mixamorig:') in bones]
 assert len(names)>=9,names
 result={}
 for half in range(2,147):
  frame=half/2;scene.frame_set(int(frame),subframe=frame%1);bpy.context.view_layer.update()
  result[frame]={name:[v for row in (rig.matrix_world@rig.pose.bones[name].matrix)for v in row]for name in names}
 return result
source=poses('PunkVera_WoollyRig.blend');boxer=poses('PunkVera_LobbyBoxer.blend')
error=max(abs(a-b)for f in source for n in source[f]for a,b in zip(source[f][n],boxer[f][n]))
assert error<1e-5,error
report={'frames':len(source),'lower_body_bones':len(source[1]),'maximum_matrix_error':error,'passed':True}
(ART/'lobby-lower-body-validation.json').write_text(json.dumps(report,indent=2));print(report)
# Check exported data too; FBX baking must preserve the same lower-body playback.
bpy.ops.wm.read_factory_settings(use_empty=True);scene=bpy.context.scene;scene.render.fps=24;rigs=[]
for path in [ART.parents[2]/'WoollyArenaTest/Assets/Woolly/Resources/Characters/PunkVera.fbx',ART/'PunkVeraLobby.fbx']:
 objects=set(bpy.data.objects);actions=set(bpy.data.actions);bpy.ops.import_scene.fbx(filepath=str(path))
 rig=next(o for o in set(bpy.data.objects)-objects if o.type=='ARMATURE')
 rig.animation_data.action=next(a for a in set(bpy.data.actions)-actions if a.name.rsplit('|',1)[-1].split('.')[0]=='Idle')
 for track in rig.animation_data.nla_tracks:track.mute=True
 rigs.append(rig)
error=0
for half in range(2,147):
 frame=half/2;scene.frame_set(int(frame),subframe=frame%1);bpy.context.view_layer.update()
 for name in source[1]:
  a=rigs[0].matrix_world@rigs[0].pose.bones[name].matrix;b=rigs[1].matrix_world@rigs[1].pose.bones[name].matrix
  error=max(error,max(abs(x-y)for row,rr in zip(a,b)for x,y in zip(row,rr)))
assert error<.0001,error
report['fbx_maximum_matrix_error']=error
(ART/'lobby-lower-body-validation.json').write_text(json.dumps(report,indent=2));print(report)
