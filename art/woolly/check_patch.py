import bpy,json,colorsys
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_PaintFix_v6.blend'));o=bpy.data.objects['Woolly_Body'];m=o.data;im=bpy.data.images['Woolly_BaseColor'];w,h=im.size;pix=im.pixels[:]
for t in json.loads((p/'cuff_paint_targets.json').read_text()):
 f=m.polygons[t['face']];uv=sum((m.uv_layers.active.data[i].uv for i in f.loop_indices),__import__('mathutils').Vector((0,0)))/len(f.loop_indices);ix=(int(uv.y*h)*w+int(uv.x*w))*4;rgb=pix[ix:ix+3];print('PATCH',t['face'],list(f.center),rgb,colorsys.rgb_to_hsv(*rgb),'mask',[m.attributes['Cuff_Paint_Repair_Mask'].data[i].value for i in f.loop_indices])
