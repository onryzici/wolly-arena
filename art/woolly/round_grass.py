import bpy,math,random
from mathutils import Vector
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False);random.seed(9)
vs=[];fs=[];slots=[]
for leaf in range(7):
 angle=leaf*2.4;h=random.uniform(.43,.69);base=Vector((math.cos(angle)*.085,math.sin(angle)*.085,0));out=Vector((math.cos(angle),math.sin(angle),0));side=Vector((-math.sin(angle),math.cos(angle),0));start=len(vs)
 for u,w in [(0,.026),(.27,.081),(.6,.088),(.85,.057),(.98,.015),(1,.001)]:
  center=base+Vector((0,0,h*u))+out*(.14*u*u)
  for j in range(8):
   a=j*math.pi/4;v=center+side*(math.cos(a)*w)+out*(math.sin(a)*w*.38);vs.append(v)
 for k in range(5):
  for j in range(8):fs.append((start+k*8+j,start+k*8+(j+1)%8,start+(k+1)*8+(j+1)%8,start+(k+1)*8+j));slots.append(leaf%3==0)
m=bpy.data.meshes.new('GoldenGrass');m.from_pydata(vs,[],fs);m.update();o=bpy.data.objects.new('GoldenGrass',m);bpy.context.collection.objects.link(o)
for name in ['GrassGold','GrassLight']:
 mat=bpy.data.materials.new(name);m.materials.append(mat)
for poly,slot in zip(m.polygons,slots):poly.material_index=int(slot);poly.use_smooth=True
bpy.context.view_layer.objects.active=o;o.select_set(True)
bpy.ops.export_scene.fbx(filepath='/Users/trexoinnovation/mobil-lamb-test/WoollyArenaTest/Assets/Woolly/Art/Environment/GoldenGrass.fbx',use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,apply_scale_options='FBX_SCALE_UNITS')
