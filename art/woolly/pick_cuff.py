import bpy,json
from mathutils import Vector
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_NaturalContact_v5.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];r.data.pose_position='REST';bpy.context.view_layer.update();cam=s.camera;cam.data.ortho_scale=.32
out=[]
for side,sign,points in [('left',1,[(670,617),(556,449)]),('right',-1,[(224,605),(341,436)])]:
 center=Vector((sign*.43,.05,.71));cam.location=center+Vector((sign*2,2,-.7));cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();bpy.context.view_layer.update()
 for x,y in points:
  origin=cam.matrix_world@Vector(((x/900-.5)*.32,(.5-y/900)*.32,0));direction=cam.matrix_world.to_3x3()@Vector((0,0,-1));hit,loc,n,idx,obj,mat=s.ray_cast(bpy.context.evaluated_depsgraph_get(),origin,direction)
  print(side,x,y,hit,list(loc),idx,obj.name);out.append({'side':side,'point':list(loc),'face':idx})
(p/'cuff_paint_targets.json').write_text(json.dumps(out,indent=2))
