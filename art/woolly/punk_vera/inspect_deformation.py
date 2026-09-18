import bpy
from pathlib import Path
p=Path(__file__).resolve().parent;bpy.ops.wm.open_mainfile(filepath=str(p/'PunkVera_Corrected.blend'));r=bpy.data.objects['PunkVera_Rig'];o=bpy.data.objects['PunkVera_Body']
print('MODIFIERS',[(m.name,m.type,getattr(m,'use_deform_preserve_volume',None))for m in o.modifiers]);print('SHAPES',o.data.shape_keys)
for side,sign in [('Left',1),('Right',-1)]:
 ids=[v.index for v in o.data.vertices if v.co.x*sign>0 and v.co.z<.07];a=ids[0];b=max(ids,key=lambda i:(o.data.vertices[i].co-o.data.vertices[a].co).length)
 print(side,'POINTS',[(i,list(o.data.vertices[i].co),[(o.vertex_groups[g.group].name,g.weight)for g in o.data.vertices[i].groups])for i in [a,b]])
 print('BONE',list(r.pose.bones['mixamorig:'+side+'Foot'].matrix.to_scale()))
 print('OTHERGROUPS',[(o.vertex_groups[g.group].name,g.weight)for i in ids for g in o.data.vertices[i].groups if not o.vertex_groups[g.group].name.endswith(side+'Foot')][:8])
