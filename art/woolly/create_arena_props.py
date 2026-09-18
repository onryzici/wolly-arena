import bpy,random,math
from mathutils import Vector
from pathlib import Path
out=Path('/Users/trexoinnovation/mobil-lamb-test/WoollyArenaTest/Assets/Woolly/Art/Environment')
random.seed(27)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
colors={'Sandstone':(.63,.27,.14,1),'StoneTop':(.85,.44,.24,1),'Wood':(.45,.22,.095,1),'WoodLight':(.65,.36,.15,1),'Iron':(.22,.29,.32,1),'GrassGold':(.95,.60,.13,1),'GrassLight':(1,.78,.26,1),'Cactus':(.27,.47,.15,1),'Flower':(.92,.24,.35,1)}
mats={}
for n,c in colors.items():
 m=bpy.data.materials.new(n);m.diffuse_color=c;mats[n]=m
parts=[]
def cube(pos,scale,mat,bevel=.05):
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 if bevel:
  mod=o.modifiers.new('Soft carved edges','BEVEL');mod.width=bevel;mod.segments=2;bpy.ops.object.modifier_apply(modifier=mod.name)
 o.data.materials.append(mats[mat]);parts.append(o);return o
def cylinder(pos,r,depth,mat,verts=12):
 bpy.ops.mesh.primitive_cylinder_add(vertices=verts,radius=r,depth=depth,location=pos);o=bpy.context.object;o.data.materials.append(mats[mat]);parts.append(o);return o
def save(name):
 for o in bpy.context.selected_objects:o.select_set(False)
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name=name;bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 bpy.ops.export_scene.fbx(filepath=str(out/(name+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',bake_anim=False,apply_scale_options='FBX_SCALE_UNITS')
 bpy.data.objects.remove(o,do_unlink=True);parts.clear()
# Staggered hand-cut stone courses with softened silhouettes.
for row in range(3):
 for x in range(3):
  o=cube(((x-1)*.80+random.uniform(-.025,.025),0,.22+row*.41),(.79+random.uniform(-.025,.025),1.46+random.uniform(-.025,.025),.42), 'StoneTop' if row==2 else 'Sandstone',.055);o.rotation_euler.z=random.uniform(-.018,.018)
save('CarvedStoneCover')
# Staved barrel, wider middle, inset top and metal hoops.
for j in range(12):
 a=2*math.pi*j/12;o=cube((.33*math.cos(a),.33*math.sin(a),.48),(.19,.11,.85),'WoodLight' if j%3==0 else 'Wood',.025);o.rotation_euler.z=a+math.pi/2
cylinder((0,0,.9),.36,.06,'WoodLight');cylinder((0,0,.2),.41,.085,'Iron');cylinder((0,0,.73),.41,.085,'Iron');save('WoodBarrel')
for x in range(4):cube(((x-1.5)*.19,0,.42),(.18,.78,.78),'Wood' if x%2 else 'WoodLight',.02)
for y in [-.42,.42]:
 for z in [.12,.72]:cube((0,y,z),(.91,.1,.14),'WoodLight',.02)
 o=cube((0,y,.42),(.95,.11,.12),'WoodLight',.018);o.rotation_euler.y=.65
save('SupplyCrate')
# Broad curved tapered leaves; solid three-sided cross section avoids invisible backs.
for j in range(9):
 a=j*2.4;h=random.uniform(.38,.68);base=Vector((random.uniform(-.12,.12),random.uniform(-.12,.12),0));side=Vector((math.cos(a),math.sin(a),0));outward=Vector((-math.sin(a),math.cos(a),0));vs=[]
 for z,w,drift in [(0,.055,0),(h*.45,.095,.025),(h*.8,.058,.12),(h,0,.19)]:
  c=base+outward*drift+Vector((0,0,z));vs += [c-side*w,c+side*w,c+outward*.035]
 fs=[]
 for k in range(3):
  for q in range(3):fs.append((k*3+q,k*3+(q+1)%3,(k+1)*3+(q+1)%3,(k+1)*3+q))
 mesh=bpy.data.meshes.new('Leaf');mesh.from_pydata(vs,[],fs);o=bpy.data.objects.new('Leaf',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mats['GrassLight' if j%3==0 else 'GrassGold']);parts.append(o)
save('GoldenGrass')
