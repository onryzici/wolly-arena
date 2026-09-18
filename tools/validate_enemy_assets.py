"""Run with Blender -b --python; validates the actual shipped FBXs without Unity."""
import bpy, json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
folder=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Models'
report=[]
for name in ['MossMaw','SpineRaptor','HornBrute','StitchReaper','FrostWarden','ThornMatriarch']:
 bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
 bpy.ops.import_scene.fbx(filepath=str(folder/(name+'.fbx')))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
 names={o.name for o in meshes};expected={'Head','Torso','ArmL','ArmR'}
 expected|={f'Spider{s}{j}' for s in 'LR' for j in range(3)} if name=='ThornMatriarch' else {'LegL','LegR'}
 assert names==expected,(name,names)
 assert all(len(o.data.materials)==1 and o.data.color_attributes for o in meshes),name
 assert all(o.data.materials[0].name.startswith('EnemyVertexPalette') for o in meshes),name
 vertices=sum(len(o.data.vertices) for o in meshes)
 assert vertices<5000,(name,vertices)
 # FBX reimport must preserve independent, nonzero anatomical origins.
 assert all(o.location.z>.3 for o in meshes if o.name.startswith(('Head','Leg','Arm'))),name
 report.append({'model':name,'vertices':vertices,'moving_parts':len(meshes),'materials_per_part':1,'result':'PASS'})
path=ROOT/'WoollyArenaTest/Logs/enemy-asset-review.json';path.write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
