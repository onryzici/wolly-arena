import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Revolver_v8.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];o=bpy.data.objects['Woolly_Body'];handname='mixamorig:RightHand';handrest=r.data.bones[handname].matrix_local.copy();inv=handrest.inverted();oldaction=r.animation_data.action;oldaction.use_fake_user=True;action=oldaction.copy();action.name='Idle_Revolver_Grip_3s';action.use_fake_user=True;r.animation_data.action=action
backup=o.data.copy();backup.name='Woolly_PreFingerRig_Backup';backup.use_fake_user=True
specs={}
for name,z,start,joint,end in [('Index',.068,.118,.177,.230),('Middle',-.01,.128,.190,.245),('Ring',-.079,.105,.158,.208)]:
 specs['Grip_R_'+name+'_01']=((0,start,z),(0,joint,z),handname)
 specs['Grip_R_'+name+'_02']=((0,joint,z),(0,end,z),'Grip_R_'+name+'_01')
specs['Grip_R_Thumb_01']=((-.028,.060,.072),(-.061,.103,.097),handname)
specs['Grip_R_Thumb_02']=((-.061,.103,.097),(-.095,.147,.104),'Grip_R_Thumb_01')
for ob in bpy.context.selected_objects:ob.select_set(False)
r.select_set(True);bpy.context.view_layer.objects.active=r;bpy.ops.object.mode_set(mode='EDIT')
for name,(a,b,parent) in specs.items():
 bone=r.data.edit_bones.new(name);bone.head=handrest@Vector(a);bone.tail=handrest@Vector(b);bone.parent=r.data.edit_bones[parent];bone.use_connect=name.endswith('_02');bone.align_roll(handrest.to_3x3().col[2]);bone.use_deform=True
bpy.ops.object.mode_set(mode='OBJECT')
for name in specs:
 o.vertex_groups.new(name=name);r.pose.bones[name].rotation_mode='QUATERNION'
def smooth(a,b,x):
 t=max(0,min(1,(x-a)/(b-a)));return t*t*(3-2*t)
handgroups=[o.vertex_groups[handname].index,o.vertex_groups['mixamorig:RightHandMiddle4'].index];changed=0
for v in o.data.vertices:
 weights={g.group:g.weight for g in v.groups};weight=sum(weights.get(i,0) for i in handgroups)
 if weight<1e-7:continue
 x,y,z=inv@v.co
 thumb=smooth(.027,.063,-x)*smooth(.036,.075,z)*(1-smooth(.16,.19,y))
 digit=smooth(.095,.145,y)*(1-thumb);tw=thumb*smooth(.045,.095,y)
 allocation={handname:weight*(1-digit-tw)}
 rows=[('Index',.068,.177),('Middle',-.01,.190),('Ring',-.079,.158)]
 scores=[math.exp(-.5*((z-center)/.018)**2) for _,center,_ in rows];den=sum(scores) or 1
 for (name,center,joint),score in zip(rows,scores):
  amount=weight*digit*score/den;dist=smooth(joint-.017,joint+.025,y);allocation['Grip_R_'+name+'_01']=amount*(1-dist);allocation['Grip_R_'+name+'_02']=amount*dist
 dist=smooth(.09,.13,y);allocation['Grip_R_Thumb_01']=weight*tw*(1-dist);allocation['Grip_R_Thumb_02']=weight*tw*dist
 for index in handgroups:o.vertex_groups[index].remove([v.index])
 for name,w in allocation.items():
  if w>1e-8:o.vertex_groups[name].add([v.index],w,'REPLACE')
 changed+=1
# Smooth local skin weights across neighboring hand vertices to avoid web spikes.
local_ids=[o.vertex_groups[n].index for n in specs]+[o.vertex_groups[handname].index]
neighbors={v.index:set() for v in o.data.vertices}
for edge in o.data.edges:
 a,b=edge.vertices;neighbors[a].add(b);neighbors[b].add(a)
values={v.index:{g.group:g.weight for g in v.groups if g.group in local_ids} for v in o.data.vertices}
for iteration in range(3):
 updated={}
 for i,w in values.items():
  total=sum(w.values());near=[j for j in neighbors[i] if sum(values[j].values())>.2]
  if total<.25 or not near:updated[i]=w;continue
  avg={g:sum(values[j].get(g,0)/max(1e-8,sum(values[j].values())) for j in near)/len(near) for g in local_ids}
  updated[i]={g:.65*w.get(g,0)+.35*total*avg[g] for g in local_ids}
 values=updated
for i,w in values.items():
 for g,weight in w.items():
  if weight>1e-8:o.vertex_groups[g].add([i],weight,'REPLACE')
vg=o.vertex_groups.new(name='Grip_LocalSmoothing')
for v in o.data.vertices:
 amount=sum(g.weight for g in v.groups if o.vertex_groups[g.group].name.startswith('Grip_R_'))
 if amount>.02:vg.add([v.index],min(1,amount),'REPLACE')
modifier=o.modifiers.new('Gentle finger deformation smoothing','CORRECTIVE_SMOOTH');modifier.vertex_group=vg.name;modifier.factor=.65;modifier.iterations=6
# Orient palm vertically along the revolver grip, preserving the aiming direction.
# Author explicit open finger defaults in the other actions so switching clips resets the hand.
for a in list(bpy.data.actions):
 if a==action:continue
 r.animation_data.action=a
 for name in specs:
  b=r.pose.bones[name];b.rotation_quaternion=(1,0,0,0)
  for f in [a.frame_range[0],a.frame_range[1]]:b.keyframe_insert(data_path='rotation_quaternion',frame=f,group=name)
r.animation_data.action=action
# Derive a stable palm roll from world up at each frame, while preserving hand position and forward aim.
samples=[];previous=None
for f in range(1,74):
 s.frame_set(f);bpy.context.view_layer.update();hand=r.pose.bones[handname];forward=(hand.tail-hand.head).normalized();up=(Vector((0,0,1))-forward*forward.dot(Vector((0,0,1)))).normalized();x=forward.cross(up).normalized();desired=Matrix((x,forward,up)).transposed().to_4x4();desired.translation=hand.head
 local=hand.bone.convert_local_to_pose(desired,hand.bone.matrix_local,parent_matrix=hand.parent.matrix,parent_matrix_local=hand.parent.bone.matrix_local,invert=True);q=local.to_quaternion()
 if previous and previous.dot(q)<0:q.negate()
 previous=q.copy();samples.append((f,q))
for layer in action.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in list(bag.fcurves):
    if fc.data_path==r.pose.bones[handname].path_from_id('rotation_quaternion'):bag.fcurves.remove(fc)
angles={'Index':(30,50),'Middle':(58,67),'Ring':(63,67)}
for f,q in samples:
 hand=r.pose.bones[handname];hand.rotation_quaternion=q;hand.keyframe_insert(data_path='rotation_quaternion',frame=f,group=handname)
 for name in specs:
  b=r.pose.bones[name]
  if 'Thumb' in name:b.rotation_quaternion=Quaternion((0,1,0),math.radians(-15 if name.endswith('01') else -12))@Quaternion((1,0,0),math.radians(20 if name.endswith('01') else 25))
  else:
   digit=name.split('_')[2];a=angles[digit][0 if name.endswith('01') else 1];b.rotation_quaternion=Quaternion((0,0,1),math.radians(a))
  b.keyframe_insert(data_path='rotation_quaternion',frame=f,group=name)
for layer in action.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in bag.fcurves:
    if 'Grip_R_' in fc.data_path or fc.data_path==r.pose.bones[handname].path_from_id('rotation_quaternion'):
     for k in fc.keyframe_points:k.interpolation='LINEAR'
     if not any(m.type=='CYCLES' for m in fc.modifiers):fc.modifiers.new('CYCLES')
s.frame_set(1);bpy.context.view_layer.update();hand=r.pose.bones[handname];root=bpy.data.objects['Revolver_HandSocket']
# Gun local Y points down the palm, local Z follows the stacked finger rows.
size=1.20;local=Matrix.Identity(4);local.translation=Vector((-.041,.116+.035*size,-.006+.048*size));local=local@Matrix.Diagonal((size,size,size,1));root.matrix_world=hand.matrix@local;bpy.context.view_layer.update()
r.data.show_names=False
for b in r.pose.bones:
 if b.name.startswith('Grip_R_'):b.custom_shape=None
bpy.data.texts['REVOLVER_README'].write('\nV9: prop enlarged 20 percent; grip aligned inside right palm. Added 8 deform bones: Index/Middle/Ring/Thumb, two joints each. Reweighted existing mitten geometry without changing topology or UVs. New Idle_Revolver_Grip_3s active. Other clips key new finger bones open.\n')
s.frame_set(1);s.frame_start=1;s.frame_end=72
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_GripRig_v9.blend'))
report={'new_bones':list(specs),'vertices_reweighted':changed,'weapon_scale':size,'active_action':action.name,'max_weight_sum_error':max(abs(sum(g.weight for g in v.groups if g.group!=vg.index)-1) for v in o.data.vertices),'unweighted_vertices':sum(not v.groups for v in o.data.vertices)}
(p/'grip_rig_report.json').write_text(json.dumps(report,indent=2));print(report)
cam=s.camera;s.render.resolution_x=1000;s.render.resolution_y=1000;s.cycles.samples=24
for name,vec in [('side',(-2,-.5,.4)),('front',(-.5,-2,.4)),('inner',(2,-.5,.4))]:
 s.frame_set(37);bpy.context.view_layer.update();center=r.pose.bones[handname].head+Vector((0,-.11,.03));cam.data.ortho_scale=.63;cam.location=center+Vector(vec);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('grip_v9_'+name+'.png'));bpy.ops.render.render(write_still=True)
