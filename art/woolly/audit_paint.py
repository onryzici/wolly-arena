import bpy
from pathlib import Path
from mathutils import Vector
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_NaturalContact_v5.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];r.data.pose_position='REST';s.render.resolution_x=1100;s.render.resolution_y=1000;s.cycles.samples=16
center=Vector((0,0,.98));cam=s.camera;cam.data.ortho_scale=1.15
for name,v in [('front',(0,-3,.2)),('back',(0,3,.2)),('left',(3,-.7,.1)),('right',(-3,-.7,.1))]:
 cam.location=center+Vector(v);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('paint_audit_'+name+'.png'));bpy.ops.render.render(write_still=True)
