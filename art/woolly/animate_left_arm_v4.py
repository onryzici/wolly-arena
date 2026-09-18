import bpy,math,json
from pathlib import Path
from mathutils import Matrix
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource_v2.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];names=[b.name for b in r.pose.bones]
source=(p/'export_unity.py').read_text();exec(source[source.index('def sample('):source.index('idle_data=')])
base=sample(bpy.data.actions['Idle'],1); report={}
def rotate(n,angle,axis):
 b=r.pose.bones['mixamorig:'+n];m=b.matrix.copy();out=(Matrix.Rotation(math.radians(angle),3,axis)@m.to_3x3()).to_4x4();out.translation=m.translation;b.matrix=out;bpy.context.view_layer.update()
for name in ['Idle','Walk','Run','Shoot']:
 old=bpy.data.actions[name];count=int(old.frame_range[1]);poses=[sample(old,f) for f in range(1,count+1)];frames=[];points=[]
 for i,pose in enumerate(poses):
  r.animation_data.action=None;apply(pose)
  if name in ['Walk','Run']:
   # Rebuild only the free arm from neutral; phase follows the opposite leg,
   # instead of layering an arbitrary sine over the existing swing.
   for n in ['LeftArm','LeftForeArm','LeftHand']:
    b=r.pose.bones['mixamorig:'+n];loc,q,sc=base[b.name];b.location=loc;b.rotation_quaternion=q;b.scale=sc
   bpy.context.view_layer.update()
   thigh=r.pose.bones['mixamorig:LeftUpLeg'];v=thigh.tail-thigh.head
   legPitch=math.atan2(-v.y,-v.z)
   swing=max(-1,min(1,legPitch/.55))
   rotate('LeftArm',9,'Y')
   rotate('LeftArm',-(25 if name=='Run' else 15)*swing,'X')
   rotate('LeftForeArm',-(32 if name=='Run' else 19)-8*swing,'X')
  else:
   rotate('LeftArm',9,'Y');rotate('LeftForeArm',-7,'X')
  points.append(list(r.pose.bones['mixamorig:LeftHand'].head))
  frames.append({n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names})
 if name!='Shoot':frames[-1]=frames[0]
 bpy.data.actions.remove(old);action_from(name,frames)
 report[name]={'hand_travel_y':max(x[1] for x in points)-min(x[1] for x in points),'hand_x_min':min(x[0] for x in points),'hand_x_max':max(x[0] for x in points)}
r.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1)
for ob in bpy.context.selected_objects:ob.select_set(False)
for name in ['Woolly_Rig','Woolly_Body','Revolver_HandSocket','Revolver_GameMesh','Muzzle']:bpy.data.objects[name].select_set(True)
bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(p.parents[1]/'WoollyArenaTest/Assets/Woolly/Art/Woolly.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,use_armature_deform_only=False,use_mesh_modifiers=True,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource_v4.blend'));(p/'left_arm_v4_report.json').write_text(json.dumps(report,indent=2));print(report)
