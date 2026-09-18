"""Compare every copied bone and animation sample to the real Woolly rig offline."""
import json
from pathlib import Path
import bpy
import numpy as np
root=Path(__file__).resolve().parents[1]
art=root/'art/woolly/punk_vera'
bpy.ops.wm.open_mainfile(filepath=str(art/'PunkVera_WoollyRig.blend'))
s=bpy.context.scene;r=bpy.data.objects['PunkVera_Rig'];body=bpy.data.objects['PunkVera_Body']
actions={n:bpy.data.actions[n] for n in ['Idle','Walk','Run']}
with bpy.data.libraries.load(str(art.parent/'Woolly_Unity_Unarmed.blend')) as (src,dst):
 dst.objects=['Woolly_Rig'];dst.actions=['Idle','Walk','Run']
ref=dst.objects[0];s.collection.objects.link(ref)
for track in ref.animation_data.nla_tracks:track.mute=True
refactions=dict(zip(['Idle','Walk','Run'],dst.actions))
assert set(r.data.bones.keys())==set(ref.data.bones.keys())
rest_error=max(abs(x-y) for b in r.data.bones for row,rr in zip(b.matrix_local,ref.data.bones[b.name].matrix_local)for x,y in zip(row,rr))
assert rest_error<1e-7
for b in r.data.bones:
 assert (b.parent.name if b.parent else None)==(ref.data.bones[b.name].parent.name if ref.data.bones[b.name].parent else None)
soles={}
for side in ['Left','Right']:
 group=body.vertex_groups['mixamorig:'+side+'Foot'].index
 soles[side]=[v.index for v in body.data.vertices if v.co.z<.12 and any(g.group==group and g.weight>.9999 for g in v.groups)]
 assert len(soles[side])>100
report={'reference':'Woolly_Unity_Unarmed.blend','bone_count':len(r.data.bones),'rest_matrix_error':rest_error,'clips':{}}
for name,action in actions.items():
 r.animation_data.action=action;ref.animation_data.action=refactions[name]
 assert tuple(action.frame_range)==tuple(refactions[name].frame_range)
 a,b=map(int,action.frame_range);floor=999;reference_floor=999;floor_regression=0;drift=error=stretch=contact=0;initial={};first=None
 for step in range((b-a)*2+1):
  frame=a+step/2;s.frame_set(int(frame),subframe=frame%1)
  error=max(error,max(abs(x-y)for bone in r.pose.bones for row,rr in zip(bone.matrix,ref.pose.bones[bone.name].matrix)for x,y in zip(row,rr)))
  evaluated=body.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=evaluated.to_mesh();levels=[]
  points=np.array([v.co[:]for v in mesh.vertices]);evaluated.to_mesh_clear()
  if first is None:first=points.copy()
  last=points
  for side,indices in soles.items():
   levels.append(float(points[indices,2].min()))
   initial.setdefault(side,points[indices].copy())
   drift=max(drift,float(np.linalg.norm(points[indices]-initial[side],axis=1).max()))
   # Rigid sole pair distances must remain constant throughout locomotion.
   delta=points[indices]-points[indices[0]];base=initial[side]-initial[side][0]
   stretch=max(stretch,float(np.abs(np.linalg.norm(delta,axis=1)-np.linalg.norm(base,axis=1)).max()))
  floor=min(floor,*levels);contact=max(contact,abs(levels[0]-levels[1]))
  reference_body=bpy.data.objects['Woolly_Body'].evaluated_get(bpy.context.evaluated_depsgraph_get())
  reference_mesh=reference_body.to_mesh()
  used={i for face in reference_mesh.polygons for i in face.vertices}
  source_floor=min(reference_mesh.vertices[i].co.z for i in used)
  reference_body.to_mesh_clear()
  reference_floor=min(reference_floor,source_floor)
  floor_regression=max(floor_regression,source_floor-min(levels))
 seam=float(np.linalg.norm(first-last,axis=1).max())
 assert error<1e-6,(name,'not the same skeleton pose',error)
 assert stretch<.0001,(name,'boot distortion',stretch)
 assert seam<.0001,(name,'loop seam',seam)
 # Locomotion uses Woolly's exact keys; require no worse contact than that source.
 assert floor_regression<.015,(name,'contact worse than Woolly',floor_regression)
 if name=='Idle':
  assert floor>-.001
  assert drift<.0001,('idle sole movement',drift)
  assert contact<.001,('uneven feet',contact)
 report['clips'][name]={'samples':(b-a)*2+1,'pose_matrix_error':error,'minimum_sole_height':floor,'woolly_minimum_sole_height':reference_floor,'maximum_contact_regression':floor_regression,'sole_shape_error':stretch,'loop_seam':seam}
 if name=='Idle':report['clips'][name].update(sole_drift=drift,sole_height_difference=contact)
(art/'woolly-rig-validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
