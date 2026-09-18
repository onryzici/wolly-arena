import bpy
from mathutils import Vector
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_Revolver_v8.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];r.data.pose_position='REST';b=r.data.bones['mixamorig:RightHand'];s.render.resolution_x=900;s.render.resolution_y=900;s.cycles.samples=16
for obj in bpy.data.collections['Woolly_Revolver_Prop'].objects:obj.hide_render=True
cam=s.camera;cam.data.ortho_scale=.34;center=b.matrix_local@Vector((-.02,.13,0))
for name,loc in [('palm',(0,.12,1)),('back',(0,.12,-1)),('edge',(1,.12,0))]:
 cam.location=b.matrix_local@Vector(loc);cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(p/('hand_rest_'+name+'.png'));bpy.ops.render.render(write_still=True)
