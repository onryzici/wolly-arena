import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_PaintFix_v6.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];o=bpy.data.objects['Woolly_Body'];run=r.animation_data.action;run.use_fake_user=True
idle=bpy.data.actions.new('Idle_Breathing_3s');idle.use_fake_user=True;r.animation_data.action=idle;r.data.pose_position='POSE'
for tr in r.animation_data.nla_tracks:tr.mute=True
rest={b.name:b.matrix_local.copy() for b in r.data.bones};fits=json.loads((p/'sole_contact_report.json').read_text())['sole_fit_coefficients'];feet={}
for side in ['Left','Right']:
 name='mixamorig:'+side+'Foot';a,b,c=fits[name];normal=Vector((-a,-b,1)).normalized();rot=normal.rotation_difference(Vector((0,0,1)));anchor=r.data.bones[name].head_local.copy()
 indices=[v.index for v in o.data.vertices if v.co.z<.14 and (v.co.x>0)==(side=='Left')]
 bottom=min((rot@(o.data.vertices[i].co-anchor)+anchor).z for i in indices);anchor.z-=bottom
 feet[side]=(anchor,(rot.to_matrix()@rest[name].to_3x3()).to_4x4())
def world_local_rotation(name,axis,degrees):
 q=rest[name].to_quaternion();return q.inverted()@Quaternion(Vector(axis),math.radians(degrees))@q
previous={};foot_positions=[]
for f in range(1,74):
 s.frame_set(f);t=2*math.pi*((f-1)%72)/72;breath=math.sin(t);sway=math.sin(t+.4)
 for b in r.pose.bones:b.location=(0,0,0);b.rotation_mode='QUATERNION';b.rotation_quaternion=(1,0,0,0);b.scale=(1,1,1)
 root=r.pose.bones['mixamorig:Hips'];rootmat=rest[root.name].copy();rootmat.translation+=Vector((.003*sway,0,-.016+.003*breath));root.matrix=rootmat
 for name,amount in [('Spine',.35),('Spine1',.5),('Spine2',.45)]:r.pose.bones['mixamorig:'+name].rotation_quaternion=world_local_rotation('mixamorig:'+name,(1,0,0),amount*breath)
 r.pose.bones['mixamorig:Head'].rotation_quaternion=world_local_rotation('mixamorig:Head',(0,0,1),.8*math.sin(t-.3))@world_local_rotation('mixamorig:Head',(1,0,0),-.45*breath)
 for side,sign in [('Left',1),('Right',-1)]:
  for name,axis,angle in [('Arm',(0,1,0),sign*(16+.5*math.sin(t-.25))),('ForeArm',(1,0,0),-7+.7*math.sin(t-.5)),('Hand',(1,0,0),1.0*math.sin(t-.65))]:
   bn='mixamorig:'+side+name;r.pose.bones[bn].rotation_quaternion=world_local_rotation(bn,axis,angle)
 bpy.context.view_layer.update()
 for side in ['Left','Right']:
  upper=r.pose.bones['mixamorig:'+side+'UpLeg'];lower=r.pose.bones['mixamorig:'+side+'Leg'];foot=r.pose.bones['mixamorig:'+side+'Foot'];ankle,footmat=feet[side];hip=upper.head.copy();axis=(ankle-hip).normalized();distance=(ankle-hip).length;L1=upper.bone.length;L2=lower.bone.length
  d=(L1*L1-L2*L2+distance*distance)/(2*distance);height=math.sqrt(max(0,L1*L1-d*d));pole=Vector((.12 if side=='Left' else -.12,-1,0));bend=(pole-axis*pole.dot(axis)).normalized();knee=hip+axis*d+bend*height
  for bone,start,end in [(upper,hip,knee),(lower,knee,ankle)]:
   base=rest[bone.name];q=base.to_3x3().col[1].rotation_difference((end-start).normalized());desired=(q.to_matrix()@base.to_3x3()).to_4x4();desired.translation=start;bone.matrix=desired;bpy.context.view_layer.update()
  desired=footmat.copy();desired.translation=ankle;foot.matrix=desired;bpy.context.view_layer.update()
 foot_positions.append([tuple(r.pose.bones['mixamorig:'+side+'Foot'].head) for side in ['Left','Right']])
 for bone in r.pose.bones:
  q=bone.rotation_quaternion.copy()
  if bone.name in previous and q.dot(previous[bone.name])<0:q.negate();bone.rotation_quaternion=q
  previous[bone.name]=q.copy()
  bone.keyframe_insert(data_path='rotation_quaternion',frame=f,group=bone.name);bone.keyframe_insert(data_path='location',frame=f,group=bone.name)
for layer in idle.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in bag.fcurves:
    for key in fc.keyframe_points:key.interpolation='LINEAR'
    fc.modifiers.new('CYCLES')
# Verify baked action including evaluated mesh seam and stationary soles.
def evaluated_points(frame):
 s.frame_set(frame);ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh();pts=[v.co.copy() for v in m.vertices];ev.to_mesh_clear();return pts
start=evaluated_points(1);end=evaluated_points(73);loop_error=max((a-b).length for a,b in zip(start,end))
max_drift=0
for frame in range(1,74):
 s.frame_set(frame)
 for side in ['Left','Right']:max_drift=max(max_drift,(r.pose.bones['mixamorig:'+side+'Foot'].head-feet[side][0]).length)
report={'action':idle.name,'duration_seconds':3,'fps':24,'playback_frames':[1,72],'loop_closing_key':73,'foot_drift_max':max_drift,'mesh_loop_seam_max':loop_error,'retained_run_action':run.name}
(p/'idle_report.json').write_text(json.dumps(report,indent=2));print(report)
s.render.fps=24;s.frame_start=1;s.frame_end=72;s.frame_set(1)
s.timeline_markers.clear()
for name,frame in [('IDLE START',1),('INHALE',19),('EXHALE',55),('LOOP',73)]:s.timeline_markers.new(name,frame=frame)
bpy.data.texts['READ_ME'].write('\nV7 IDLE: Idle_Breathing_3s is active (24 fps, frames 1-72; frame 73 duplicates frame 1). Subtle breathing, head/arm follow-through, relaxed stance with analytically planted feet. Baked editable bone keys, no runtime constraints. Running_NaturalContact_V5 preserved; set frame range 1-17 to preview it. V6 cuff material correction retained.\n')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_Idle_v7.blend'))
# Full character previews without opening windows.
cam=s.camera;center=Vector((0,0,.82));cam.data.ortho_scale=1.90;cam.location=center+Vector((.6,-3,.25));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.resolution_x=900;s.render.resolution_y=1000;s.cycles.samples=20
for frame in [1,19,55]:
 s.frame_set(frame);s.render.filepath=str(p/('idle_v7_%02d.png'%frame));bpy.ops.render.render(write_still=True)
