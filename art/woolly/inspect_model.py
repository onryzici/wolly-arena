import bpy, json, bmesh
from mathutils import Vector
from pathlib import Path
out=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly')
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath='/Users/trexoinnovation/Downloads/Meshy_AI_Woolly_Adventurer_Running.glb')
r={'objects':[], 'materials':[], 'images':[], 'actions':[]}
for o in bpy.data.objects:
 d={'name':o.name,'type':o.type,'scale':list(o.scale),'dimensions':list(o.dimensions)}
 if o.type=='ARMATURE':d['bones']=[b.name for b in o.data.bones]
 if o.type=='MESH':
  m=o.data; bm=bmesh.new();bm.from_mesh(m)
  d.update(vertices=len(m.vertices),triangles=sum(len(p.vertices)-2 for p in m.polygons),uv_layers=[u.name for u in m.uv_layers],modifiers=[(x.name,x.type) for x in o.modifiers],loose_vertices=sum(not v.link_edges for v in bm.verts),boundary_edges=sum(e.is_boundary for e in bm.edges),nonmanifold_edges=sum(not e.is_manifold for e in bm.edges),degenerate_faces=sum(f.calc_area()<1e-12 for f in bm.faces),unweighted=sum(not v.groups for v in m.vertices));bm.free()
 r['objects'].append(d)
for m in bpy.data.materials:
 r['materials'].append({'name':m.name,'nodes':[(n.name,n.type, n.image.name if n.type=='TEX_IMAGE' and n.image else '') for n in m.node_tree.nodes]})
for i in bpy.data.images:r['images'].append({'name':i.name,'size':list(i.size),'colorspace':i.colorspace_settings.name,'packed':bool(i.packed_file)})
for a in bpy.data.actions:r['actions'].append({'name':a.name,'range':list(a.frame_range)})
(out/'inspection.json').write_text(json.dumps(r,indent=2))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
pts=[o.matrix_world@Vector(c) for o in meshes for c in o.bound_box]
lo=Vector([min(p[i] for p in pts) for i in range(3)]);hi=Vector([max(p[i] for p in pts) for i in range(3)]); center=(lo+hi)/2; size=max(hi-lo)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=24
scene.render.resolution_x=1000;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Studio World');scene.world.use_nodes=True;scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.16,.16,.16,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.7
bpy.ops.object.camera_add(location=center+Vector((0,-size*2.8,size*.28)));cam=bpy.context.object;cam.name='Preview Camera';cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=size*1.28;scene.camera=cam
for name,pos,power,scale in [('Key',(-1,-2,3),1000,2),('Fill',(2,-1,1),600,2),('Rim',(0,2,2),1000,1.5)]:
 bpy.ops.object.light_add(type='AREA',location=center+Vector(pos)*size);l=bpy.context.object;l.name=name;l.data.energy=power;l.data.shape='DISK';l.data.size=size*scale;l.rotation_euler=(center-l.location).to_track_quat('-Z','Y').to_euler()
scene.view_settings.view_transform='AgX'
for o in bpy.context.selected_objects:o.select_set(False)
for o in meshes:o.select_set(True)
bpy.context.view_layer.objects.active=meshes[0]
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':
   area.spaces.active.region_3d.view_distance=size*2
   area.spaces.active.region_3d.view_location=center
   area.spaces.active.shading.type='MATERIAL'
scene.frame_start=int(min((a.frame_range[0] for a in bpy.data.actions),default=1));scene.frame_end=int(max((a.frame_range[1] for a in bpy.data.actions),default=250));scene.frame_set(scene.frame_start)
bpy.ops.wm.save_as_mainfile(filepath=str(out/'Woolly_Adventurer_Editable.blend'))
for name,direction in [('front',(0,-1,.12)),('back',(0,1,.12)),('side',(1,0,.12))]:
 cam.location=center+Vector(direction)*size*2.8;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();scene.render.filepath=str(out/(name+'.png'));bpy.ops.render.render(write_still=True)
