"""Offline Blender authoring: named joint pivots, merged meshes per moving part."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
source=(ROOT/'art/woolly/expansion/build_models.py').read_text()
exec(source[source.index('import bpy'):source.index('for k,name in enumerate')].replace("ROOT=Path(__file__).resolve().parents[3]",'ROOT=Path(__file__).resolve().parents[1]'))
report={}
DARK='171529';BONE='E4BA70';GOLD='E89C16'
def joint(name,at,make):
 before=set(bpy.context.scene.objects);make();parts=[o for o in bpy.context.scene.objects if o not in before and o.type=='MESH']
 bpy.ops.object.select_all(action='DESELECT')
 for o in parts:
  o.select_set(True);bpy.context.view_layer.objects.active=o
  for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();mesh=bpy.context.object
 bpy.context.scene.cursor.location=at;bpy.ops.object.origin_set(type='ORIGIN_CURSOR');mesh.name=name
 return mesh
names=['MossMaw','SpineRaptor','HornBrute','StitchReaper','FrostWarden','ThornMatriarch']
for k,name in enumerate(names):
 clear();skin=['167F32','D52D17','532297','A7470F','0874B9','790EAA'][k];belly=['9CBC23','F09A20','B63881','DF961E','1DCCD4','E72E91'][k];large=k in (2,4);spider=k==5;hip=.68 if large else .52;head=1.42 if large else 1.15;w=.43 if large else .29
 def torso():
  sphere('Chest',(0,.02,hip+.24),(w,.25,.34),skin)
  sphere('Chest bib',(0,-.205,hip+.22),(w*.72,.075,.23),belly)
  cube('Leather waist',(0,0,hip-.1),(w*1.8,.43,.12),DARK)
  cube('Buckle',(0,-.23,hip-.1),(.13,.06,.13),GOLD,.025)
  if k==1:
   cone('Tail',(0,.18,hip),(0,.85,.40),.15,skin,.025)
   for j in range(4):cone('Spine',(0,.18+j*.14,hip+.13-j*.035),(0,.22+j*.14,hip+.34-j*.065),.08,BONE)
  if spider:sphere('Abdomen',(0,.43,.65),(.5,.53,.36),skin)
 joint('Torso',(0,0,hip),torso)
 def skull():
  sphere('Head',(0,-.035,head),(.35,.28,.3),skin)
  sphere('Muzzle',(0,-.29,head-.1),(.27,.19,.16),belly)
  sphere('Nose',(0,-.46,head-.04),(.09,.045,.07),DARK)
  for s in (-1,1):
   sphere('Eye socket',(s*.17,-.255,head+.045),(.14,.085,.14),DARK)
   sphere('Eye',(s*.17,-.313,head+.05),(.095,.038,.09),'F9EDBD')
   sphere('Pupil',(s*.16,-.348,head+.05),(.036,.018,.061),DARK)
   brow=cube('Brow',(s*.17,-.29,head+.16),(.29,.13,.085),skin);brow.rotation_euler[1]=-s*.2
   cone('Fang',(s*.20,-.4,head-.12),(s*.18,-.43,head-.28),.065,BONE)
   if k in (1,3):
    cone('Long ear',(s*.23,.015,head+.18),(s*.4,.015,head+.62),.14,skin,.025)
    cone('Ear inner',(s*.25,-.07,head+.25),(s*.37,-.045,head+.53),.065,belly)
   else:
    cone('Horn',(s*.27,.04,head+.2),(s*.47,.04,head+.5),.12,BONE,.05)
    cone('Horn tip',(s*.47,.04,head+.5),(s*.42,-.06,head+.64),.05,BONE)
  if k==0:
   for j in range(3):cone('Leaf crown',((j-1)*.13,.04,head+.22),((j-1)*.24,.03,head+.62-abs(j-1)*.12),.12,'6DB557')
  if k==4:
   for j in range(3):cone('Ice crown',((j-1)*.12,0,head+.25),((j-1)*.16,0,head+.65),.08,'B4F6EF')
  if k==3:
   for j in range(4):cube('Stitches',(-.12+j*.08,-.468,head-.12),(.035,.025,.075),DARK,.008)
 joint('Head',(0,0,head-.22),skull)
 for s,side in [(-1,'L'),(1,'R')]:
  def arm():
   cone('Upper arm',(s*(w-.01),0,hip+.4),(s*(w+.16),-.03,hip+.13),.12,skin,.10)
   sphere('Glove',(s*(w+.18),-.10,hip),(.18 if large else .13,.17,.18),DARK)
   cube('Cuff',(s*(w+.17),-.04,hip+.14),(.24,.25,.1),GOLD,.025)
   if large:cone('Shoulder spike',(s*w,0,hip+.48),(s*(w+.2),0,hip+.73),.13,'AEEBEA' if k==4 else BONE)
  joint('Arm'+side,(s*w,0,hip+.4),arm)
  if not spider:
   def leg():
    cone('Thigh',(s*.21,0,hip),(s*.23,.04,.3),.13,skin,.105)
    sphere('Knee',(s*.23,-.02,.29),(.125,.13,.13),belly)
    cone('Shin',(s*.23,.03,.29),(s*.23,-.03,.13),.105,DARK,.085)
    cube('Boot',(s*.23,-.13,.105),(.25,.39,.21),DARK,.075)
    for j in (-1,1):cone('Toe claw',(s*.23+j*.065,-.25,.1),(s*.23+j*.065,-.36,.055),.045,BONE)
   joint('Leg'+side,(s*.21,0,hip),leg)
  else:
   for j in range(3):
    def leg():
     cone('Upper spider leg',(s*.3,.38-j*.3,.67),(s*.74,.57-j*.44,.4),.09,skin,.06)
     cone('Lower spider leg',(s*.74,.57-j*.44,.4),(s*.9,.65-j*.5,.05),.065,DARK,.025)
    joint('Spider'+side+str(j),(s*.3,.38-j*.3,.67),leg)
 # Bake each part's palette into vertex colors: one material per animated mesh.
 for obj in list(bpy.context.scene.objects):
  if obj.type!='MESH':continue
  colors=obj.data.color_attributes.new(name='Color',type='FLOAT_COLOR',domain='CORNER')
  for poly in obj.data.polygons:
   rgba=obj.data.materials[poly.material_index].diffuse_color
   for loop in poly.loop_indices:colors.data[loop].color_srgb=rgba
  obj.data.materials.clear();obj.data.materials.append(bpy.data.materials.get('EnemyVertexPalette') or bpy.data.materials.new('EnemyVertexPalette'))
  for poly in obj.data.polygons:poly.material_index=0
 bpy.ops.object.select_all(action='SELECT')
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},bake_anim=False,colors_type='LINEAR',axis_forward='-Z',axis_up='Y')
 report[name]={'vertices':sum(len(o.data.vertices) for o in bpy.context.scene.objects if o.type=='MESH'),'joints':[o.name for o in bpy.context.scene.objects]}
 bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'art/woolly/expansion'/f'{name}_Articulated.blend'))
(OUT/'enemy-budget.json').write_text(json.dumps(report,indent=2))
print(report)
