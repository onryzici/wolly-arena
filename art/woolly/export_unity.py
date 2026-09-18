import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');out=Path('/Users/trexoinnovation/mobil-lamb-test/WoollyArenaTest/Assets/Woolly/Art');out.mkdir(exist_ok=True,parents=True)
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_GripRig_v9.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];body=bpy.data.objects['Woolly_Body'];armed=r.animation_data.action;run=bpy.data.actions['Running_NaturalContact_V5'];s.render.fps=24
# Bake the editable Blender color repair into a portable game texture.
mat=body.active_material;nt=mat.node_tree;bs=next(n for n in nt.nodes if n.type=='BSDF_PRINCIPLED');output=next(n for n in nt.nodes if n.type=='OUTPUT_MATERIAL');source=bs.inputs['Base Color'].links[0].from_socket
baked=bpy.data.images.new('Woolly_BaseColor_Unity',2048,2048,alpha=False);baked.colorspace_settings.name='sRGB';target=nt.nodes.new('ShaderNodeTexImage');target.image=baked;nt.nodes.active=target;emit=nt.nodes.new('ShaderNodeEmission');nt.links.new(source,emit.inputs['Color']);nt.links.new(emit.outputs[0],output.inputs['Surface'])
for ob in bpy.context.selected_objects:ob.select_set(False)
body.select_set(True);bpy.context.view_layer.objects.active=body;s.render.engine='CYCLES';s.cycles.samples=1;s.render.bake.margin=8
bpy.ops.object.bake(type='EMIT');baked.filepath_raw=str(out/'Woolly_BaseColor.png');baked.file_format='PNG';baked.save();nt.links.new(bs.outputs[0],output.inputs['Surface']);nt.links.new(target.outputs['Color'],bs.inputs['Base Color'])
normal=bpy.data.images['Woolly_Normal'];normal.filepath_raw=str(out/'Woolly_Normal.png');normal.file_format='PNG';normal.save()
# Preserve all authored transforms with explicit, portable clips.
names=[b.name for b in r.pose.bones]
def sample(action,frame):
 r.animation_data.action=action;s.frame_set(int(frame),subframe=frame-int(frame));bpy.context.view_layer.update();return {n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names}
def apply(data):
 for n,(loc,q,scale) in data.items():b=r.pose.bones[n];b.location=loc;b.rotation_quaternion=q;b.scale=scale
 bpy.context.view_layer.update()
def action_from(name,frames):
 a=bpy.data.actions.new(name);a.use_fake_user=True;r.animation_data.action=a
 previous={}
 for frame,data in enumerate(frames,1):
  for n,(loc,q,scale) in data.items():
   b=r.pose.bones[n];q=q.copy()
   if n in previous and q.dot(previous[n])<0:q.negate()
   previous[n]=q.copy();b.location=loc;b.rotation_quaternion=q;b.scale=scale
   b.keyframe_insert(data_path='location',frame=frame,group=n);b.keyframe_insert(data_path='rotation_quaternion',frame=frame,group=n);b.keyframe_insert(data_path='scale',frame=frame,group=n)
 for layer in a.layers:
  for strip in layer.strips:
   for bag in strip.channelbags:
    for fc in bag.fcurves:
     for k in fc.keyframe_points:k.interpolation='LINEAR'
 return a
idle_data=[sample(armed,f) for f in range(1,74)];base=idle_data[0];idle=action_from('Idle',idle_data)
upper=[n for n in names if not any(x in n for x in ['Hips','UpLeg','Leg','Foot','Toe'])]
run_data=[]
for f in range(1,18):
 data=sample(run,f)
 for n in upper:data[n]=base[n]
 run_data.append(data)
# Distribute the small source endpoint mismatch over the cycle rather than snap at the seam.
for i,data in enumerate(run_data):
 u=i/16
 for n in names:
  loc,q,sc=data[n];start=run_data[0][n];last=run_data[-1][n]
  data[n]=(loc+(start[0]-last[0])*u,q.slerp(q@(last[1].inverted()@start[1]),u),sc)
run_data[-1]={n:tuple(v.copy() for v in vals) for n,vals in run_data[0].items()};runclip=action_from('Run',run_data)
# Separate walk gait: alternating planted stance and low swing, with solved knees.
rest={b.name:b.matrix_local.copy() for b in r.data.bones};walk_data=[]
for f in range(25):
 r.animation_data.action=None;apply(base);theta=2*math.pi*f/24;root=r.pose.bones['mixamorig:Hips'];rootmat=rest[root.name].copy();rootmat.translation+=Vector((.006*math.sin(theta),0,-.048+.004*math.cos(theta*2)));root.matrix=rootmat;bpy.context.view_layer.update()
 for side,offset in [('Left',0),('Right',.5)]:
  phase=(f/24+offset)%1;ankle=r.data.bones['mixamorig:'+side+'Foot'].head_local.copy()
  if phase<.60:
   u=phase/.60;ankle.y+=-.18+.36*u;lift=0;pitch=.13*(1-min(u/.2,1))-.20*max(0,(u-.72)/.28)
  else:
   u=(phase-.60)/.40;ankle.y+=.18-.36*(u*u*(3-2*u));lift=.068*math.sin(math.pi*u);pitch=-.45*math.sin(math.pi*u)
  ankle.z+=lift
  upperb=r.pose.bones['mixamorig:'+side+'UpLeg'];lower=r.pose.bones['mixamorig:'+side+'Leg'];foot=r.pose.bones['mixamorig:'+side+'Foot'];hip=upperb.head.copy();axis=(ankle-hip).normalized();distance=(ankle-hip).length;L1=upperb.bone.length;L2=lower.bone.length;d=(L1*L1-L2*L2+distance*distance)/(2*distance);h=math.sqrt(max(0,L1*L1-d*d));pole=Vector((.1 if side=='Left' else -.1,-1,0));bend=(pole-axis*pole.dot(axis)).normalized();knee=hip+axis*d+bend*h
  for b,start,end in [(upperb,hip,knee),(lower,knee,ankle)]:
   rot=rest[b.name].to_3x3().col[1].rotation_difference((end-start).normalized());m=(rot.to_matrix()@rest[b.name].to_3x3()).to_4x4();m.translation=start;b.matrix=m;bpy.context.view_layer.update()
  m=(Matrix.Rotation(-pitch,3,'X')@rest[foot.name].to_3x3()).to_4x4();m.translation=ankle;foot.matrix=m;bpy.context.view_layer.update()
 walk_data.append({n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names})
walk_data[-1]=walk_data[0];walk=action_from('Walk',walk_data)
shoot_data=[]
for f in range(10):
 r.animation_data.action=None;apply(base);u=f/9;recoil=math.sin(math.pi*min(1,u/.3))*0 if False else (math.sin(math.pi*u)**2)*math.exp(-3*u)*2
 for n,angle in [('mixamorig:RightArm',-6),('mixamorig:RightForeArm',-13),('mixamorig:RightHand',-10),('mixamorig:Spine2',-1.5)]:
  b=r.pose.bones[n];b.rotation_quaternion=b.rotation_quaternion@Quaternion((1,0,0),math.radians(angle*recoil))
 shoot_data.append({n:(r.pose.bones[n].location.copy(),r.pose.bones[n].rotation_quaternion.copy(),r.pose.bones[n].scale.copy()) for n in names})
shoot=action_from('Shoot',shoot_data)
# Merge editable weapon parts into one export mesh, preserving material slots.
props=bpy.data.collections['Woolly_Revolver_Prop'];socket=bpy.data.objects['Revolver_HandSocket'];r.animation_data.action=idle;s.frame_set(1);bpy.context.view_layer.update()
parts=[x for x in props.objects if x.type=='MESH']
for ob in bpy.context.selected_objects:ob.select_set(False)
for ob in parts:
 ob.select_set(True);bpy.context.view_layer.objects.active=ob
 for mod in list(ob.modifiers):
  try:bpy.ops.object.modifier_apply(modifier=mod.name)
  except RuntimeError:pass
bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();weapon=bpy.context.object;weapon.name='Revolver_GameMesh'
muzzle=bpy.data.objects.new('Muzzle',None);s.collection.objects.link(muzzle);muzzle.parent=socket;muzzle.location=(0,.241,.11)
# Only export the four finished clips, not iterations or control shapes.
for track in list(r.animation_data.nla_tracks):r.animation_data.nla_tracks.remove(track)
keep=[idle,walk,runclip,shoot]
for a in list(bpy.data.actions):
 if a not in keep:bpy.data.actions.remove(a)
r.animation_data.action=idle;s.frame_start=1;s.frame_end=73;s.frame_set(1)
for ob in bpy.context.selected_objects:ob.select_set(False)
for ob in [r,body,socket,weapon,muzzle]:ob.hide_set(False);ob.select_set(True)
bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(out/'Woolly.fbx'),use_selection=True,object_types={'ARMATURE','MESH','EMPTY'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,use_armature_deform_only=False,use_mesh_modifiers=True,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Unity_AnimationSource.blend'))
(p/'unity_export_report.json').write_text(json.dumps({'clips':{a.name:list(a.frame_range) for a in keep},'fps':24,'bones':len(r.data.bones),'weapon_vertices':len(weapon.data.vertices),'fbx':str(out/'Woolly.fbx')},indent=2))
