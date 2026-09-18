import bpy,json
from mathutils import Vector
bpy.ops.wm.open_mainfile(filepath='/Users/trexoinnovation/mobil-lamb-test/art/woolly/Woolly_Adventurer_Editable.blend')
r=bpy.data.objects['Woolly_Rig'];ad=r.animation_data
print('ACTIVE',ad.action.name if ad.action else None,'slot',ad.action_slot)
for t in ad.nla_tracks: print('TRACK',t.name,t.mute,[(s.action.name,s.blend_type,s.influence,s.extrapolation) for s in t.strips])
for a in bpy.data.actions:
 print('ACTION',a.name)
 for layer in a.layers:
  for strip in layer.strips:
   for bag in strip.channelbags:
    print('CHANNELS',[(f.data_path,f.array_index,len(f.keyframe_points)) for f in bag.fcurves][:15])
for side in ['Left','Right']:
 for n in ['UpLeg','Leg','Foot','ToeBase','Toe_End']:
  b=r.data.bones['mixamorig:'+side+n];print('REST',b.name,list(b.head_local),list(b.tail_local))
for f in [1,3,5,7,9,11,13,15,17]:
 bpy.context.scene.frame_set(f)
 for side in ['Left','Right']:
  b=r.pose.bones['mixamorig:'+side+'Foot'];toe=r.pose.bones['mixamorig:'+side+'ToeBase'];v=toe.head-b.head
  print('FRAME',f,side,'ankle',list(b.head),'forward',list(v.normalized()),'rotation',list(b.rotation_quaternion))
