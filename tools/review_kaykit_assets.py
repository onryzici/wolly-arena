"""Validate exported animation/UV data and render an offline integration contact sheet."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];assets=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/KayKit'
bpy.ops.wm.read_factory_settings(use_empty=True);report=[]
names=['Skeleton_Minion','Skeleton_Rogue','Skeleton_Warrior','Skeleton_Mage']
for index,name in enumerate(names):
 before=set(bpy.context.scene.objects);before_actions=set(bpy.data.actions);bpy.ops.import_scene.fbx(filepath=str(assets/(name+'.fbx')))
 objects=set(bpy.context.scene.objects)-before;actions=set(bpy.data.actions)-before_actions;rig=next(o for o in objects if o.type=='ARMATURE')
 idle=next(a for a in actions if a.name.split('|')[-1].split('.')[0]=='Idle');run=next(a for a in actions if a.name.split('|')[-1].split('.')[0]=='Running_A')
 rig.animation_data.action=run
 if run.slots:rig.animation_data.action_slot=run.slots[0]
 foot=rig.pose.bones['foot.l'];bpy.context.scene.frame_set(int(run.frame_range[0])+2);a=foot.matrix.translation.copy();bpy.context.scene.frame_set(int((run.frame_range[0]+run.frame_range[1])*.5));b=foot.matrix.translation.copy();motion=(a-b).length
 assert motion>.03,(name,motion)
 rig.animation_data.action=idle
 if idle.slots:rig.animation_data.action_slot=idle.slots[0]
 bpy.context.scene.frame_set(10)
 for o in objects:
  if o.type=='MESH':
   assert o.data.uv_layers,(name,o.name)
   m=bpy.data.materials.new(name+' palette');m.use_nodes=True;bs=m.node_tree.nodes.get('Principled BSDF');bs.inputs['Roughness'].default_value=.8
   t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(assets/'skeleton_texture.png'),check_existing=True);t.interpolation='Closest';m.node_tree.links.new(t.outputs['Color'],bs.inputs['Base Color']);o.data.materials.clear();o.data.materials.append(m)
 offset=bpy.data.objects.new(name+' display',None);bpy.context.collection.objects.link(offset)
 for o in objects:
  if not o.parent:o.parent=offset
 offset.location=Vector(((index-1.5)*2.5,2.5,0))
 report.append({'name':name,'clips':len(actions),'bones':len(rig.data.bones),'foot_motion':round(motion,4),'result':'PASS'})
for index,name in enumerate(['barrel_large','crates_stacked','chest_gold','pillar_decorated','barrel_small_stack','torch_lit']):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(assets/(name+'.fbx')))
 for o in set(bpy.context.scene.objects)-before:
  if not o.parent:o.location+=Vector(((index-2.5)*1.7,-2.8,0))
  if o.type=='MESH':
   assert o.data.uv_layers,name
   m=bpy.data.materials.new(name+' palette');m.use_nodes=True;t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(assets/'dungeon_texture.png'),check_existing=True);t.interpolation='Closest';m.node_tree.links.new(t.outputs['Color'],m.node_tree.nodes.get('Principled BSDF').inputs['Base Color']);o.data.materials.clear();o.data.materials.append(m)
for pos,power in [((0,-5,9),1200),((-5,2,6),750)]:
 bpy.ops.object.light_add(type='AREA',location=pos);bpy.context.object.data.energy=power;bpy.context.object.data.size=7
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object;floor.location.z=-.02;m=bpy.data.materials.new('Floor');m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.045,.055,.08,1);floor.data.materials.append(m)
bpy.ops.object.camera_add(location=(0,-17,12));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,.8))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=14
scene=bpy.context.scene;scene.camera=cam;scene.world=bpy.data.worlds.new('World');scene.world.color=(.15,.15,.15);scene.view_settings.view_transform='Standard';scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.render.filepath=str(ROOT/'art/third-party/kaykit/integration-review.png');bpy.ops.render.render(write_still=True)
(ROOT/'WoollyArenaTest/Logs/kaykit-assets.json').write_text(json.dumps(report,indent=2));print(report)
