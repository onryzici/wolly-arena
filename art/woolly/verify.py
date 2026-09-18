import bpy,numpy as np,json,bmesh
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Editable.blend'))
o=bpy.data.objects['Woolly_Body'];ref=o.copy();ref.data=bpy.data.meshes['Woolly_OriginalMesh_Backup'];bpy.context.scene.collection.objects.link(ref)
def corners(obj):
 e=obj.evaluated_get(bpy.context.evaluated_depsgraph_get());m=e.to_mesh();a=np.array([m.vertices[l.vertex_index].co[:] for l in m.loops]);e.to_mesh_clear();return a
errors=[]
for f in range(1,18):
 bpy.context.scene.frame_set(f);errors.append(float(np.max(np.abs(corners(o)-corners(ref)))))
bm=bmesh.new();bm.from_mesh(o.data)
r={'max_animated_corner_position_error':max(errors),'frames_checked':17,'boundary_edges_after':sum(e.is_boundary for e in bm.edges),'nonmanifold_edges_after':sum(not e.is_manifold for e in bm.edges),'triangles':sum(len(x.vertices)-2 for x in o.data.polygons)}
bm.free();(p/'verification.json').write_text(json.dumps(r,indent=2));print(r)
