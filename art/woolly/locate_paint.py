import bpy,numpy as np,json
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_NaturalContact_v5.blend'))
o=bpy.data.objects['Woolly_Body'];m=o.data;im=bpy.data.images['Woolly_BaseColor'];w,h=im.size;pixels=np.array(im.pixels[:]).reshape(h,w,4)
uv=m.uv_layers.active.data;selected={};edge_faces={}
for face in m.polygons:
 c=face.center
 if not(.20<abs(c.x)<.65 and .65<c.z<1.05):continue
 t=np.mean([uv[i].uv[:] for i in face.loop_indices],axis=0);rgb=pixels[min(h-1,int(t[1]*h)),min(w-1,int(t[0]*w)),:3]
 if min(rgb)>.15 and (max(rgb)-min(rgb))/max(rgb)<.42:selected[face.index]=list(c)
for f in m.polygons:
 for key in f.edge_keys:edge_faces.setdefault(tuple(sorted(key)),[]).append(f.index)
adj={i:set() for i in selected}
for fs in edge_faces.values():
 for a in fs:
  if a in selected:adj[a].update(b for b in fs if b in selected and b!=a)
comps=[];todo=set(selected)
while todo:
 stack=[todo.pop()];comp=[]
 while stack:
  a=stack.pop();comp.append(a)
  for b in adj[a]&todo:todo.remove(b);stack.append(b)
 coords=np.array([selected[i] for i in comp]);comps.append({'faces':comp,'center':coords.mean(0).tolist(),'lo':coords.min(0).tolist(),'hi':coords.max(0).tolist()})
comps.sort(key=lambda c:-len(c['faces']));(p/'paint_regions.json').write_text(json.dumps(comps,indent=2))
for i,c in enumerate(comps):print(i,len(c['faces']),c['center'],c['lo'],c['hi'])
