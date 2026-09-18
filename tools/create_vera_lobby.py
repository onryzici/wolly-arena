"""Offline lobby-only boxing pose/knuckles; retains Woolly's planted lower body."""
import bpy,math,bmesh,json
from pathlib import Path
from mathutils import Vector,Matrix
ROOT=Path(__file__).resolve().parents[1];ART=ROOT/'art/woolly/punk_vera'
bpy.ops.wm.open_mainfile(filepath=str(ART/'PunkVera_WoollyRig.blend'))
s=bpy.context.scene;r=bpy.data.objects['PunkVera_Rig'];body=bpy.data.objects['PunkVera_Body'];idle=bpy.data.actions['Idle'];frames=[]
# Copy actual idle keys, overriding only the upper arms, forearms and hands.
for f in range(1,74):
 r.animation_data.action=idle;s.frame_set(f);bpy.context.view_layer.update()
 base={b.name:(b.location.copy(),b.rotation_quaternion.copy(),b.scale.copy())for b in r.pose.bones}
 r.animation_data.action=None
 for name,(loc,q,scale)in base.items():b=r.pose.bones[name];b.location=loc;b.rotation_quaternion=q;b.scale=scale
 bpy.context.view_layer.update()
 def aim(name,direction):
  b=r.pose.bones[name];m=b.matrix.copy();q=m.to_3x3().col[1].rotation_difference(Vector(direction).normalized());target=(q.to_matrix()@m.to_3x3()).to_4x4();target.translation=m.translation;b.matrix=target;bpy.context.view_layer.update()
 breath=.018*math.sin((f-1)*math.pi/36)
 for side,sign in [('Left',1),('Right',-1)]:
  aim('mixamorig:'+side+'Arm',(sign*.52,-.27,-1))
  aim('mixamorig:'+side+'ForeArm',(-sign*.25,-.8,1+breath))
  aim('mixamorig:'+side+'Hand',(-sign*.1,-.85,.5))
 frames.append({b.name:(b.location.copy(),b.rotation_quaternion.copy(),b.scale.copy())for b in r.pose.bones})
a=idle.copy();a.use_fake_user=True
for action in list(bpy.data.actions):
 if action!=a:bpy.data.actions.remove(action)
a.name='Idle';r.animation_data.action=a
for f,data in enumerate(frames,1):
 for name,(loc,q,scale)in data.items():
  if name not in ['mixamorig:'+side+part for side in ['Left','Right']for part in ['Arm','ForeArm','Hand']]:continue
  b=r.pose.bones[name];b.location=loc;b.rotation_quaternion=q;b.scale=scale
  for prop in ['location','rotation_quaternion','scale']:b.keyframe_insert(data_path=prop,frame=f,group=name)
s.frame_set(1);bpy.context.view_layer.update()
# Cut away the open hand beyond the wrist; fist geometry overlaps this cut under the cuff.
remove=set()
for side in ['Left','Right']:
 hand=r.data.bones['mixamorig:'+side+'Hand'];axis=(hand.tail_local-hand.head_local).normalized()
 for v in body.data.vertices:
  weight=sum(g.weight for g in v.groups if side+'Hand' in body.vertex_groups[g.group].name)
  if weight>.45 and (v.co-hand.head_local).dot(axis)>-.014:remove.add(v.index)
bm=bmesh.new();bm.from_mesh(body.data);bm.verts.ensure_lookup_table();bmesh.ops.delete(bm,geom=[bm.verts[i]for i in remove],context='VERTS');bm.to_mesh(body.data);bm.free()
materials={}
for name,color in [('Vera_Fists',(.80,.77,.69,1)),('Vera_Knuckles',(.82,.54,.16,1)),('Vera_Grip',(.11,.065,.13,1)),('Vera_Pink',(.71,.09,.28,1))]:
 mat=bpy.data.materials.new(name);mat.diffuse_color=color;mat.use_nodes=True;node=mat.node_tree.nodes.get('Principled BSDF');node.inputs['Base Color'].default_value=color;node.inputs['Roughness'].default_value=.43;node.inputs['Metallic'].default_value=.45 if name=='Vera_Knuckles'else 0;materials[name]=mat
parts={name:[]for name in materials}
def piece(name,side,center,size,kind='sphere',rotation=None):
 if kind=='sphere':
  bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=10,location=center);o=bpy.context.object;o.scale=size
 elif kind=='ring':
  bpy.ops.mesh.primitive_torus_add(major_radius=.019,minor_radius=.007,major_segments=16,minor_segments=8,location=center);o=bpy.context.object
  o.rotation_euler=(math.pi/2,0,0)
 else:
  bpy.ops.mesh.primitive_cube_add(size=1,location=center);o=bpy.context.object;o.scale=size
  bevel=o.modifiers.new('Rounded','BEVEL');bevel.width=.12;bevel.segments=3
  bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=bevel.name)
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 # Bake world geometry into the hand bone's bind space and weight rigidly.
 world=o.matrix_world.copy();skin=r.pose.bones['mixamorig:'+side+'Hand'].matrix@r.data.bones['mixamorig:'+side+'Hand'].matrix_local.inverted()
 for v in o.data.vertices:v.co=skin.inverted()@world@v.co
 o.matrix_world=Matrix.Identity(4);o.parent=r;o.matrix_parent_inverse=Matrix.Identity(4)
 group=o.vertex_groups.new(name='mixamorig:'+side+'Hand');group.add(list(range(len(o.data.vertices))),1,'REPLACE');mod=o.modifiers.new('Woolly hand','ARMATURE');mod.object=r
 o.data.materials.append(materials[name]);o.name=name+'_'+side
 for poly in o.data.polygons:poly.use_smooth=True
 parts[name].append(o);return o
for side,sign in [('Left',1),('Right',-1)]:
 hand=r.pose.bones['mixamorig:'+side+'Hand'];center=hand.head+(hand.tail-hand.head).normalized()*.055
 piece('Vera_Fists',side,center,(.079,.064,.077))
 piece('Vera_Fists',side,center+Vector((-sign*.057,-.002,-.018)),(.033,.041,.047))
 piece('Vera_Grip',side,center+Vector((0,-.058,.002)),(.151,.027,.061),'box')
 for i in range(4):piece('Vera_Knuckles',side,center+Vector(((i-1.5)*.038,-.079,.013)),None,'ring')
 piece('Vera_Knuckles',side,center+Vector((0,-.075,-.018)),(.153,.021,.018),'box')
 piece('Vera_Pink',side,center+Vector((0,-.09,-.018)),(.045,.012,.021),'box')
# Consolidate by material, preserving each hand's weights.
accessories=[]
for name,objects in parts.items():
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True)
 bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();o=bpy.context.object;o.name=name;accessories.append(o)
r.animation_data.action=a;s.frame_set(1)
bpy.ops.object.select_all(action='DESELECT')
for o in [r,body]+accessories:o.select_set(True)
bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(ART/'PunkVeraLobby.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(ART/'PunkVera_LobbyBoxer.blend'))
s.render.resolution_x=800;s.render.resolution_y=900;s.cycles.samples=24;s.camera.data.ortho_scale=1.95
for name,pos in [('boxing-front.png',(0,-5,.85)),('boxing-quarter.png',(2,-5,1.65))]:
 s.camera.location=pos;s.camera.rotation_euler=(Vector((0,0,.85))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(ART/name);bpy.ops.render.render(write_still=True)
# Portrait uses the same model and guard, with no background geometry.
s.render.film_transparent=True;s.render.resolution_x=640;s.render.resolution_y=800;s.render.resolution_percentage=100
s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA'
s.camera.location=(1,-5,1.25);s.camera.rotation_euler=(Vector((0,0,.87))-s.camera.location).to_track_quat('-Z','Y').to_euler()
s.render.filepath=str(ART/'PunkVeraLobbyPortrait.png');bpy.ops.render.render(write_still=True)
