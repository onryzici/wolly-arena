import bpy
from pathlib import Path
from mathutils import Vector
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_NaturalContact_v5.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];r.data.pose_position='REST';s.render.resolution_x=900;s.render.resolution_y=900;s.cycles.samples=16;cam=s.camera;cam.data.ortho_scale=.32
for side,sign in [('left',1),('right',-1)]:
 center=Vector((sign*.43,.05,.71));cam.location=center+Vector((sign*2,2,-.7));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('cuff_'+side+'_before.png'));bpy.ops.render.render(write_still=True)
