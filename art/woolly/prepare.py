import bpy,bmesh,json
from mathutils import Vector,Quaternion
from pathlib import Path
out=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.open_mainfile(filepath=str(out/'Woolly_Adventurer_Editable.blend'))
s=bpy.context.scene;o=bpy.data.objects['output_unwrapped'];rig=o.parent
print('CUSTOM',[(b.name,b.custom_shape.name if b.custom_shape else None) for b in rig.pose.bones]);print('NLA',[(t.name,[(x.name,x.action.name,x.frame_start,x.frame_end) for x in t.strips]) for t in rig.animation_data.nla_tracks])
o.name='Woolly_Body';rig.name='Woolly_Rig';rig.show_in_front=True
# Preserve a pristine mesh datablock for reversible mesh editing.
original=o.data.copy();original.name='Woolly_OriginalMesh_Backup';original.use_fake_user=True
# Weld only coincident points that carry the same skin weights; retain UVs and corner normals.
m=o.data;normals=[tuple(n.vector) for n in m.corner_normals];uvs=[tuple(l.uv) for l in m.uv_layers.active.data]
weights={v.index:tuple(sorted((g.group,round(g.weight,7)) for g in v.groups)) for v in m.vertices}
bm=bmesh.new();bm.from_mesh(m);bm.verts.ensure_lookup_table();groups={};mapping={}
for v in bm.verts:
 key=(tuple(round(c,7) for c in v.co),weights[v.index])
 if key in groups and (v.co-groups[key].co).length<1e-7:mapping[v]=groups[key]
 else:groups[key]=v
before=len(m.vertices)
bmesh.ops.weld_verts(bm,targetmap=mapping);bm.to_mesh(m);bm.free();m.update()
if len(m.loops)==len(normals) and all(tuple(l.uv)==uvs[n] for n,l in enumerate(m.uv_layers.active.data)):
 m.normals_split_custom_set(normals)
else:
 o.data=original.copy();m=o.data;print('WELD_REVERTED')
print('VERTICES',before,len(m.vertices))
# Editable textures stored beside the project and also packed for portability.
texdir=out/'textures';texdir.mkdir(exist_ok=True)
for img in list(bpy.data.images):
 if img.type!='IMAGE':continue
 if img.name=='texture_0_metallic_roughness':
  backup=img.copy();backup.name='Woolly_MetallicRoughness_4K_Backup';backup.use_fake_user=True
  img.scale(2048,2048)
 names={'texture_0':'Woolly_BaseColor','normal':'Woolly_Normal','texture_0_metallic_roughness':'Woolly_MetallicRoughness'}
 if img.name in names:
  img.name=names[img.name];img.filepath_raw=str(texdir/(img.name+'.png'));img.file_format='PNG';img.save();img.pack()
mat=o.active_material;mat.name='Woolly_Paintable_PBR'
for n in mat.node_tree.nodes:
 n.select=False
 if n.type=='TEX_IMAGE' and n.image.name=='Woolly_BaseColor':n.select=True;mat.node_tree.nodes.active=n
# Hide studio apparatus in viewport while leaving render lighting functional.
for x in s.objects:
 if x.type in {'LIGHT','CAMERA'}:x.hide_set(True)
# Frame the character in a useful front view, with the rig selectable in the outliner.
rig.show_in_front=False
s.frame_start=1;s.frame_end=17;s.frame_set(1)
for x in bpy.context.selected_objects:x.select_set(False)
o.select_set(True);bpy.context.view_layer.objects.active=o
pts=[o.matrix_world@Vector(c) for c in o.bound_box];lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)]);center=(lo+hi)/2;size=max(hi-lo)
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':
   sp=area.spaces.active;sp.region_3d.view_distance=size*1.7;sp.region_3d.view_location=center;sp.region_3d.view_rotation=Quaternion((1,0,0),1.57079632679);sp.region_3d.view_perspective='ORTHO';sp.shading.type='MATERIAL';sp.overlay.show_floor=False
s.camera.hide_set(False);s.camera.location=center+Vector((0,-size*3,size*.2));s.camera.rotation_euler=(center-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.ortho_scale=size*1.22;s.camera.hide_set(True)
text=bpy.data.texts.new('READ_ME')
text.write('Woolly editable working file\nWoolly_Body: Tab for mesh edits; Texture Paint for base-color painting.\nWoolly_Rig: select and enter Pose Mode. Running: frames 1-17 at imported 24 fps.\nAll textures packed; editable PNG copies in textures/.\nOriginal mesh and 4K packed map retained as fake-user datablock backups.\nCoincident vertices with matching skin weights welded; UVs and custom corner normals preserved.\nMetallic/roughness reduced from 4K to 2K. Base color and normal remain 2K.\nGLB source remains unchanged.\n')
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Woolly_Adventurer_Editable.blend'))
s.render.filepath=str(out/'optimized_front.png');bpy.ops.render.render(write_still=True)
s.frame_set(9);s.render.filepath=str(out/'optimized_run_frame09.png');bpy.ops.render.render(write_still=True)
report={'vertices_before':before,'vertices_after':len(m.vertices),'triangles':sum(len(p.vertices)-2 for p in m.polygons),'bones':len(rig.data.bones),'uv_preserved':all(tuple(l.uv)==uvs[n] for n,l in enumerate(m.uv_layers.active.data)),'frames':[1,17],'unweighted_vertices':sum(not v.groups for v in m.vertices)}
(out/'optimization.json').write_text(json.dumps(report,indent=2))
