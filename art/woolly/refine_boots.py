import bpy,json
from pathlib import Path
from mathutils import Vector
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_FeetFixed_v2.blend'))
s=bpy.context.scene;o=bpy.data.objects['Woolly_Body'];r=bpy.data.objects['Woolly_Rig']
backup=o.data.copy();backup.name='Woolly_PreBootWeights_Backup';backup.use_fake_user=True
old={v.index:{g.group:g.weight for g in v.groups} for v in o.data.vertices}
def smooth(a,b,z):
 t=max(0,min(1,(z-a)/(b-a)));return t*t*(3-2*t)
changed=[]
for v in o.data.vertices:
 z=v.co.z
 if z>=.31:continue
 side='Left' if v.co.x>0 else 'Right'
 foot=o.vertex_groups['mixamorig:'+side+'Foot'];leg=o.vertex_groups['mixamorig:'+side+'Leg']
 blend=1-smooth(.26,.31,z);legw=smooth(.14,.29,z)
 target={foot.index:1-legw,leg.index:legw}
 weights={idx:old[v.index].get(idx,0)*(1-blend)+target.get(idx,0)*blend for idx in set(old[v.index])|set(target)}
 total=sum(weights.values())
 for group_index in [g.group for g in v.groups]:o.vertex_groups[group_index].remove([v.index])
 for idx,w in weights.items():
  if w>1e-7:o.vertex_groups[idx].add([v.index],w/total,'REPLACE')
 changed.append(v.index)
# Validate weights and rigid sole distances throughout the run.
sole=[v.index for v in o.data.vertices if v.co.z<.07 and v.co.x>0];a,b=sole[0],max(sole,key=lambda i:(o.data.vertices[i].co-o.data.vertices[sole[0]].co).length)
base=(o.data.vertices[a].co-o.data.vertices[b].co).length;errors=[]
for f in range(1,18):
 s.frame_set(f);ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh();errors.append(abs((m.vertices[a].co-m.vertices[b].co).length-base));ev.to_mesh_clear()
report={'vertices_reweighted':len(changed),'max_weight_sum_error':max(abs(sum(g.weight for g in v.groups)-1) for v in o.data.vertices),'unweighted_vertices':sum(not v.groups for v in o.data.vertices),'left_sole_distance_error_over_17_frames':max(errors),'unchanged_vertices_above_z':.31}
(p/'boot_refinement_report.json').write_text(json.dumps(report,indent=2));print(report)
bpy.data.texts['READ_ME'].write('\nV3 boot cleanup: soles rigidly weighted to Foot; smooth transition to Leg across ankle/cuff. Toe/end-bone contamination removed below ankle. Pre-edit mesh datablock retained as Woolly_PreBootWeights_Backup.\n')
s.frame_set(1);bpy.ops.wm.save_as_mainfile(filepath=str(p/'Woolly_Adventurer_Refined_v3.blend'))
s.render.resolution_x=1000;s.render.resolution_y=800
cam=s.camera;center=Vector((0,0,.30));cam.data.ortho_scale=.9;cam.location=center+Vector((2,-2,.4));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler()
for f in [5,13]:
 s.frame_set(f);s.render.filepath=str(p/('boots_refined_%02d.png'%f));bpy.ops.render.render(write_still=True)
