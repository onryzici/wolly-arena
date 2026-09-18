import bpy,numpy as np,json
bpy.ops.wm.open_mainfile(filepath='/Users/trexoinnovation/mobil-lamb-test/art/woolly/Woolly_Adventurer_Editable.blend')
for o in bpy.data.objects:
 print('OBJ',o.name,'hidden',o.hide_render,o.hide_viewport,'display',o.display_type,'parent',o.parent,'animation',o.animation_data)
for i in bpy.data.images:
 if i.type=='IMAGE':
  a=np.empty(len(i.pixels),dtype=np.float32);i.pixels.foreach_get(a);a=a.reshape(-1,4);print('IMAGE',i.name,[(float(a[:,c].min()),float(a[:,c].max()),float(a[:,c].std())) for c in range(4)])
o=bpy.data.objects['output_unwrapped']; coords=np.array([v.co[:] for v in o.data.vertices]);print('UNIQUE',len(np.unique(np.round(coords,6),axis=0)))
for a in bpy.data.actions:print('ACTION',a.name,'users',a.users)
print('FPS',bpy.context.scene.render.fps)
