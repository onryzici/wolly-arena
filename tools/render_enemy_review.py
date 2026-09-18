import bpy, math
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
names=['MossMaw','SpineRaptor','HornBrute','StitchReaper','FrostWarden','ThornMatriarch']
for i,name in enumerate(names):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(root/'WoollyArenaTest/Assets/Woolly/Resources/Models'/(name+'.fbx')))
 for obj in set(bpy.context.scene.objects)-before:
  obj.location+=Vector(((i%6-2.5)*2.2, 0 if i<6 else -3,0))
  if i>=6:obj.scale*=1.7
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object;floor.location.z=-.08;m=bpy.data.materials.new('Backdrop');m.diffuse_color=(.09,.13,.19,1);floor.data.materials.append(m)
world=bpy.context.scene.world;world.color=(.5,.5,.5)
for p,power,size in [((1,-4,10),1000,8),((-7,1,6),700,7)]:
 bpy.ops.object.light_add(type='AREA',location=p);bpy.context.object.data.energy=power;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=size
for m in bpy.data.materials:
 if m.name.startswith('EnemyVertexPalette'):
  m.use_nodes=True;v=m.node_tree.nodes.new('ShaderNodeVertexColor');v.layer_name='Color';m.node_tree.links.new(v.outputs['Color'],m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
 elif m.name.startswith('C_'):
  hex=m.name[2:8];m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=tuple(int(hex[i:i+2],16)/255 for i in (0,2,4))+(1,)
 else:
  m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=m.diffuse_color
bpy.ops.object.camera_add(location=(0,-16,8));cam=bpy.context.object;cam.rotation_euler=(Vector((0,-.5,.5))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=15.5
scene=bpy.context.scene;scene.camera=cam;scene.view_settings.view_transform='Standard';scene.view_settings.look='None';scene.world.color=(.22,.22,.22);scene.render.engine='CYCLES';scene.cycles.samples=24;scene.render.resolution_x=1800;scene.render.resolution_y=600;scene.render.resolution_percentage=100;scene.render.filepath=str(root/'art/woolly/expansion/enemy-articulated-review.png');bpy.ops.wm.save_as_mainfile(filepath=str(root/'art/woolly/expansion/EnemyArticulatedReview.blend'));bpy.ops.render.render(write_still=True)
