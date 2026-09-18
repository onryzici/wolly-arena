"""Bake the free left arm onto the right with half-cycle phase and mirrored rest axes."""
import bpy, math, json
from pathlib import Path
from mathutils import Matrix
p=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource_v4.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];names=[b.name for b in r.pose.bones]
source=(p/'export_unity.py').read_text();exec(source[source.index('def sample('):source.index('idle_data=')])
reflection=Matrix.Diagonal((-1,1,1,1))
pairs=[(n,n.replace('Right','Left')) for n in names if 'Right' in n and any(k in n for k in ['Shoulder','Arm','Hand']) and n.replace('Right','Left') in names]
report={}
for name in ['Idle','Walk','Run']:
 old=bpy.data.actions[name];count=int(old.frame_range[1]);poses=[sample(old,f) for f in range(1,count+1)];targets=[]
 for i in range(count):
  frame=1+((i+(count-1)/2)%(count-1)) if name!='Idle' else i+1
  sample(old,frame)
  targets.append({right:reflection@r.pose.bones[left].matrix@r.data.bones[left].matrix_local.inverted()@reflection@r.data.bones[right].matrix_local for right,left in pairs})
 frames=[];points=[]
 for i,pose in enumerate(poses):
  r.animation_data.action=None;apply(pose)
  for right,left in pairs:
   r.pose.bones[right].matrix=targets[i][right];bpy.context.view_layer.update()
  points.append(list(r.pose.bones['mixamorig:RightHand'].head))
  frames.append({n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names})
 frames[-1]=frames[0]
 # No non-arm channels may change; all new poses must remain finite.
 right_names={n for n,_ in pairs}
 for i,data in enumerate(frames[:-1]):
  for n,(loc,q,scale) in data.items():
   assert all(math.isfinite(v) for values in (loc,q,scale) for v in values)
   if n not in right_names:
    assert (loc-poses[i][n][0]).length<1e-4
    assert abs(q.normalized().dot(poses[i][n][1].normalized()))>0.9999,(name,i,n,list(q),list(poses[i][n][1]))
 travel=max(v[1] for v in points)-min(v[1] for v in points)
 if name!='Idle':assert travel>.05,(name,travel)
 bpy.data.actions.remove(old);action_from(name,frames)
 report[name]={'frames':count,'right_hand_travel':travel,'preserved_non_arm_channels':True,'loop_seam_closed':True}
r.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1)
for ob in bpy.context.selected_objects:ob.select_set(False)
for name in ['Woolly_Rig','Woolly_Body','Revolver_HandSocket','Revolver_GameMesh','Muzzle']:bpy.data.objects[name].select_set(True)
bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(p.parents[1]/'WoollyArenaTest/Assets/Woolly/Art/Woolly.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,use_armature_deform_only=False,use_mesh_modifiers=True,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Unity_Unarmed.blend'))
(p/'unarmed_locomotion_report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
