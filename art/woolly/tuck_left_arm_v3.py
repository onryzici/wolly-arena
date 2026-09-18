import bpy,math,json
from pathlib import Path
from mathutils import Matrix
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource_v2.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];names=[b.name for b in r.pose.bones]
source=(p/'export_unity.py').read_text();exec(source[source.index('def sample('):source.index('idle_data=')])
report=[]
for name in ['Idle','Walk','Run','Shoot']:
 old=bpy.data.actions[name];count=int(old.frame_range[1]);poses=[sample(old,f) for f in range(1,count+1)];frames=[]
 for pose in poses:
  r.animation_data.action=None;apply(pose);before=r.pose.bones['mixamorig:LeftHand'].head.copy()
  b=r.pose.bones['mixamorig:LeftArm'];m=b.matrix.copy();out=(Matrix.Rotation(math.radians(13),3,'Y')@m.to_3x3()).to_4x4();out.translation=m.translation;b.matrix=out;bpy.context.view_layer.update()
  # Small forward elbow bend, while retaining the authored step swing.
  b=r.pose.bones['mixamorig:LeftForeArm'];m=b.matrix.copy();out=(Matrix.Rotation(math.radians(-7),3,'X')@m.to_3x3()).to_4x4();out.translation=m.translation;b.matrix=out;bpy.context.view_layer.update()
  after=r.pose.bones['mixamorig:LeftHand'].head.copy();report.append({'clip':name,'before':list(before),'after':list(after)})
  frames.append({n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names})
 if name!='Shoot':frames[-1]=frames[0]
 bpy.data.actions.remove(old);action_from(name,frames)
r.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1)
for ob in bpy.context.selected_objects:ob.select_set(False)
for name in ['Woolly_Rig','Woolly_Body','Revolver_HandSocket','Revolver_GameMesh','Muzzle']:bpy.data.objects[name].select_set(True)
bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(p.parents[1]/'WoollyArenaTest/Assets/Woolly/Art/Woolly.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,use_armature_deform_only=False,use_mesh_modifiers=True,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource_v3.blend'));(p/'left_arm_v3_report.json').write_text(json.dumps(report,indent=2))
