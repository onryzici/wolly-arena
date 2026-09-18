import bpy,numpy as np,math,json
from mathutils import Vector,Matrix
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Refined_v3.blend'))
s=bpy.context.scene;o=bpy.data.objects['Woolly_Body'];r=bpy.data.objects['Woolly_Rig'];original=r.animation_data.action;original.use_fake_user=True;fixed=original.copy();fixed.name='Running_SoleContact_V4';r.animation_data.action=fixed
fits={};bones=[r.pose.bones['mixamorig:'+side+'Foot'] for side in ['Left','Right']]
for side,sign in [('Left',1),('Right',-1)]:
 vs=np.array([v.co[:] for v in o.data.vertices if v.co.x*sign>0 and v.co.z<.065]);pts=[]
 for x in np.linspace(vs[:,0].min(),vs[:,0].max(),9)[:-1]:
  for y in np.linspace(vs[:,1].min(),vs[:,1].max(),13)[:-1]:
   a=vs[(vs[:,0]>=x)&(vs[:,0]<x+np.ptp(vs[:,0])/8)&(vs[:,1]>=y)&(vs[:,1]<y+np.ptp(vs[:,1])/12)]
   if len(a):pts.append(a[a[:,2].argmin()])
 a=np.array(pts);coeff=np.linalg.lstsq(np.c_[a[:,:2],np.ones(len(a))],a[:,2],rcond=None)[0]
 normal=Vector((-coeff[0],-coeff[1],1)).normalized();toe=r.data.bones['mixamorig:'+side+'ToeBase'];f=toe.tail_local-toe.head_local;f=f-normal*f.dot(normal);f.normalize();right=normal.cross(f).normalized()
 fits['mixamorig:'+side+'Foot']=(Matrix((right,-f,normal)).transposed(),normal,list(map(float,coeff)))
def smooth(a,b,x):
 t=max(0,min(1,(x-a)/(b-a)));return t*t*(3-2*t)
samples=[];previous={}
for i in range(65):
 t=1+i/4;s.frame_set(int(t),subframe=t-int(t))
 for b in bones:
  rest=b.bone.matrix_local.to_3x3();delta=b.matrix.to_3x3()@rest.inverted();basis,n,coeff=fits[b.name]
  forward=-(delta@basis).col[1];old_pitch=math.atan2(forward.z,-forward.y)
  # Fully level during low stance, smoothly blend back during airborne recovery.
  airborne=smooth(.205,.30,b.head.z);pitch=old_pitch*airborne
  f=Vector((0,-math.cos(pitch),math.sin(pitch)));right=Vector((1,0,0));up=right.cross(-f)
  target_basis=Matrix((right,-f,up)).transposed();desired=(target_basis@basis.inverted()@rest).to_4x4();desired.translation=b.matrix.translation
  local=b.bone.convert_local_to_pose(desired,b.bone.matrix_local,parent_matrix=b.parent.matrix,parent_matrix_local=b.parent.bone.matrix_local,invert=True);q=local.to_quaternion()
  if b.name in previous and previous[b.name].dot(q)<0:q.negate()
  previous[b.name]=q.copy();samples.append((t,b.name,q))
for layer in fixed.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in list(bag.fcurves):
    if any(fc.data_path==b.path_from_id('rotation_quaternion') for b in bones):bag.fcurves.remove(fc)
for t,name,q in samples:
 b=r.pose.bones[name];b.rotation_quaternion=q;b.keyframe_insert(data_path='rotation_quaternion',frame=t,group=name)
for layer in fixed.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in bag.fcurves:
    if any(fc.data_path==b.path_from_id('rotation_quaternion') for b in bones):
     for key in fc.keyframe_points:key.interpolation='LINEAR'
errors=[]
for i in range(129):
 t=1+i/8;s.frame_set(int(t),subframe=t-int(t))
 for b in bones:
  if b.head.z<=.205:
   n=b.matrix.to_3x3()@b.bone.matrix_local.to_3x3().inverted()@fits[b.name][1]
   errors.append(math.degrees(n.angle(Vector((0,0,1)))))
report={'action':fixed.name,'sole_fit_coefficients':{k:v[2] for k,v in fits.items()},'max_stance_sole_plane_tilt_degrees':max(errors),'stance_measurements':len(errors),'note':'Best-fit outsole support plane aligned during stance; individual sculpted tread lugs retain their original irregularities.'}
(p/'sole_contact_report.json').write_text(json.dumps(report,indent=2));print(report)
s.frame_set(7);bpy.data.texts['READ_ME'].write('\nV4: actual best-fit outsole planes used to correct both feet. Stance below ankle height 0.205 aligns support plane horizontally; smooth transition to swing by 0.30. Sculpted tread geometry unchanged. Frame 7 shows left stance.\n');bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_SoleContact_v4.blend'))
# Matched close-up images, with a horizontal reference grid supplied by a plane.
s.render.resolution_x=1100;s.render.resolution_y=850;s.cycles.samples=32
for label,act in [('before',original),('after',fixed)]:
 r.animation_data.action=act;s.frame_set(7);bpy.context.view_layer.update();b=r.pose.bones['mixamorig:LeftFoot'];center=b.head+Vector((0,-.04,-.07));cam=s.camera;cam.data.ortho_scale=.65;cam.location=center+Vector((0,-3,.10));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('sole_contact_'+label+'.png'));bpy.ops.render.render(write_still=True)
