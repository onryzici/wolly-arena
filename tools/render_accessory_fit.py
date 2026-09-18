"""Offline geometry previews of the shared accessory fit JSON; not Unity rendering."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];ART=ROOT/'art/woolly';OUT=ART/'accessory_review';OUT.mkdir(exist_ok=True)
profiles=json.loads((ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Characters/AccessoryFits.json').read_text())['profiles']
cases=[('Woolly',ART/'Woolly_Unity_Unarmed.blend','Woolly_Rig'),('Vera',ART/'punk_vera/PunkVera_WoollyRig.blend','PunkVera_Rig'),('Patchwork',ART/'patchwork/Patchwork_WoollyRig.blend','Patchwork_Rig')]
for index,(name,path,rig_name)in enumerate(cases):
 bpy.ops.wm.open_mainfile(filepath=str(path));s=bpy.context.scene;r=bpy.data.objects[rig_name];r.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1);fit=profiles[index]
 unit=(r.pose.bones['mixamorig:Head'].head-r.pose.bones['mixamorig:LeftFoot'].head).length/.914;print(name,'reference unit',unit)
 mats={}
 for label,color in [('dark',(.08,.09,.14,1)),('brown',(.52,.25,.10,1)),('gold',(1,.71,.18,1)),('glass',(.16,.65,.82,1))]:
  m=bpy.data.materials.new(label);m.diffuse_color=color;m.use_nodes=True;m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=color;mats[label]=m
 def vec(d):return Vector((d['x'],d['y'],d['z']))
 def unity(v):return Vector((v[0],-v[2],v[1]))*unit
 def part(kind,local,scale,mat,origin,fit_scale=(1,1,1),rotation=0):
  pos=vec(origin)+Vector(tuple(local[i]*fit_scale[i]for i in range(3)));dims=tuple(scale[i]*fit_scale[i]*unit for i in range(3))
  if kind=='cylinder':bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=.5,depth=2,location=unity(pos))
  else:bpy.ops.mesh.primitive_cube_add(size=1,location=unity(pos))
  o=bpy.context.object;o.scale=(dims[0],dims[2],dims[1]);o.data.materials.append(mats[mat]);o.rotation_euler.y=rotation
 hs=vec(fit['hatScale']);gs=vec(fit['glassesScale'])
 for pos,scale,mat in [((0,0,0),(.88,.018,.91),'dark'),((0,.02,0),(.85,.018,.88),'brown'),((0,.135,0),(.66,.12,.78),'brown'),((0,.058,0),(.667,.022,.787),'gold')]:part('cylinder',pos,scale,mat,fit['hat'],hs)
 for side in [-1,1]:
  for pos,scale,mat in [((side*.14,0,0),(.25,.15,.045),'dark'),((side*.14,0,.027),(.19,.095,.014),'glass'),((side*.255,0,-.15),(.025,.026,.32),'dark')]:part('cube',pos,scale,mat,fit['glasses'],gs)
 part('cube',(0,0,0),(.085,.026,.035),'gold',fit['glasses'],gs)
 c=bpy.data.curves.new('Draped necklace','CURVE');c.dimensions='3D';c.bevel_depth=.007*unit;c.bevel_resolution=2;spline=c.splines.new('POLY');spline.points.add(39)
 for n,p in enumerate(spline.points):
  a=n*math.pi/20;front=math.sin(a);pt=vec(fit['necklace'])+Vector((math.cos(a)*fit['neckWidth'],-.12*max(0,front),front*fit['neckDepth']));p.co=(*unity(pt),1)
 spline.use_cyclic_u=True;o=bpy.data.objects.new('Draped necklace',c);s.collection.objects.link(o);o.data.materials.append(mats['gold'])
 part('cube',(0,-.17,fit['neckDepth']+.018),(.10,.10,.028),'gold',fit['necklace'],rotation=math.pi/4)
 for o in list(s.objects):
  if o.type=='LIGHT':bpy.data.objects.remove(o,do_unlink=True)
 for pos,power in [((1,-4,5),650),((-3,-1,3),380)]:
  bpy.ops.object.light_add(type='AREA',location=pos);bpy.context.object.data.energy=power;bpy.context.object.data.size=4
 s.render.engine='CYCLES';s.cycles.samples=14;s.render.resolution_x=750;s.render.resolution_y=900;s.render.resolution_percentage=100;s.render.film_transparent=True;s.render.use_freestyle=False;s.camera.data.ortho_scale=2.05
 for label,pos in [('front',(0,-5,.9)),('quarter',(2,-5,1.6))]:
  s.camera.location=pos;s.camera.rotation_euler=(Vector((0,0,.9))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(OUT/(name+'-'+label+'.png'));bpy.ops.render.render(write_still=True)
