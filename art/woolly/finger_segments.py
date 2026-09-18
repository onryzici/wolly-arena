import bpy,json,numpy as np
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Revolver_v8.blend'))
o=bpy.data.objects['Woolly_Body'];inv=bpy.data.objects['Woolly_Rig'].data.bones['mixamorig:RightHand'].matrix_local.inverted();vs=json.loads((p/'hand_vertices.json').read_text());coords={v['i']:np.array(v['local']) for v in vs};adj={i:set() for i in coords}
for e in o.data.edges:
 a,b=e.vertices
 if a in coords and b in coords:adj[a].add(b);adj[b].add(a)
for threshold in [.12,.15,.18,.20]:
 todo={i for i,c in coords.items() if c[1]>threshold};comps=[]
 while todo:
  stack=[todo.pop()];part=[]
  while stack:
   a=stack.pop();part.append(a)
   for b in adj[a]&todo:todo.remove(b);stack.append(b)
  a=np.array([coords[i] for i in part]);comps.append((len(part),a.mean(0).tolist(),a.min(0).tolist(),a.max(0).tolist()))
 print('CUT',threshold,sorted(comps,reverse=True))
