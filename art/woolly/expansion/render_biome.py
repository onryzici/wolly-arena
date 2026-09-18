import bpy,math
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[3];out=root/'WoollyArenaTest/Assets/Woolly/Resources/Biomes'
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def model(name,x,y,scale=1,z=0):
 before=set(bpy.context.scene.objects);bpy.ops.import_scene.fbx(filepath=str(root/'WoollyArenaTest/Assets/Woolly/Resources/Models'/(name+'.fbx')))
 for obj in set(bpy.context.scene.objects)-before:obj.location+=Vector((x,y,z));obj.scale*=scale
for x,y in [(-4,4.6),(4,4.6),(-5,-.4),(5,-.4),(-3,-5),(3,-5)]:model('IceWall',x,y);model('IceCluster',x,y,.8,.85)
for side in (-1,1):
 for n in range(9):
  model('FrostFir',side*(10.8+(n%2)*.25),-9+n*2.3,1+(n%3)*.16)
  if n<8:model('FrostFir',-9+n*2.6,side*10.4,.9+(n%3)*.12)
for n in range(8):model('IceCluster',(-1 if n%2==0 else 1)*9.5,-8+(n//2)*5,.55)
for m in bpy.data.materials:
 if m.name.startswith('C_'):
  h=m.name[2:8];m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=tuple(int(h[i:i+2],16)/255 for i in (0,2,4))+(1,)
bpy.ops.mesh.primitive_plane_add(size=2);plane=bpy.context.object;plane.scale=(18,12,1)
m=bpy.data.materials.new('Painted snow');m.use_nodes=True;tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(str(out/'FrostGround.png'));m.node_tree.links.new(tex.outputs['Color'],m.node_tree.nodes.get('Principled BSDF').inputs['Base Color']);plane.data.materials.append(m)
bpy.context.scene.world.color=(.5,.5,.5)
bpy.ops.object.light_add(type='AREA',location=(-3,-6,18));bpy.context.object.data.energy=5500;bpy.context.object.data.size=16
bpy.ops.object.camera_add(location=(0,-22,27));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=27
s=bpy.context.scene;s.camera=cam;s.render.engine='CYCLES';s.cycles.samples=24;s.render.resolution_x=1200;s.render.resolution_y=750;s.render.resolution_percentage=100;s.render.filepath=str(out/'FrostPreview.png');bpy.ops.render.render(write_still=True)
