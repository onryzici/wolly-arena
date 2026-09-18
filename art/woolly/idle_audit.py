import bpy
bpy.ops.wm.open_mainfile(filepath='/Users/trexoinnovation/mobil-lamb-test/art/woolly/Woolly_Adventurer_PaintFix_v6.blend')
r=bpy.data.objects['Woolly_Rig']
for b in r.data.bones:print(b.name,'parent',b.parent.name if b.parent else '-',tuple(b.head_local),tuple(b.tail_local))
print('transform',r.matrix_world)
