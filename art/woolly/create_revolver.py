import bpy,math,json
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Idle_v7.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];body=bpy.data.objects['Woolly_Body'];base=r.animation_data.action;base.use_fake_user=True
armed=base.copy();armed.name='Idle_Revolver_Aim_3s';armed.use_fake_user=True
names=['mixamorig:RightArm','mixamorig:RightForeArm','mixamorig:RightHand'];samples=[];prev={}
for f in range(1,74):
 r.animation_data.action=base;s.frame_set(f);t=2*math.pi*((f-1)%72)/72;lift=(1-math.cos(t))/2
 for name,direction in [(names[0],Vector((-.22,-.50-.12*lift,-.82+.08*lift))),(names[1],Vector((-.10,-1,.16+.12*lift))),(names[2],Vector((-.08,-1,.13+.12*lift)))]:
  b=r.pose.bones[name];head=b.head.copy();rest=b.bone.matrix_local;rot=rest.to_3x3().col[1].rotation_difference(direction.normalized());desired=(rot.to_matrix()@rest.to_3x3()).to_4x4();desired.translation=head;b.matrix=desired;bpy.context.view_layer.update()
  q=b.rotation_quaternion.copy()
  if name in prev and q.dot(prev[name])<0:q.negate()
  prev[name]=q.copy();samples.append((f,name,q,b.location.copy()))
r.animation_data.action=armed
for layer in armed.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in list(bag.fcurves):
    if any(fc.data_path in [r.pose.bones[n].path_from_id('rotation_quaternion'),r.pose.bones[n].path_from_id('location')] for n in names):bag.fcurves.remove(fc)
for f,name,q,loc in samples:
 b=r.pose.bones[name];b.rotation_quaternion=q;b.location=loc;b.keyframe_insert(data_path='rotation_quaternion',frame=f,group=name);b.keyframe_insert(data_path='location',frame=f,group=name)
for layer in armed.layers:
 for strip in layer.strips:
  for bag in strip.channelbags:
   for fc in bag.fcurves:
    if any(n in fc.data_path for n in names):
     for k in fc.keyframe_points:k.interpolation='LINEAR'
     fc.modifiers.new('CYCLES')
# Original stylized game prop, built as editable mesh parts.
col=bpy.data.collections.new('Woolly_Revolver_Prop');s.collection.children.link(col)
def mat(name,color,metal=0,rough=.5):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(*color,1);bs.inputs['Metallic'].default_value=metal;bs.inputs['Roughness'].default_value=rough;return m
steel=mat('Revolver | Midnight steel',(.065,.11,.20),.55,.38);edge=mat('Revolver | Blue steel edges',(.18,.28,.42),.6,.32);black=mat('Revolver | Recesses',(.009,.014,.027),.1,.6);gripmat=mat('Revolver | Amber grip',(.33,.16,.035),.05,.62);gold=mat('Revolver | Brass accents',(.7,.40,.075),.65,.32)
parts=[]
def finish(obj,name,material,bevel=0):
 obj.name=name
 for c in list(obj.users_collection):c.objects.unlink(obj)
 col.objects.link(obj);obj.data.materials.append(material)
 if bevel:
  mod=obj.modifiers.new('Soft stylized edges','BEVEL');mod.width=bevel;mod.segments=2
  mod=obj.modifiers.new('Weighted highlights','WEIGHTED_NORMAL')
 parts.append(obj);return obj
def box(name,loc,scale,material,bevel=.006,angle=0):
 bpy.ops.mesh.primitive_cube_add(size=1,location=loc);o=bpy.context.object;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.rotation_euler.x=angle;return finish(o,name,material,bevel)
def cyl(name,loc,radius,depth,material,vertices=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc,rotation=(math.pi/2,0,0));return finish(bpy.context.object,name,material,.002)
box('Revolver_Frame',(0,.015,.053),(.064,.13,.085),steel,.009)
box('Revolver_Grip',(0,-.035,-.048),(.053,.066,.14),gripmat,.013,-.22)
box('Revolver_GripBase',(0,-.049,-.113),(.061,.063,.018),edge,.004,-.22)
box('Revolver_TopStrap',(0,.038,.111),(.044,.138,.023),edge,.004)
cyl('Revolver_Cylinder',(0,.048,.069),.051,.076,steel)
for i in range(6):
 angle=i*2*math.pi/6;x=math.sin(angle)*.033;z=.069+math.cos(angle)*.033
 cyl('Cylinder_Chamber_%02d'%i,(x,.087,z),.010,.003,black,10)
 cyl('Cylinder_RearAccent_%02d'%i,(x,.009,z),.008,.002,gold,10)
# Hollow muzzle with a dark inner bore; no solid plug on the opening.
verts=[];faces=[];N=12
for y,rad in [(.084,.029),(.239,.029),(.239,.019),(.098,.019)]:
 for i in range(N):a=i*2*math.pi/N;verts.append((math.cos(a)*rad,y,.11+math.sin(a)*rad))
for ring in range(3):
 for i in range(N):j=(i+1)%N;faces.append((ring*N+i,ring*N+j,(ring+1)*N+j,(ring+1)*N+i))
mesh=bpy.data.meshes.new('Revolver_BarrelMesh');mesh.from_pydata(verts,[],faces);mesh.update();barrel=bpy.data.objects.new('Revolver_HollowBarrel',mesh);col.objects.link(barrel);barrel.data.materials.append(edge);barrel.data.materials.append(black)
for face in barrel.data.polygons:face.material_index=1 if face.index>=2*N else 0
parts.append(barrel);cyl('Barrel_DarkBack',(0,.099,.11),.018,.002,black)
box('Barrel_Underlug',(0,.164,.075),(.039,.125,.035),steel,.005)
box('Front_Sight',(0,.211,.144),(.012,.029,.020),gold,.003)
box('Rear_Sight',(0,-.019,.133),(.022,.024,.015),steel,.002)
box('Hammer',(0,-.052,.111),(.023,.031,.046),edge,.004,-.35)
bpy.ops.mesh.primitive_torus_add(major_segments=16,minor_segments=6,location=(0,.019,-.009),rotation=(0,math.pi/2,0),major_radius=.036,minor_radius=.006)
finish(bpy.context.object,'Trigger_Guard',edge)
box('Trigger',(0,.005,.003),(.012,.012,.030),gold,.003,-.3)
# Grip emblem on outer side.
bpy.ops.mesh.primitive_uv_sphere_add(segments=12,ring_count=6,radius=1,location=(-.028,-.035,-.045));emblem=bpy.context.object;emblem.scale=(.003,.012,.012);finish(emblem,'Grip_BrassStud',gold)
root=bpy.data.objects.new('Revolver_HandSocket',None);col.objects.link(root);root.empty_display_type='PLAIN_AXES';root.empty_display_size=.05
for obj in parts:obj.parent=root
s.frame_set(1);bpy.context.view_layer.update();hand=r.pose.bones['mixamorig:RightHand'];direction=(hand.tail-hand.head).normalized();gripcenter=hand.head+direction*.063
forward=Vector((-.08,-1,.13)).normalized();right=forward.cross(Vector((0,0,1))).normalized();up=right.cross(forward).normalized();world=Matrix((right,forward,up)).transposed().to_4x4();world.translation=gripcenter-world.to_3x3()@Vector((0,-.035,-.048))
root.parent=r;root.parent_type='BONE';root.parent_bone=hand.name;root.matrix_world=world;bpy.context.view_layer.update()
# Validate bone attachment, repeat seam, and preserve available actions.
initial=root.matrix_world.copy();s.frame_set(73);bpy.context.view_layer.update();seam=max(abs(initial[i][j]-root.matrix_world[i][j]) for i in range(4) for j in range(4))
report={'active_action':armed.name,'duration_seconds':3,'fps':24,'frames':[1,72],'seam_key':73,'prop_parts':len(parts),'parent_bone':root.parent_bone,'attachment_loop_matrix_error':seam,'preserved_actions':[a.name for a in bpy.data.actions]}
(p/'revolver_report.json').write_text(json.dumps(report,indent=2));print(report)
s.frame_start=1;s.frame_end=72;s.frame_set(1)
for obj in bpy.context.selected_objects:obj.select_set(False)
body.select_set(True);bpy.context.view_layer.objects.active=body
s.timeline_markers.clear()
for name,f in [('RELAXED AIM',1),('RAISE / AIM',37),('LOOP',73)]:s.timeline_markers.new(name,frame=f)
text=bpy.data.texts.new('REVOLVER_README');text.write('Stylized revolver: original editable primitive-based prop in Woolly_Revolver_Prop.\nRevolver_HandSocket is parented to mixamorig:RightHand.\nIdle_Revolver_Aim_3s: frames 1-72, 24fps, repeated endpoint at 73.\nSubtle raise/aim/return; no firing. Existing unarmed Idle_Breathing_3s and Running_NaturalContact_V5 retained.\nTo use unarmed actions, hide the Woolly_Revolver_Prop collection.\nThis is a visual game prop, not a functional weapon design.\n')
bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_Revolver_v8.blend'))
cam=s.camera;center=Vector((0,-.06,.83));cam.data.ortho_scale=1.95;s.render.resolution_x=1000;s.render.resolution_y=1100;s.cycles.samples=24
for frame,angle in [(1,(-1,-3,.6)),(37,(-1,-3,.6)),(37,(-3,-1,.6))]:
 s.frame_set(frame);cam.location=center+Vector(angle);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('revolver_v8_%02d_%s.png'%(frame,'side' if angle[0]==-3 else 'front')));bpy.ops.render.render(write_still=True)
