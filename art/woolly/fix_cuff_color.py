import bpy,json
from pathlib import Path
from mathutils import Vector
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_NaturalContact_v5.blend'))
s=bpy.context.scene;o=bpy.data.objects['Woolly_Body'];m=o.data;r=bpy.data.objects['Woolly_Rig'];regions=json.loads((p/'paint_regions.json').read_text())
from bpy_extras.object_utils import world_to_camera_view
r.data.pose_position='REST';bpy.context.view_layer.update()
selected=set();cam=s.camera;cam.data.ortho_scale=.32
s.render.resolution_x=900;s.render.resolution_y=900
for sign,spots in [(1,[(670,605,88),(563,449,48)]),(-1,[(233,584,94),(341,436,45)])]:
 center=Vector((sign*.43,.05,.71));cam.location=center+Vector((sign*2,2,-.7));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();bpy.context.view_layer.update()
 for face in m.polygons:
  c=face.center
  if c.x*sign<.3 or c.z>.80 or c.z<.60:continue
  pr=world_to_camera_view(s,cam,o.matrix_world@c);px=pr.x*900;py=(1-pr.y)*900
  if any((px-x)**2+(py-y)**2<rad**2 for x,y,rad in spots):
   origin=cam.matrix_world@Vector(((px/900-.5)*.32,(.5-py/900)*.32,0));direction=cam.matrix_world.to_3x3()@Vector((0,0,-1));hit,loc,n,idx,obj,mat=s.ray_cast(bpy.context.evaluated_depsgraph_get(),origin,direction)
   if hit and (loc-o.matrix_world@c).length<.003:selected.add(face.index)
core=set(selected)
edge_faces={}
for f in m.polygons:
 for e in f.edge_keys:edge_faces.setdefault(tuple(sorted(e)),set()).add(f.index)
for _ in range(2):
 neighbors=set()
 for fidx in selected:
  for e in m.polygons[fidx].edge_keys:neighbors.update(edge_faces[tuple(sorted(e))])
 selected.update(neighbors)
r.data.pose_position='POSE';bpy.context.view_layer.update()
mask=m.attributes.new('Cuff_Paint_Repair_Mask','FLOAT','CORNER')
for f in m.polygons:
 for li in f.loop_indices:mask.data[li].value=1 if f.index in selected else 0
mat=o.active_material.copy();mat.name='Woolly_PBR_CuffPaintRepair';o.data.materials[0]=mat;nt=mat.node_tree;nodes=nt.nodes;links=nt.links
tex=next(n for n in nodes if n.type=='TEX_IMAGE' and n.image.name=='Woolly_BaseColor');bs=next(n for n in nodes if n.type=='BSDF_PRINCIPLED')
frame=nodes.new('NodeFrame');frame.label='Local cuff paint repair — white spill only';frame.name='Cuff paint repair'
def node(typ,name):
 n=nodes.new(typ);n.name=name;n.label=name;n.parent=frame;return n
attr=node('ShaderNodeAttribute','Cuff surface mask');attr.attribute_name=mask.name
sep=node('ShaderNodeSeparateColor','Source HSV');sep.mode='HSV';links.new(tex.outputs['Color'],sep.inputs[0])
# Replace pale contamination; retain dark cuff material and saturated yellow fabric.
sat=node('ShaderNodeMapRange','Pale stain selection');sat.clamp=True;sat.inputs['From Min'].default_value=.88;sat.inputs['From Max'].default_value=.99;sat.inputs['To Min'].default_value=1;sat.inputs['To Max'].default_value=0;links.new(sep.outputs[1],sat.inputs['Value'])
bright=node('ShaderNodeMapRange','Protect dark wrist cuff');bright.clamp=True;bright.inputs['From Min'].default_value=.12;bright.inputs['From Max'].default_value=.4;links.new(sep.outputs[2],bright.inputs['Value'])
fac=node('ShaderNodeMath','Stain times surface');fac.operation='MULTIPLY';links.new(attr.outputs['Fac'],fac.inputs[0]);links.new(sat.outputs['Result'],fac.inputs[1])
protect=node('ShaderNodeMapRange','Protect natural white wool');protect.clamp=True;protect.inputs['From Min'].default_value=.40;protect.inputs['From Max'].default_value=.60;links.new(sep.outputs[1],protect.inputs['Value'])
guard=node('ShaderNodeMath','Exclude white hand');guard.operation='MULTIPLY';links.new(fac.outputs[0],guard.inputs[0]);links.new(protect.outputs[0],guard.inputs[1])
fac2=node('ShaderNodeMath','Protect dark details');fac2.operation='MULTIPLY';links.new(fac.outputs[0],fac2.inputs[0]);links.new(bright.outputs[0],fac2.inputs[1])
# Match jacket yellow in linear RGB while retaining source value variations.
combine=node('ShaderNodeCombineColor','Matched jacket yellow');combine.mode='HSV';combine.inputs[0].default_value=.105;combine.inputs[1].default_value=.98;links.new(sep.outputs[2],combine.inputs[2])
mix=node('ShaderNodeMixRGB','Cuff localized color repair');mix.blend_type='MIX';links.new(fac2.outputs[0],mix.inputs[0]);links.new(tex.outputs['Color'],mix.inputs[1]);links.new(combine.outputs[0],mix.inputs[2]);links.new(mix.outputs[0],bs.inputs['Base Color'])
for i,n in enumerate([attr,sep,sat,bright,fac,fac2,combine,mix]):n.location=((i%4)*220,-(i//4)*250)
frame.location=(-1100,-550)
bpy.data.texts['READ_ME'].write('\nV6: local reversible cuff discoloration correction. Cuff_Paint_Repair_Mask isolates white/gray paint spills on yellow sleeves; material nodes recolor pale pixels while preserving dark cuffs and hands. Original packed BaseColor remains intact. V5 natural run retained.\n')
s.frame_set(7);bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_PaintFix_v6.blend'))
r.data.pose_position='REST';s.render.resolution_x=900;s.render.resolution_y=900;s.cycles.samples=24;cam=s.camera;cam.data.ortho_scale=.32
for side,sign in [('left',1),('right',-1)]:
 center=Vector((sign*.43,.05,.71));cam.location=center+Vector((sign*2,2,-.7));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('cuff_'+side+'_after.png'));bpy.ops.render.render(write_still=True)
print('Masked faces',len(selected),'core',len(core))
