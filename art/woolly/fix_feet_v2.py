import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Editable.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];original=r.animation_data.action;original.use_fake_user=True
fixed=original.copy();fixed.name='Running_Feet_Flatter_V2';r.animation_data.action=fixed
bones=[r.pose.bones['mixamorig:'+side+'Foot'] for side in ['Left','Right']]
# Sample original animation before changing any curves; keep sagittal ankle flexion.
times=set([1+i/2 for i in range(33)])
for layer in original.layers:
 for st in layer.strips:
  for bag in st.channelbags:
   for fc in bag.fcurves:
    if any(b.name in fc.data_path for b in bones):times.update(float(k.co.x) for k in fc.keyframe_points)
samples=[];metrics=[];prev={}
for time in sorted(times):
 s.frame_set(int(time),subframe=time-int(time))
 for b in bones:
  rest=b.bone.matrix_local.to_3x3();delta=b.matrix.to_3x3()@rest.inverted()
  toe=r.data.bones[b.name.replace('Foot','ToeBase')]
  forward=toe.tail_local-toe.head_local;forward.z=0;forward.normalize()
  up=Vector((0,0,1));right=up.cross(forward).normalized()
  rest_basis=Matrix((right,-forward,up)).transposed()
  actual_forward=delta@forward;actual_right=delta@right
  original_pitch=math.atan2(actual_forward.z,-actual_forward.y)
  pitch=math.radians(12)*math.tanh(original_pitch*0.4/math.radians(12))
  target_forward=Vector((0,-math.cos(pitch),math.sin(pitch)))
  target_right=Vector((1,0,0));target_up=target_right.cross(-target_forward)
  target_basis=Matrix((target_right,-target_forward,target_up)).transposed()
  desired=(target_basis@rest_basis.inverted()@rest).to_4x4();desired.translation=b.matrix.translation
  local=b.bone.convert_local_to_pose(desired,b.bone.matrix_local,parent_matrix=b.parent.matrix,parent_matrix_local=b.parent.bone.matrix_local,invert=True)
  q=local.to_quaternion()
  if b.name in prev and q.dot(prev[b.name])<0:q.negate()
  prev[b.name]=q.copy();samples.append((time,b.name,q))
  metrics.append({'frame':time,'bone':b.name,'lateral_tilt_before_degrees':math.degrees(math.asin(max(-1,min(1,actual_right.z))))})
# Replace only the two foot rotation channels in a separate editable action.
for layer in fixed.layers:
 for st in layer.strips:
  for bag in st.channelbags:
   for fc in list(bag.fcurves):
    if any(fc.data_path==b.path_from_id('rotation_quaternion') for b in bones):bag.fcurves.remove(fc)
for time,name,q in samples:
 b=r.pose.bones[name];b.rotation_quaternion=q;b.keyframe_insert(data_path='rotation_quaternion',frame=time,group=name)
for layer in fixed.layers:
 for st in layer.strips:
  for bag in st.channelbags:
   for fc in bag.fcurves:
    if any(fc.data_path==b.path_from_id('rotation_quaternion') for b in bones):
     for k in fc.keyframe_points:k.interpolation='LINEAR'
# Keep rigid boot toes in their rest orientation relative to the corrected foot.
for side in ['Left','Right']:
 toe_bone=r.pose.bones['mixamorig:'+side+'ToeBase']
 for layer in fixed.layers:
  for st in layer.strips:
   for bag in st.channelbags:
    for fc in list(bag.fcurves):
     if fc.data_path==toe_bone.path_from_id('rotation_quaternion'):bag.fcurves.remove(fc)
 toe_bone.rotation_quaternion=(1,0,0,0)
 for time in sorted(times):toe_bone.keyframe_insert(data_path='rotation_quaternion',frame=time,group=toe_bone.name)

# Check the corrected orientation at samples and intermediate frames.
max_roll=0; max_pitch=0
for i in range(65):
 t=1+i/4;s.frame_set(int(t),subframe=t-int(t))
 for b in bones:
  toe=r.data.bones[b.name.replace('Foot','ToeBase')];f=toe.tail_local-toe.head_local;f.z=0;f.normalize();right=Vector((0,0,1)).cross(f).normalized()
  delta_check=b.matrix.to_3x3()@b.bone.matrix_local.to_3x3().inverted()
  forward_check=delta_check@f
  max_pitch=max(max_pitch,abs(math.degrees(math.atan2(forward_check.z,-forward_check.y))))
  actual=b.matrix.to_3x3()@b.bone.matrix_local.to_3x3().inverted()@right
  max_roll=max(max_roll,abs(math.degrees(math.asin(max(-1,min(1,actual.z))))))
report={'original_action':original.name,'corrected_action':fixed.name,'max_lateral_tilt_before_degrees':max(abs(x['lateral_tilt_before_degrees']) for x in metrics),'max_lateral_tilt_after_degrees':max_roll,'max_pitch_after_degrees':max_pitch,'changed_bones':[b.name for b in bones],'samples':metrics}
(p/'foot_correction_v2_report.json').write_text(json.dumps(report,indent=2));print({k:v for k,v in report.items() if k!='samples'})
s.frame_set(1)
bpy.data.texts['READ_ME'].write('\nFoot correction: Running_Feet_Flatter_V2 is active. Original Running retained. Both ankle rotations aligned to running direction, forward/backward flexion softened and limited to 12 degrees. Foot and toe quaternion channels changed; boot toe flex neutralized.\n')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_FeetFixed_v2.blend'))
s.render.resolution_x=900;s.render.resolution_y=900
for frame in [1,5,9]:
 s.frame_set(frame);s.render.filepath=str(p/('feet_fixed_v2_%02d.png'%frame));bpy.ops.render.render(write_still=True)

s.frame_set(5)
from mathutils import Vector
cam=s.camera;center=Vector((0,0,.8));cam.location=center+Vector((3,0,.2));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/'feet_fixed_v2_side05.png');bpy.ops.render.render(write_still=True)
