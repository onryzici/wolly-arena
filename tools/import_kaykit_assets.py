"""Convert the free KayKit Skeleton GLBs to compact FBX, keeping selected authored clips."""
import bpy,json,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];src=ROOT/'art/third-party/kaykit';out=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/KayKit';out.mkdir(parents=True,exist_ok=True)
keep={'Idle','Walking_A','Running_A','1H_Melee_Attack_Chop','Spellcast_Shoot','Hit_A','Death_A'}
report=[]
for path in sorted((src/'skeleton').glob('*.glb')):
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(path))
 for o in bpy.context.scene.objects:
  if o.animation_data:
   o.animation_data.action=None
   for track in list(o.animation_data.nla_tracks):
    for strip in list(track.strips):
     if strip.action and strip.action.name not in keep:track.strips.remove(strip)
    if not track.strips:o.animation_data.nla_tracks.remove(track)
 for action in list(bpy.data.actions):
  if action.name not in keep:bpy.data.actions.remove(action)
 rig=next(o for o in bpy.context.scene.objects if o.type=='ARMATURE')
 bpy.ops.object.select_all(action='SELECT')
 bpy.ops.export_scene.fbx(filepath=str(out/(path.stem+'.fbx')),use_selection=True,object_types={'MESH','ARMATURE'},add_leaf_bones=False,apply_scale_options='FBX_SCALE_UNITS',bake_anim=True,bake_anim_use_nla_strips=False,bake_anim_use_all_actions=True,bake_anim_simplify_factor=.3,axis_forward='-Z',axis_up='Y')
 report.append({'name':path.stem,'bones':len(rig.data.bones),'clips':[a.name for a in bpy.data.actions],'vertices':sum(len(o.data.vertices) for o in bpy.context.scene.objects if o.type=='MESH')})
for path in (src/'dungeon').glob('*.fbx'):shutil.copy2(path,out/path.name)
for folder,texture in [('skeleton','skeleton_texture.png'),('dungeon','dungeon_texture.png')]:shutil.copy2(src/folder/texture,out/texture)
license_out=ROOT/'WoollyArenaTest/Assets/Woolly/ThirdParty/KayKit';license_out.mkdir(parents=True,exist_ok=True)
for folder in ['skeleton','dungeon']:shutil.copy2(src/folder/'LICENSE.txt',license_out/(folder+'-LICENSE.txt'))
(license_out/'conversion-report.json').write_text(json.dumps(report,indent=2));print(report)
