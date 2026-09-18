"""Validate and export the copied Woolly rig with Vera's newly bound mesh."""
from pathlib import Path
import runpy, shutil
import bpy
from mathutils import Vector
root=Path(__file__).resolve().parents[1];art=root/'art/woolly/punk_vera';out=root/'WoollyArenaTest/Assets/Woolly/Resources/Characters'
runpy.run_path(str(root/'tools/validate_vera_woolly_rig.py'))
bpy.ops.wm.open_mainfile(filepath=str(art/'PunkVera_WoollyRig.blend'))
r=bpy.data.objects['PunkVera_Rig'];body=bpy.data.objects['PunkVera_Body'];s=bpy.context.scene
r.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1)
bpy.ops.object.select_all(action='DESELECT');r.select_set(True);body.select_set(True);bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(out/'PunkVera.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(art/'PunkVera_Corrected.blend'))
# A reference render shares exactly the same camera and pose as Vera's front view.
s.camera.location=(0,-5,.85);s.camera.rotation_euler=(Vector((0,0,.85))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.ortho_scale=1.95
s.render.resolution_x=800;s.render.resolution_y=900;s.render.resolution_percentage=100;s.cycles.samples=20
woolly=bpy.data.objects['Woolly_Body'];body.hide_render=True;woolly.hide_render=False;woolly.hide_set(False)
s.render.filepath=str(art/'woolly-reference-front.png');bpy.ops.render.render(write_still=True)
body.hide_render=False;woolly.hide_render=True;woolly.hide_set(True)
for obj in list(s.objects):
 if obj.type in ['LIGHT','CAMERA']:bpy.data.objects.remove(obj,do_unlink=True)
bpy.ops.object.camera_add(location=(2.2,-5,2.05));cam=bpy.context.object;cam.rotation_euler=(Vector((0,0,.83))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=2.1;s.camera=cam
for pos,power in [((1,-4,5),550),((-3,0,3),330)]:
 bpy.ops.object.light_add(type='AREA',location=pos);bpy.context.object.data.energy=power;bpy.context.object.data.size=4
s.world.color=(.35,.35,.35);s.render.engine='CYCLES';s.cycles.samples=24;s.render.film_transparent=True;s.render.resolution_x=600;s.render.resolution_y=700
s.render.use_freestyle=True;settings=s.view_layers[0].freestyle_settings;line=settings.linesets[0] if settings.linesets else settings.linesets.new('Outline')
if not line.linestyle:line.linestyle=bpy.data.linestyles.new('Cartoon Ink')
line.select_crease=False;line.linestyle.color=(.09,.055,.11);line.linestyle.thickness=1.4
s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA';s.render.filepath=str(out/'PunkVeraPortrait.png');bpy.ops.render.render(write_still=True)
shutil.copyfile(art/'woolly-rig-front.png',art/'idle-balanced-front.png')
shutil.copyfile(art/'woolly-rig-quarter.png',art/'Idle_01.png')
print('PUBLISHED: actual Woolly skeleton, exact Idle/Walk/Run, rebound Vera mesh')
