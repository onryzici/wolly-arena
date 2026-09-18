"""Offline Blender audit: file units, animated mesh extents, and hand socket scales.
Run with Blender -b --python tools/check_kaykit_scale.py. Never launches Unity.
"""
import bpy, json, math
from pathlib import Path
from mathutils import Vector
from io_scene_fbx import parse_fbx
ROOT=Path(__file__).resolve().parents[1]
assets=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/KayKit'
report=[]
for path in sorted(assets.glob('Skeleton_*.fbx')):
 if path.stem in {'Skeleton_Blade','Skeleton_Axe','Skeleton_Staff'}: continue
 raw,_=parse_fbx.parse(str(path))
 objects=next(e for e in raw.elems if e.id==b'Objects')
 for obj in objects.elems:
  if obj.id!=b'Model':continue
  for props in obj.elems:
   if props.id!=b'Properties70':continue
   for prop in props.elems:
    if prop.props[0]==b'Lcl Scaling':
     assert max(abs(x) for x in prop.props[-3:])<2,(path.name,'FBX node scale',prop.props)
 bpy.ops.wm.read_factory_settings(use_empty=True)
 bpy.ops.import_scene.fbx(filepath=str(path))
 rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
 for track in list(rig.animation_data.nla_tracks):rig.animation_data.nla_tracks.remove(track)
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 biggest=0;samples=0
 for action in bpy.data.actions:
  rig.animation_data.action=action
  if action.slots:rig.animation_data.action_slot=action.slots[0]
  for step in range(17):
   frame=action.frame_range[0]+(action.frame_range[1]-action.frame_range[0])*step/16
   bpy.context.scene.frame_set(int(frame),subframe=frame-int(frame))
   deps=bpy.context.evaluated_depsgraph_get();points=[]
   for obj in meshes:
    evaluated=obj.evaluated_get(deps);mesh=evaluated.to_mesh()
    points.extend(evaluated.matrix_world@v.co for v in mesh.vertices);evaluated.to_mesh_clear()
   span=max(max(p[i] for p in points)-min(p[i] for p in points) for i in range(3))
   assert .2<span<4,(path.name,action.name,frame,span)
   assert all(math.isfinite(v) for p in points for v in p)
   socket=rig.matrix_world@rig.pose.bones['handslot.r'].matrix
   assert all(.95<s<1.05 for s in socket.to_scale()),(path.name,'socket',socket.to_scale())
   biggest=max(biggest,span);samples+=1
 report.append(dict(model=path.stem,samples=samples,max_span_m=round(biggest,4),node_and_socket_scale='PASS'))
out=ROOT/'WoollyArenaTest/Logs/kaykit-scale-offline.json';out.parent.mkdir(exist_ok=True);out.write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
