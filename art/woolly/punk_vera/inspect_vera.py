import bpy,json
from pathlib import Path
from mathutils import Vector
p=Path(__file__).resolve().parent
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.ops.import_scene.gltf(filepath=str(next(p.rglob('*Running*.glb'))))
r=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE');o=next(o for o in bpy.context.scene.objects if o.type=='MESH');s=bpy.context.scene
info={'rig':r.name,'rig_matrix':[list(row) for row in r.matrix_world],'mesh':o.name,'mesh_matrix':[list(row)for row in o.matrix_world],'dimensions':list(o.dimensions),'vertices':len(o.data.vertices),'actions':[(a.name,list(a.frame_range))for a in bpy.data.actions],'bones':{b.name:{'head':list(b.head_local),'tail':list(b.tail_local)}for b in r.data.bones},'images':[(i.name,list(i.size))for i in bpy.data.images]}
(p/'inspection.json').write_text(json.dumps(info,indent=2));print(json.dumps(info))
for track in r.animation_data.nla_tracks:track.mute=True
r.animation_data.action=next(a for a in bpy.data.actions if 'Running' in a.name);s.frame_set(1)
bpy.ops.object.camera_add(location=(3,-6,2.5));cam=bpy.context.object;center=Vector((0,0,1));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.8;s.camera=cam
bpy.ops.object.light_add(type='AREA',location=(1,-3,5));bpy.context.object.data.energy=450;bpy.context.object.data.size=5
s.world.color=(.3,.3,.3);s.render.engine='CYCLES';s.cycles.samples=16;s.render.resolution_x=700;s.render.resolution_y=850;s.render.resolution_percentage=100;s.render.filepath=str(p/'before.png');bpy.ops.render.render(write_still=True)
bpy.ops.wm.save_as_mainfile(filepath=str(p/'PunkVera_Source.blend'))
