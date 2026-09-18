"""Offline source inspection and reproducible rest-space Blender input from the extracted GLB."""
import bpy,json
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parents[1];art=root/'art/woolly/patchwork'
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(next((art/'source').glob('*Walking*'))))
s=bpy.context.scene;r=next(o for o in s.objects if o.type=='ARMATURE');body=next(o for o in s.objects if o.type=='MESH');r.animation_data_clear();r.data.pose_position='REST';bpy.context.view_layer.update()
report={'rig':r.name,'rig_matrix':[list(row)for row in r.matrix_world],'body_matrix':[list(row)for row in body.matrix_world],'vertices':len(body.data.vertices),'bones':{b.name:{'head':list(b.head_local),'tail':list(b.tail_local)}for b in r.data.bones},'bounds':[list(body.matrix_world@Vector(v))for v in body.bound_box]}
(art/'source-inspection.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
pts=[body.matrix_world@Vector(v)for v in body.bound_box];center=sum(pts,Vector())/8;h=max(v.z for v in pts)-min(v.z for v in pts)
s.render.engine='CYCLES';s.cycles.samples=12;s.render.resolution_x=650;s.render.resolution_y=800;s.render.resolution_percentage=100;s.world=bpy.data.worlds.new('World');s.world.color=(.4,.4,.4);s.render.film_transparent=True
for pos,power in [((2,-4,5),650),((-3,-2,3),400)]:
 bpy.ops.object.light_add(type='AREA',location=pos);o=bpy.context.object;o.data.energy=power;o.data.size=4
bpy.ops.object.camera_add();cam=bpy.context.object;cam.data.type='ORTHO';cam.data.ortho_scale=h*1.2;s.camera=cam
for name,offset in [('source-front',(0,-5,0)),('source-side',(5,0,0))]:
 cam.location=center+Vector(offset);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(art/(name+'.png'));bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(art/'Patchwork_Source.blend'))
