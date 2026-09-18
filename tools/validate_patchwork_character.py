"""Offline Blender rig, deformation, FBX and artwork validation; never launches Unity."""
from pathlib import Path
import json,math
import bpy,numpy as np
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];ART=ROOT/'art/woolly/patchwork';OUT=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Characters'
bpy.ops.wm.open_mainfile(filepath=str(ART/'Patchwork_WoollyRig.blend'))
s=bpy.context.scene;r=bpy.data.objects['Patchwork_Rig'];body=bpy.data.objects['Patchwork_Body'];actions={n:bpy.data.actions[n]for n in ['Idle','Walk','Run']}
knife=bpy.data.objects['Patchwork_Knife']
grip=bpy.data.objects['Patchwork_Grip']
assert max(grip.dimensions)<.5, tuple(grip.dimensions)
assert grip.data.color_attributes.get('Color')
for v in grip.data.vertices:
 assert len(v.groups)==1 and grip.vertex_groups[v.groups[0].group].name=='mixamorig:RightHand' and abs(v.groups[0].weight-1)<1e-6
assert knife.data.color_attributes.get('Color') and len(knife.data.vertices)>100
for vertex in knife.data.vertices:
 assert len(vertex.groups)==1 and knife.vertex_groups[vertex.groups[0].group].name=='mixamorig:RightHand' and abs(vertex.groups[0].weight-1)<1e-6
with bpy.data.libraries.load(str(ART.parent/'Woolly_Unity_Unarmed.blend'))as(src,dst):dst.objects=['Woolly_Rig'];dst.actions=['Idle','Walk','Run']
reference=dst.objects[0];s.collection.objects.link(reference);refs=dict(zip(['Idle','Walk','Run'],dst.actions))
for t in reference.animation_data.nla_tracks:t.mute=True
assert set(r.data.bones.keys())==set(reference.data.bones.keys())
error=max(abs(a-b)for bone in r.data.bones for row,rr in zip(bone.matrix_local,reference.data.bones[bone.name].matrix_local)for a,b in zip(row,rr));assert error<1e-7
for bone in r.data.bones:
 ref=reference.data.bones[bone.name];assert (bone.parent.name if bone.parent else None)==(ref.parent.name if ref.parent else None)
for v in body.data.vertices:
 assert abs(sum(g.weight for g in v.groups)-1)<1e-4
 assert all(body.vertex_groups[g.group].name in r.data.bones for g in v.groups)
sole={side:[v.index for v in body.data.vertices if v.co.z<.1 and any(body.vertex_groups[g.group].name=='mixamorig:'+side+'Foot' and g.weight>.999 for g in v.groups)]for side in ['Left','Right']}
assert all(len(x)>100 for x in sole.values())
report={'bones':len(r.data.bones),'vertices':len(body.data.vertices),'rest_matrix_error':error,'normalized_weights':True,'clips':{},'not_run':['Unity importer/shader/runtime','phone build and device checks']}
for name,action in actions.items():
 r.animation_data.action=action;reference.animation_data.action=refs[name];a,b=map(int,action.frame_range);assert tuple(action.frame_range)==tuple(refs[name].frame_range)
 pose_error=0
 for frame in range(a,b+1):
  s.frame_set(frame);pose_error=max(pose_error,max(abs(x-y)for bone in r.pose.bones for row,rr in zip(bone.matrix,reference.pose.bones[bone.name].matrix)for x,y in zip(row,rr)))
 assert pose_error<1e-6
 first=None;floor=999;drift=0;stretch=0
 for frame in np.linspace(a,b,9):
  s.frame_set(int(frame),subframe=float(frame)%1);evaluated=body.evaluated_get(bpy.context.evaluated_depsgraph_get());mesh=evaluated.to_mesh();pts=np.array([v.co[:]for v in mesh.vertices]);evaluated.to_mesh_clear();assert np.isfinite(pts).all()
  if first is None:first=pts.copy()
  floor=min(floor,float(pts[:,2].min()));last=pts
  evaluated_knife=knife.evaluated_get(bpy.context.evaluated_depsgraph_get());knife_mesh=evaluated_knife.to_mesh()
  skin=r.pose.bones['mixamorig:RightHand'].matrix@r.data.bones['mixamorig:RightHand'].matrix_local.inverted()
  assert max((v.co-skin@knife.data.vertices[v.index].co).length for v in knife_mesh.vertices)<1e-5
  evaluated_knife.to_mesh_clear()
  for indices in sole.values():
   drift=max(drift,float(np.linalg.norm(pts[indices]-first[indices],axis=1).max()))
   stretch=max(stretch,float(np.abs(np.linalg.norm(pts[indices]-pts[indices[0]],axis=1)-np.linalg.norm(first[indices]-first[indices[0]],axis=1)).max()))
 seam=float(np.linalg.norm(last-first,axis=1).max());assert seam<.0001,(name,seam);assert stretch<.0001,(name,stretch)
 if name=='Idle':assert drift<.0001 and floor>-.005,(drift,floor)
 report['clips'][name]={'frames':b-a+1,'exact_woolly_pose_error':pose_error,'sampled_loop_seam':seam,'sole_rigid_shape_error':stretch,'minimum_height':floor,'sole_motion':drift}
 if name in ['Walk','Run']:
  s.frame_set(a+(b-a)//4);s.camera.location=(2,-5,1.65);s.camera.rotation_euler=(Vector((0,0,.84))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.ortho_scale=2.05;s.render.resolution_x=650;s.render.resolution_y=800;s.render.resolution_percentage=100;s.cycles.samples=12;s.render.filepath=str(ART/(name.lower()+'-check.png'));bpy.ops.render.render(write_still=True)
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.fbx(filepath=str(OUT/'Patchwork.fbx'))
imported={a.name.rsplit('|',1)[-1]for a in bpy.data.actions};assert {'Idle','Walk','Run'}<=imported,imported
exported_knife=bpy.data.objects.get('Patchwork_Knife');assert exported_knife and exported_knife.data.color_attributes
assert any(c.color[0]>.2 and c.color[1]<.04 for c in exported_knife.data.color_attributes.active_color.data)
report['knife']={'right_hand_rigid_binding':True,'fbx_vertex_palette_and_blood':True,'vertices':len(exported_knife.data.vertices)}
exported_grip=bpy.data.objects.get('Patchwork_Grip');assert exported_grip and exported_grip.data.color_attributes
report['grip']={'right_hand_rigid_binding':True,'fbx_vertex_palette':True,'vertices':len(exported_grip.data.vertices)}
report['fbx_actions']=sorted(imported);report['fbx_round_trip']=True
(ART/'validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
