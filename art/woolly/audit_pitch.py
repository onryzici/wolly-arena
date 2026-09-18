import bpy,math
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath='/Users/trexoinnovation/mobil-lamb-test/art/woolly/Woolly_Adventurer_SoleContact_v4.blend')
r=bpy.data.objects['Woolly_Rig'];r.animation_data.action=bpy.data.actions['Running']
for f in range(1,18):
 bpy.context.scene.frame_set(f);row=[]
 for side in ['Left','Right']:
  b=r.pose.bones['mixamorig:'+side+'Foot'];toe=r.data.bones['mixamorig:'+side+'ToeBase'];fw=toe.tail_local-toe.head_local;fw.z=0;fw.normalize();v=b.matrix.to_3x3()@b.bone.matrix_local.to_3x3().inverted()@fw
  row.append((side,round(math.degrees(math.atan2(v.z,-v.y)),1),round(b.head.z,3)))
 print(f,row)
