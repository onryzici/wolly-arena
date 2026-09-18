import bpy,bmesh,json
from pathlib import Path
from mathutils import Vector
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_FeetFixed_v2.blend'))
s=bpy.context.scene;o=bpy.data.objects['Woolly_Body'];r=bpy.data.objects['Woolly_Rig']
for low,high in [(0,.07),(.07,.13),(.13,.19),(.19,.25),(.25,.3)]:
 vs=[v for v in o.data.vertices if low<=v.co.z<high];tot={}
 for v in vs:
  for g in v.groups:
   name=o.vertex_groups[g.group].name;tot[name]=tot.get(name,0)+g.weight
 print('BAND',low,high,len(vs),sorted(tot.items(),key=lambda x:-x[1])[:8])
bm=bmesh.new();bm.from_mesh(o.data)
print('BOUNDARY',[(tuple(e.verts[0].co),tuple(e.verts[1].co)) for e in bm.edges if e.is_boundary]);print('NONMANIFOLD',[(tuple(e.verts[0].co),len(e.link_faces)) for e in bm.edges if not e.is_manifold and not e.is_boundary]);bm.free()
s.render.resolution_x=1000;s.render.resolution_y=800
for mode in ['REST','POSE']:
 r.data.pose_position=mode;s.frame_set(5);bpy.context.view_layer.update()
 cam=s.camera;center=Vector((0,0,.30));cam.data.ortho_scale=.9;cam.location=center+Vector((2,-2,.4));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('boots_before_'+mode+'.png'));bpy.ops.render.render(write_still=True)
