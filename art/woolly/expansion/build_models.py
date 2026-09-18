import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Models'
OUT.mkdir(parents=True,exist_ok=True)
REPORT={}
def clear():
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(hex):
 name='C_'+hex
 m=bpy.data.materials.get(name) or bpy.data.materials.new(name)
 m.diffuse_color=tuple(int(hex[i:i+2],16)/255 for i in (0,2,4))+(1,)
 return m
def finish(o,name,p,s,c):
 o.name=name;o.location=p;o.scale=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(mat(c));return o
def sphere(name,p,s,c):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=16,ring_count=10);o=finish(bpy.context.object,name,p,s,c)
 for poly in o.data.polygons:poly.use_smooth=True
 return o
def cube(name,p,s,c,bevel=.07):
 bpy.ops.mesh.primitive_cube_add(size=1);o=finish(bpy.context.object,name,p,s,c)
 mod=o.modifiers.new('Soft crafted edges','BEVEL');mod.width=bevel;mod.segments=2
 bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=mod.name)
 o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');return o
def cone(name,a,b,r,c,r2=0):
 d=Vector(b)-Vector(a);bpy.ops.mesh.primitive_cone_add(vertices=12,radius1=r,radius2=r2,depth=d.length);o=bpy.context.object;o.name=name;o.location=(Vector(a)+Vector(b))*.5;o.rotation_euler=d.to_track_quat('Z','Y').to_euler();o.data.materials.append(mat(c));return o
def torus(name,p,r,t,c,rotation=(0,0,0)):
 bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=t,major_segments=20,minor_segments=6,location=p,rotation=rotation);o=bpy.context.object;o.name=name;o.data.materials.append(mat(c));return o
def export(name):
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 for o in meshes:
  bpy.context.view_layer.objects.active=o
  for mod in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
 bpy.ops.object.select_all(action='DESELECT')
 for o in meshes:o.select_set(True)
 bpy.context.view_layer.objects.active=meshes[0];bpy.ops.object.join();o=bpy.context.object;o.name=name
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.export_scene.fbx(filepath=str(OUT/(name+'.fbx')),use_selection=True,object_types={'MESH'},add_leaf_bones=False,bake_anim=False,axis_forward='-Z',axis_up='Y')
 REPORT[name]={'vertices':len(o.data.vertices),'polygons':len(o.data.polygons),'dimensions':list(o.dimensions)}
 return o
DARK='252333';BONE='F7DEAA';GOLD='E8B448'
def face(w,z,front,angry=True):
 for side in (-1,1):
  sphere('Ivory eye',(side*w,-front,z),(.14,.07,.14),BONE)
  sphere('Vertical pupil',(side*w,-front-.064,z),(.045,.028,.082),DARK)
  brow=cube('Heavy brow',(side*w,-front-.04,z+.13),(.34,.1,.10),DARK,.035);brow.rotation_euler[1]=side*-.22
 sphere('Mouth',(0,-front+.012,z-.23),(.26,.065,.14),DARK)
 for x in (-.16,-.055,.055,.16):cone('Fang',(x,-front-.058,z-.16),(x,-front-.07,z-.29),.045,BONE)
for k,name in enumerate(['MossMaw','SpineRaptor','HornBrute','StitchReaper','FrostWarden','ThornMatriarch']):
 clear();skin=['719A3D','BA4C66','695A99','B18354','589BAF','774886'][k];belly=['BED16D','E99986','A396C0','E5C48F','B6EBEE','C582B2'][k]
 boss=k>=4
 if k==0:
  sphere('Pear body',(0,0,.55),(.47,.34,.48),skin);sphere('Jaw',(0,-.22,.42),(.4,.22,.22),belly);face(.19,.74,.31)
  for x,z in [(-.28,1),(-.08,1.16),(.16,1.1),(.33,.93)]:cone('Leaf crest',(x,.06,z-.2),(x*1.3,.04,z+.08),.13,'426743')
 elif k==1:
  sphere('Runner haunch',(0,.12,.58),(.3,.36,.34),skin);sphere('Long head',(0,-.15,.91),(.32,.37,.28),skin);face(.15,.98,.44)
  for i in range(5):cone('Spine',(0,.05+i*.13,.95-i*.09),(0,.10+i*.17,1.22-i*.09),.105,BONE)
  cone('Swept tail',(0,.30,.58),(0,.85,.42),.19,skin,.02)
 elif k in (2,4):
  sphere('Broad shoulders',(0,0,.93),(.62,.37,.5),skin);sphere('Belly plate',(0,-.25,.67),(.39,.15,.38),belly);sphere('Skull',(0,-.04,1.32),(.36,.32,.32),skin);face(.16,1.38,.34)
  for side in (-1,1):
   cone('Horn root',(side*.27,0,1.48),(side*.49,.02,1.75),.14,BONE,.085);cone('Hook horn',(side*.49,.02,1.75),(side*.39,-.09,1.92),.085,BONE)
   sphere('Huge fist',(side*.66,-.08,.56),(.23,.27,.26),skin)
   for j in range(3):cone('Shoulder shard',(side*(.39+j*.1),.02,1.13),(side*(.48+j*.14),.02,1.45+(j%2)*.12),.09,'A6F2ED' if boss else BONE)
  if boss:torus('Warden belt',(0,0,.62),.42,.075,GOLD)
 elif k==3:
  sphere('Sack torso',(0,0,.64),(.34,.24,.4),skin);sphere('Stitched hood',(0,0,1.12),(.34,.26,.32),belly)
  for side in (-1,1):
   sphere('Button eye',(side*.14,-.25,1.16),(.09,.03,.10),DARK)
   for sign in (-1,1):
    stitch=cube('Eye cross',(side*.14,-.283,1.16),(.13,.022,.026),BONE,.006);stitch.rotation_euler[1]=sign*.7
   cone('Rag ear',(side*.23,0,1.32),(side*.39,.02,1.57),.13,skin)
  for i in range(5):cube('Sack stitches',(-.12+i*.06,-.249,.96),(.025,.025,.12),DARK,.004)
  cone('Needle arm',(.3,0,.75),(.70,-.1,.44),.055,DARK,.02)
 else:
  sphere('Armoured abdomen',(0,.15,.67),(.63,.52,.5),skin);sphere('Face',(0,-.3,1.14),(.4,.3,.35),skin);face(.2,1.22,.55)
  for side in (-1,1):
   for i in range(3):
    cone('Spider limb',(side*.42,.3-i*.28,.8),(side*.89,.45-i*.38,.51),.105,skin,.07)
    cone('Spider claw',(side*.89,.45-i*.38,.51),(side*1.02,.50-i*.4,.08),.07,DARK)
   for i in range(3):cone('Thorn crown',(side*(.14+i*.13),-.18,1.39),(side*(.21+i*.18),-.14,1.8-i*.13),.095,GOLD)
 if k!=5:
  for side in (-1,1):
   sphere('Boot claw',(side*(.26 if k in (2,4) else .2),-.12,.16),(.20,.27,.14),DARK)
   if k not in (2,4):sphere('Arm',(side*.38,0,.54),(.13,.17,.25),skin)
 export(name)
for kind,name in enumerate(['FrontierRevolver','CopperRepeater','TwinBarrel','GalaxyRelic','ArcCoil','StarLance']):
 clear();steel='3D5064';wood='814E3B';ivory='E6D7B2'
 if kind<3:
  cube('Receiver',(0,0,.03),(.21,.34,.20),steel,.04)
  grip=cube('Carved walnut grip',(0,.10,-.16),(.15,.17,.28),wood,.045);grip.rotation_euler[0]=-.25
  cube('Grip inlay',(0,.19,-.16),(.075,.015,.14),ivory,.02)
  for side in (-1,1):sphere('Brass screw',(side*.081,.11,-.1),(.018,.023,.023),GOLD)
  for j in range(2 if kind==2 else 1):
   x=(j-.5)*.13 if kind==2 else 0;end=-.62 if kind==1 else -.47
   cone('Steel barrel',(x,-.13,.07),(x,end,.07),.062,steel,.052)
   torus('Muzzle collar',(x,end,.07),.052,.012,GOLD,(math.pi/2,0,0));cone('Dark bore',(x,end-.001,.07),(x,end-.008,.07),.04,DARK,.04)
  if kind==0:
   cone('Six shot cylinder',(0,.06,.04),(0,-.12,.04),.135,GOLD,.135)
   for i in range(6):
    a=i*math.pi/3;cone('Cylinder flute',(math.cos(a)*.115,.045,.04+math.sin(a)*.115),(math.cos(a)*.115,-.11,.04+math.sin(a)*.115),.025,steel,.025)
  else:
   stock=cube('Walnut stock',(0,.30,-.07),(.17,.39,.22),wood,.06)
   cube('Brass buttplate',(0,.49,-.07),(.19,.045,.24),GOLD,.02)
   cube('Foregrip',(0,-.25,-.035),(.23,.24,.10),wood,.035)
   if kind==1:
    cone('Scope',(0,.03,.23),(0,-.24,.23),.065,steel,.065);sphere('Scope lens',(0,-.25,.23),(.044,.015,.044),'65DCE3')
  cube('Front sight',(0,-.39,.145),(.025,.065,.06),GOLD,.008)
  torus('Trigger guard',(0,.025,-.17),.09,.014,GOLD,(0,math.pi/2,0))
 else:
  color=['9A71EB','5FE8F1','FFC76A'][kind-3]
  sphere('Radiant core',(0,0,0),(.145,.18,.145),color)
  for i in range(3):torus('Gilded orbit',(0,0,0),.23+i*.02,.022,GOLD,(i*.8,math.pi/2+i*.4,0))
  for side in (-1,1):
   cube('Relic housing',(side*.2,.02,0),(.1,.30,.14),steel,.04)
   cone('Crystal prong',(side*.18,-.1,0),(side*.12,-.43,0),.075,color)
  if kind==4:
   for i in range(5):torus('Electric coil',(0,-.12-i*.065,0),.11-i*.007,.018,color,(math.pi/2,0,0))
   cone('Coil tip',(0,-.37,0),(0,-.57,0),.065,steel,.02)
  if kind==5:
   for i in range(5):
    a=i*math.pi*.4;cone('Five point star',(math.cos(a)*.10,0,math.sin(a)*.10),(math.cos(a)*.38,0,math.sin(a)*.38),.095,color)
 export(name)
clear()
for z,r in [(1.05,.65),(1.55,.5),(1.95,.34)]:
 cone('Frozen fir',(0,0,z-.6),(0,0,z+.45),r,'3F7983',.01);cone('Snow mantle',(0,0,z-.38),(0,0,z+.49),r*.88,'CEEDEF',.01)
cone('Trunk',(0,0,0),(0,0,.8),.15,'586777',.11);export('FrostFir')
clear()
for x,y,z,r in [(0,0,1.5,.29),(.32,.1,.9,.22),(-.25,-.12,.7,.2)]:
 cone('Ice crystal',(x,y,.08),(x,y,z),r,'76D7E7',r*.6);cone('Crystal tip',(x,y,z),(x,y,z+.28),r*.6,'C4F5F0')
export('IceCluster')
clear()
cube('Glacial boulder',(0,0,.5),(2.4,1.3,1),'83BACB',.24)
cube('Snow cap',(0,0,.97),(2.35,1.27,.20),'D8F1F0',.12)
export('IceWall')
(OUT/'model-budget.json').write_text(json.dumps(REPORT,indent=2))
print(json.dumps(REPORT,indent=2))
