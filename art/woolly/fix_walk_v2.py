import bpy, math, json
from pathlib import Path
from mathutils import Vector, Matrix
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];names=[b.name for b in r.pose.bones]
# Reuse the exporter keyframe helpers without rebuilding textures or the other clips.
source=(p/'export_unity.py').read_text();exec(source[source.index('def sample('):source.index('idle_data=')])
base=sample(bpy.data.actions['Idle'],1)
neutral={side:r.pose.bones['mixamorig:'+side+'Foot'].matrix.copy() for side in ['Left','Right']}
for side,m in neutral.items():
 foot=r.pose.bones['mixamorig:'+side+'Foot'];toe=r.pose.bones['mixamorig:'+side+'ToeBase'];v=toe.head-foot.head
 yaw=math.atan2(v.x,-v.y);rot=Matrix.Rotation(-yaw,3,'Z');m2=(rot@m.to_3x3()).to_4x4();m2.translation=m.translation;neutral[side]=m2
rest={b.name:b.matrix_local.copy() for b in r.data.bones};frames=[];report=[]
for f in range(25):
 r.animation_data.action=None;apply(base);theta=2*math.pi*f/24
 root=r.pose.bones['mixamorig:Hips'];m=rest[root.name].copy();m.translation+=Vector((.003*math.sin(theta),0,-.026+.005*math.cos(2*theta)));root.matrix=m;bpy.context.view_layer.update()
 for side,offset,sign in [('Left',0,1),('Right',.5,-1)]:
  phase=(f/24+offset)%1;ankle=neutral[side].translation.copy();ankle.x=sign*.105
  if phase<.6:
   u=phase/.6;ankle.y+=-.18+.36*u;lift=0;pitch=.09*(1-min(u/.18,1))-.14*max(0,(u-.78)/.22)
  else:
   u=(phase-.6)/.4;ankle.y+=.18-.36*u*u*(3-2*u);lift=.065*math.sin(math.pi*u);pitch=-.28*math.sin(math.pi*u)
  ankle.z+=lift
  upper=r.pose.bones['mixamorig:'+side+'UpLeg'];lower=r.pose.bones['mixamorig:'+side+'Leg'];foot=r.pose.bones['mixamorig:'+side+'Foot']
  hip=upper.head.copy();axis=(ankle-hip).normalized();distance=(ankle-hip).length;L1=upper.bone.length;L2=lower.bone.length
  d=(L1*L1-L2*L2+distance*distance)/(2*distance);h=math.sqrt(max(0,L1*L1-d*d));pole=Vector((0,-1,0));bend=(pole-axis*pole.dot(axis)).normalized();knee=hip+axis*d+bend*h
  for b,start,end in [(upper,hip,knee),(lower,knee,ankle)]:
   rot=rest[b.name].to_3x3().col[1].rotation_difference((end-start).normalized());m=(rot.to_matrix()@rest[b.name].to_3x3()).to_4x4();m.translation=start;b.matrix=m;bpy.context.view_layer.update()
  m=(Matrix.Rotation(-pitch,3,'X')@neutral[side].to_3x3()).to_4x4();m.translation=ankle;foot.matrix=m;bpy.context.view_layer.update()
 frames.append({n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names})
 report.append({'frame':f+1,'ankle_width':r.pose.bones['mixamorig:LeftFoot'].head.x-r.pose.bones['mixamorig:RightFoot'].head.x})
frames[-1]=frames[0];old=bpy.data.actions['Walk'];bpy.data.actions.remove(old);walk=action_from('Walk',frames)
# Apply rotations in character space: the model faces -Y and up is +Z.
def pitch_bone(name,angle):
 b=r.pose.bones[name];m=b.matrix.copy();rot=Matrix.Rotation(math.radians(angle),3,'X');out=(rot@m.to_3x3()).to_4x4();out.translation=m.translation;b.matrix=out;bpy.context.view_layer.update()
def capture():
 return {n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names}
for clip,count,amplitude in [('Walk',25,7),('Run',17,12)]:
 old=bpy.data.actions[clip];poses=[sample(old,f) for f in range(1,count+1)];result=[]
 for i,pose in enumerate(poses):
  r.animation_data.action=None;apply(pose);phase=2*math.pi*i/(count-1)
  pitch_bone('mixamorig:LeftArm',amplitude*math.sin(phase))
  pitch_bone('mixamorig:LeftForeArm',-10-amplitude*.65*(.5+.5*math.cos(phase)))
  result.append(capture())
 result[-1]=result[0];bpy.data.actions.remove(old);action_from(clip,result)
result=[];recoil_report=[]
for i in range(10):
 r.animation_data.action=None;apply(base);u=i/9;envelope=(u/.22 if u<.22 else max(0,(1-u)/.78)**2)
 pitch_bone('mixamorig:RightArm',-2*envelope)
 pitch_bone('mixamorig:RightForeArm',-4*envelope)
 pitch_bone('mixamorig:RightHand',-3*envelope)
 result.append(capture());recoil_report.append({'frame':i+1,'muzzle':list(bpy.data.objects['Muzzle'].matrix_world.translation)})
bpy.data.actions.remove(bpy.data.actions['Shoot']);action_from('Shoot',result)
(p/'recoil_v2_report.json').write_text(json.dumps(recoil_report,indent=2))
r.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1)
for ob in bpy.context.selected_objects:ob.select_set(False)
for name in ['Woolly_Rig','Woolly_Body','Revolver_HandSocket','Revolver_GameMesh','Muzzle']:bpy.data.objects[name].select_set(True)
bpy.context.view_layer.objects.active=r
out=p.parents[1]/'WoollyArenaTest/Assets/Woolly/Art/Woolly.fbx'
bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,use_armature_deform_only=False,use_mesh_modifiers=True,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource_v2.blend'))
(p/'walk_v2_report.json').write_text(json.dumps(report,indent=2))
