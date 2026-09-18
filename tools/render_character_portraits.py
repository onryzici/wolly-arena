"""Offline matching portraits: approved Idle + Woolly's exact revolver socket."""
import bpy,json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];ART=ROOT/'art/woolly/punk_vera';OUT=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Characters'
bpy.ops.wm.open_mainfile(filepath=str(ART/'PunkVera_WoollyRig.blend'))
s=bpy.context.scene;rig=bpy.data.objects['PunkVera_Rig'];rig.animation_data.action=bpy.data.actions['Idle'];s.frame_set(1)
with bpy.data.libraries.load(str(ART.parent/'Woolly_Unity_Unarmed.blend'))as(src,dst):dst.objects=['Revolver_HandSocket','Revolver_GameMesh','Muzzle']
for obj in dst.objects:s.collection.objects.link(obj);obj.hide_render=False;obj.hide_set(False)
socket,gun,muzzle=dst.objects;socket.parent=rig
# Both source skeletons are identical, so no pose, wrist or socket offset is changed.
for obj in s.objects:
 if obj.type=='MESH' and obj.name not in ['Woolly_Body','PunkVera_Body',gun.name]:obj.hide_render=True
s.camera.location=(1.75,-5,1.65);s.camera.rotation_euler=(Vector((0,0,.85))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.camera.data.ortho_scale=2.04
s.render.resolution_x=640;s.render.resolution_y=800;s.render.resolution_percentage=100;s.render.film_transparent=True;s.render.engine='CYCLES';s.cycles.samples=32
s.render.image_settings.file_format='PNG';s.render.image_settings.color_mode='RGBA';s.render.use_freestyle=True
settings=s.view_layers[0].freestyle_settings;line=settings.linesets[0]if settings.linesets else settings.linesets.new('Matching ink')
if not line.linestyle:line.linestyle=bpy.data.linestyles.new('Warm character ink')
line.select_crease=False;line.linestyle.color=(.043,.014,.009);line.linestyle.thickness=1.8
for name,bodyName in [('WoollyPortrait.png','Woolly_Body'),('PunkVeraLobbyPortrait.png','PunkVera_Body')]:
 for n in ['Woolly_Body','PunkVera_Body']:
  body=bpy.data.objects[n];body.hide_render=n!=bodyName;body.hide_set(n!=bodyName)
 s.render.filepath=str(OUT/name);bpy.ops.render.render(write_still=True)
print('Matching armed portraits rendered with the original socket and unchanged Idle.')
