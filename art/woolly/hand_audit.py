import bpy,json
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Revolver_v8.blend'))
o=bpy.data.objects['Woolly_Body'];r=bpy.data.objects['Woolly_Rig'];b=r.data.bones['mixamorig:RightHand'];inv=b.matrix_local.inverted()
vs=[]
for v in o.data.vertices:
 gs={o.vertex_groups[g.group].name:g.weight for g in v.groups}
 if gs.get('mixamorig:RightHand',0)+gs.get('mixamorig:RightHandMiddle4',0)>.35:
  vs.append({'i':v.index,'co':list(v.co),'local':list(inv@v.co),'weights':gs})
(p/'hand_vertices.json').write_text(json.dumps(vs))
print('count',len(vs));print('local bounds',[[min(v['local'][i] for v in vs),max(v['local'][i] for v in vs)] for i in range(3)]);print('matrix',b.matrix_local);print('socket local',bpy.data.objects['Revolver_HandSocket'].matrix_basis)
