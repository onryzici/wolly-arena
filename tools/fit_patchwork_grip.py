"""Replace only the open mitten beyond the cuff with an authored closed knife grip."""
import bpy,bmesh,math
from mathutils import Vector,Matrix

def grip_frame(rig):
 hand=rig.pose.bones['mixamorig:RightHand'];roll=Matrix.Rotation(math.radians(-45),3,'X')
 center=hand.matrix@(roll@Vector((-.025,.12,.015)))
 direction=(hand.matrix.to_3x3()@roll@Vector((0,0,1))).normalized()
 across=Vector((0,-1,0)).cross(direction).normalized()
 return center,direction,across

def fit_grip(rig,body):
 bone=rig.data.bones['mixamorig:RightHand'];inv=bone.matrix_local.inverted();roll=Matrix.Rotation(math.radians(-45),3,'X');remove=[]
 for v in body.data.vertices:
  weight=sum(g.weight for g in v.groups if 'RightHand' in body.vertex_groups[g.group].name)
  if weight>.45 and (inv@v.co).y>.052:remove.append(v.index)
 bm=bmesh.new();bm.from_mesh(body.data);bm.verts.ensure_lookup_table();bmesh.ops.delete(bm,geom=[bm.verts[i]for i in remove],context='VERTS');bm.to_mesh(body.data);bm.free()
 parts=[]
 cream=(.61,.55,.47,1);shade=(.25,.16,.12,1);stitch=(.12,.075,.06,1)
 def finish(o,color):
  bpy.context.view_layer.update()
  world=o.matrix_world.copy()
  for v in o.data.vertices:v.co=bone.matrix_local@(roll@(world@v.co))
  o.matrix_world=Matrix.Identity(4)
  layer=o.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
  for c in layer.data:c.color=color
  for p in o.data.polygons:p.use_smooth=True
  parts.append(o)
 def ellipsoid(at,size,color):
  bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=at);o=bpy.context.object;o.scale=size;finish(o,color)
 def tube(points,radius,color):
  verts=[];faces=[]
  for i,p in enumerate(points):
   tangent=(points[min(i+1,len(points)-1)]-points[max(0,i-1)]).normalized();side=tangent.cross(Vector((0,0,1))).normalized();up=side.cross(tangent).normalized()
   for k in range(8):verts.append(p+radius*(side*math.cos(k*math.pi/4)+up*math.sin(k*math.pi/4)))
  for i in range(len(points)-1):
   for k in range(8):a=i*8+k;b=i*8+(k+1)%8;faces.append((a,b,b+8,a+8))
  faces.append(tuple(reversed(range(8))));faces.append(tuple((len(points)-1)*8+k for k in range(8)))
  mesh=bpy.data.meshes.new('Grip piece');mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new('Grip piece',mesh);bpy.context.scene.collection.objects.link(o);finish(o,color)
 # Back of the hand overlaps the retained wrist beneath the original sleeve.
 ellipsoid((.009,.07,0),(.054,.061,.081),cream)
 # Four bent fingers surround the handle, with separate knuckles and curled tips.
 for z in [-.051,-.015,.021,.057]:
  points=[Vector((-.037+.044*math.cos(a),.13+.044*math.sin(a),z))for a in [math.radians(-18+i*215/14)for i in range(15)]]
  tube(points,.017,cream);ellipsoid(points[-1],(.018,.018,.017),cream)
 # Thumb closes over the upper fingers, crossing the grip rather than the blade.
 tube([Vector((-.014,.069,.072)),Vector((-.043,.09,.073)),Vector((-.071,.112,.054)),Vector((-.078,.128,.029))],.023,cream)
 ellipsoid((-.078,.128,.029),(.023,.023,.024),cream)
 # A worn bandage stripe and short stitches echo the supplied patchwork clothing.
 for z in [-.041,-.019,.003,.025,.047]:
  tube([Vector((.054,.059,z)),Vector((.058,.079,z+.006))],.0022,stitch)
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:o.select_set(True)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();o=bpy.context.object;o.name='Patchwork_Grip';o.parent=rig;o.matrix_parent_inverse=Matrix.Identity(4)
 group=o.vertex_groups.new(name=bone.name);group.add(list(range(len(o.data.vertices))),1,'REPLACE');mod=o.modifiers.new('Closed right hand','ARMATURE');mod.object=rig
 material=bpy.data.materials.new('PatchworkGripVertexPalette');material.use_nodes=True;n=material.node_tree.nodes.new('ShaderNodeVertexColor');n.layer_name='Color';bsdf=material.node_tree.nodes.get('Principled BSDF');material.node_tree.links.new(n.outputs['Color'],bsdf.inputs['Base Color']);bsdf.inputs['Roughness'].default_value=.9;o.data.materials.clear();o.data.materials.append(material)
 return o
