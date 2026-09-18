import bpy,json,numpy as np
from pathlib import Path
from mathutils import Vector
p=Path(__file__).resolve().parent;bpy.ops.wm.open_mainfile(filepath=str(p/'PunkVera_Source.blend'));r=bpy.data.objects['target_character'];o=bpy.data.objects['output_unwrapped'];r.data.pose_position='REST';s=bpy.context.scene
vs=np.array([v.co[:]for v in o.data.vertices]);print('REST BOUNDS',vs.min(0),vs.max(0))
for z in [.35,.5,.65,.8,.95]:
 a=vs[(vs[:,2]>z)&(vs[:,2]<z+.15)&(vs[:,1]>.05)]
 if len(a):print('BACK',z,len(a),a.min(0),a.max(0),a.mean(0))
s.camera.location=(2,5,2);s.camera.rotation_euler=(Vector((0,0,.8))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.ortho_scale=2.1;s.render.filepath=str(p/'tail-back-rest.png');bpy.ops.render.render(write_still=True)
s.camera.location=(0,5,.85);s.camera.rotation_euler=(Vector((0,0,.85))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.ortho_scale=1.2;s.render.filepath=str(p/'tail-back-close.png');bpy.ops.render.render(write_still=True)
