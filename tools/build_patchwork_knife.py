"""Authored cartoon knife, rigidly skinned to Patchwork's right hand in the shared idle pose."""
import bpy,runpy
from pathlib import Path
from mathutils import Vector,Matrix

def build_knife(rig):
 hand=rig.pose.bones['mixamorig:RightHand']
 center,forward,across=runpy.run_path(str(Path(__file__).with_name('fit_patchwork_grip.py')))['grip_frame'](rig)
 normal=forward.cross(across).normalized()
 vertices=[];faces=[];colors=[]
 steel=(.48,.57,.63,1);edge=(.81,.89,.91,1);dark=(.055,.035,.031,1);blood=(.34,.012,.02,1);wrap=(.15,.075,.04,1)
 def point(x,y,z):return center+across*x+normal*y+forward*z
 def polygon(coords,color,reverse=False):
  if reverse:coords=list(reversed(coords))
  first=len(vertices);vertices.extend(point(*p)for p in coords);faces.append(tuple(range(first,len(vertices))));colors.append(color)
 def prism(shape,thickness,color):
  polygon([(x,-thickness,z)for x,z in shape],color)
  polygon([(x,thickness,z)for x,z in reversed(shape)],color)
  for i,(x,z)in enumerate(shape):
   a,b=shape[(i+1)%len(shape)];polygon([(x,-thickness,z),(a,-thickness,b),(a,thickness,b),(x,thickness,z)],color,True)
 # Short leather grip with visible wraps, guard and an exaggerated clipped blade.
 prism([(-.027,-.055),(.027,-.055),(.03,.075),(-.03,.075)],.025,dark)
 for z in [-.038,-.008,.022,.052]:prism([(-.03,z-.009),(.03,z-.009),(.03,z+.009),(-.03,z+.009)],.027,wrap)
 prism([(-.066,.07),(.066,.07),(.066,.094),(-.066,.094)],.034,steel)
 blade=[(-.047,.097),(.044,.097),(.044,.345),(.005,.445),(-.046,.355)]
 prism(blade,.013,steel)
 for sign in [-1,1]:
  y=sign*.0134
  polygon([(-.047,y,.10),(-.027,y,.115),(-.026,y,.345),(.005,y,.445),(-.046,y,.355)],edge,sign>0)
  # Flat painted dried-blood marks on both faces, with irregular edges.
  polygon([(-.020,y+sign*.0005,.245),(.008,y+sign*.0005,.255),(.031,y+sign*.0005,.231),(.043,y+sign*.0005,.28),(.043,y+sign*.0005,.345),(.005,y+sign*.0005,.434),(-.026,y+sign*.0005,.356),(-.012,y+sign*.0005,.318),(-.032,y+sign*.0005,.291)],blood,sign>0)
  polygon([(-.02,y+sign*.0005,.191),(-.004,y+sign*.0005,.180),(.009,y+sign*.0005,.196),(.0,y+sign*.0005,.216),(-.017,y+sign*.0005,.211)],blood,sign>0)
 mesh=bpy.data.meshes.new('Patchwork_Knife');mesh.from_pydata(vertices,[],faces);mesh.update()
 obj=bpy.data.objects.new('Patchwork_Knife',mesh);bpy.context.scene.collection.objects.link(obj)
 palette=mesh.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
 for poly,color in zip(mesh.polygons,colors):
  for loop in poly.loop_indices:palette.data[loop].color=color
 mat=bpy.data.materials.new('PatchworkKnifeVertexPalette');mat.use_nodes=True
 color_node=mat.node_tree.nodes.new('ShaderNodeVertexColor');color_node.layer_name='Color';bsdf=mat.node_tree.nodes.get('Principled BSDF');mat.node_tree.links.new(color_node.outputs['Color'],bsdf.inputs['Base Color']);bsdf.inputs['Roughness'].default_value=.7
 mesh.materials.append(mat)
 skin=hand.matrix@hand.bone.matrix_local.inverted()
 for v in mesh.vertices:v.co=skin.inverted()@v.co
 obj.parent=rig;obj.matrix_parent_inverse=Matrix.Identity(4);group=obj.vertex_groups.new(name=hand.name);group.add(list(range(len(mesh.vertices))),1,'REPLACE');modifier=obj.modifiers.new('Right hand','ARMATURE');modifier.object=rig
 return obj
