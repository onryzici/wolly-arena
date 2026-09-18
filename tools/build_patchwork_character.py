"""Offline Blender build: preserve Patchwork's artwork, rebind to Woolly's corrected rig."""
from pathlib import Path
import bpy, math, json, runpy
import numpy as np
from mathutils import Vector, Matrix
ROOT=Path(__file__).resolve().parents[1]; ART=ROOT/'art/woolly/patchwork'; OUT=ROOT/'WoollyArenaTest/Assets/Woolly/Resources/Characters'
bpy.ops.wm.open_mainfile(filepath=str(ART/'Patchwork_Source.blend'))
s=bpy.context.scene;old=next(o for o in s.objects if o.type=='ARMATURE');body=next(o for o in s.objects if o.type=='MESH');old.animation_data_clear()
rest={b.name:(b.head_local.copy(),b.tail_local.copy())for b in old.data.bones}
for action in list(bpy.data.actions):bpy.data.actions.remove(action)
with bpy.data.libraries.load(str(ART.parent/'Woolly_Unity_Unarmed.blend')) as (src,dst):
 dst.objects=['Woolly_Rig'];dst.actions=['Idle','Walk','Run']
r=dst.objects[0];s.collection.objects.link(r);r.name='Patchwork_Rig';r.data.pose_position='POSE'
for a in dst.actions:a.use_fake_user=True
for t in list(r.animation_data.nla_tracks):r.animation_data.nla_tracks.remove(t)
r.animation_data.action=bpy.data.actions['Idle'];s.render.fps=24;s.frame_set(1);bpy.context.view_layer.update()
def segment(a,b,c,d):
 axis=(b-a).normalized();rot=axis.rotation_difference((d-c).normalized()).to_matrix();stretch=Matrix.Identity(3)
 for i in range(3):
  for j in range(3):stretch[i][j]+=((d-c).length/(b-a).length-1)*axis[i]*axis[j]
 m=(rot@stretch).to_4x4();m.translation=c-m.to_3x3()@a;return m
def smooth(a,b,x):
 u=max(0,min(1,(x-a)/(b-a)));return u*u*(3-2*u)
maps={}
for name,(a,b)in rest.items():
 target=r.data.bones.get(name)
 if target:maps[name]=segment(a,b,target.head_local,target.tail_local)
# Meshy's tiny reversed spine and horizontal HeadTop markers are not anatomical axes.
# Map the torso as a single hips-to-neck segment, preserving its coat silhouette.
torso=segment(rest['mixamorig:Hips'][0],rest['mixamorig:Neck'][0],r.data.bones['mixamorig:Hips'].head_local,r.data.bones['mixamorig:Neck'].head_local)
for part in ['Hips','Spine','Spine1','Spine2']:maps['mixamorig:'+part]=torso
# Preserve head volume. Correct the lowered face in bind geometry rather than rotating
# the already-correct animation skeleton (which would affect every animation/socket).
head=Matrix.Rotation(math.radians(-8),4,'X');pivot=rest['mixamorig:Head'][0];head.translation=r.data.bones['mixamorig:Head'].head_local-head.to_3x3()@pivot
for part in ['mixamorig:Head','mixamorig:HeadTop_End','headfront']:maps[part]=head
maps['mixamorig:Neck']=head
original=[v.co.copy()for v in body.data.vertices]
# Footwear remains rigid, aligned by ankles without distorting the sole.
boots={}
for side in ['Left','Right']:
 m=Matrix.Identity(4);m.translation=r.data.bones['mixamorig:'+side+'Foot'].head_local-rest['mixamorig:'+side+'Foot'][0];boots[side]=m
for v in body.data.vertices:
 weights=[(body.vertex_groups[g.group].name,g.weight)for g in v.groups];total=sum(w for n,w in weights if n in maps);assert total>.99,(v.index,total)
 pos=sum(((maps[n]@v.co)*w for n,w in weights if n in maps),Vector())/total
 if v.co.z<.28:
  side='Left'if v.co.x>0 else'Right';pos=(boots[side]@v.co).lerp(pos,smooth(.19,.28,v.co.z))
  foot=1-smooth(.20,.28,v.co.z)
  for index in [g.group for g in v.groups]:body.vertex_groups[index].remove([v.index])
  for part,w in [('Foot',foot),('Leg',1-foot)]:
   if w:body.vertex_groups['mixamorig:'+side+part].add([v.index],w,'REPLACE')
 v.co=pos
# Redistribute torso weights by the corrected spine's anatomical heights. Keeping
# weights from Meshy's reversed/tiny spine segments would stretch the coat in Run.
spine_names=['mixamorig:'+n for n in ['Hips','Spine','Spine1','Spine2']]
spine_heights=[r.data.bones[n].head_local.z for n in spine_names]
for v in body.data.vertices:
 weights=[(body.vertex_groups[g.group].name,g.weight)for g in v.groups]
 transfer=[(n,w)for n,w in weights if n in spine_names or ('UpLeg' in n and original[v.index].z>.65)]
 total=sum(w for _,w in transfer)
 if not total:continue
 for name,_ in transfer:body.vertex_groups[name].remove([v.index])
 z=v.co.z
 if z<=spine_heights[0]:body.vertex_groups[spine_names[0]].add([v.index],total,'REPLACE')
 elif z>=spine_heights[-1]:body.vertex_groups[spine_names[-1]].add([v.index],total,'REPLACE')
 else:
  for index in range(3):
   if spine_heights[index]<=z<spine_heights[index+1]:
    blend=smooth(spine_heights[index],spine_heights[index+1],z)
    body.vertex_groups[spine_names[index]].add([v.index],total*(1-blend),'REPLACE')
    body.vertex_groups[spine_names[index+1]].add([v.index],total*blend,'REPLACE');break
# Extra Meshy head markers carry the same rigid head motion after transfer.
for marker in ['headfront','mixamorig:HeadTop_End']:
 group=body.vertex_groups.get(marker)
 if group:
  for v in body.data.vertices:
   amount=sum(g.weight for g in v.groups if g.group==group.index)
   if amount:body.vertex_groups['mixamorig:Head'].add([v.index],amount,'ADD')
  body.vertex_groups.remove(group)
# Normalize the source's quantized skin weights after merging extra head markers.
for v in body.data.vertices:
 weights=[(g.group,g.weight)for g in v.groups if g.weight>0];total=sum(w for _,w in weights);assert total>0
 for index,weight in weights:body.vertex_groups[index].add([v.index],weight/total,'REPLACE')
body.name='Patchwork_Body';body.parent=r;body.matrix_parent_inverse=Matrix.Identity(4)
for mod in body.modifiers:
 if mod.type=='ARMATURE':mod.object=r;mod.use_deform_preserve_volume=False
bpy.data.objects.remove(old,do_unlink=True);body.data.update();bpy.context.view_layer.update()
# Plant both sole bottoms on the same floor in idle, without editing animation keys.
for side,sign in [('Left',1),('Right',-1)]:
 foot=r.pose.bones['mixamorig:'+side+'Foot'];skin=foot.matrix@foot.bone.matrix_local.inverted()
 indices=[i for i,p in enumerate(original)if p.x*sign>0 and p.z<.06]
 pts=np.array([tuple(skin@body.data.vertices[i].co)for i in indices]);supports=[]
 for x in np.linspace(pts[:,0].min(),pts[:,0].max(),7)[:-1]:
  for y in np.linspace(pts[:,1].min(),pts[:,1].max(),9)[:-1]:
   patch=pts[(pts[:,0]>=x)&(pts[:,0]<x+np.ptp(pts[:,0])/6)&(pts[:,1]>=y)&(pts[:,1]<y+np.ptp(pts[:,1])/8)]
   if len(patch):supports.append(patch[patch[:,2].argmin()])
 supports=np.array(supports);coef=np.linalg.lstsq(np.c_[supports[:,:2],np.ones(len(supports))],supports[:,2],rcond=None)[0]
 correction=Vector((-float(coef[0]),-float(coef[1]),1)).normalized().rotation_difference(Vector((0,0,1))).to_matrix().to_4x4();correction.translation=foot.head-correction.to_3x3()@foot.head
 correction.translation.z+=.008-min((correction@Vector(tuple(p))).z for p in pts);bind=skin.inverted()@correction@skin
 for i,p in enumerate(original):
  if p.x*sign>0 and p.z<.28:body.data.vertices[i].co=body.data.vertices[i].co.lerp(bind@body.data.vertices[i].co,1-smooth(.20,.28,p.z))
body.data.update()
# Extract the supplied base color unchanged.
mat=body.data.materials[0];bsdf=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=bsdf.inputs['Base Color'].links[0].from_node.image
tex.filepath_raw=str(OUT/'Patchwork_BaseColor.png');tex.file_format='PNG';tex.save();bsdf.inputs['Roughness'].default_value=.85
# Keep source polygon count: no decimation that could damage stitches, ears or skin weights.
s.frame_set(1);s.frame_start=1;s.frame_end=73
grip=runpy.run_path(str(ROOT/'tools/fit_patchwork_grip.py'));gripMesh=grip['fit_grip'](r,body)
bpy.context.view_layer.update()
knife=runpy.run_path(str(ROOT/'tools/build_patchwork_knife.py'))['build_knife'](r)
bpy.ops.object.select_all(action='DESELECT');r.select_set(True);body.select_set(True);knife.select_set(True);gripMesh.select_set(True);bpy.context.view_layer.objects.active=r
bpy.ops.export_scene.fbx(filepath=str(OUT/'Patchwork.fbx'),use_selection=True,object_types={'ARMATURE','MESH'},add_leaf_bones=False,axis_forward='-Z',axis_up='Y',bake_anim=True,bake_anim_use_all_actions=True,bake_anim_use_nla_strips=False,bake_anim_simplify_factor=0,path_mode='RELATIVE',apply_scale_options='FBX_SCALE_UNITS')
bpy.ops.wm.save_as_mainfile(filepath=str(ART/'Patchwork_WoollyRig.blend'))
s.render.resolution_x=650;s.render.resolution_y=800;s.render.resolution_percentage=100;s.cycles.samples=16;s.render.film_transparent=True;s.render.image_settings.color_mode='RGBA';s.camera.data.ortho_scale=1.95
for name,position in [('corrected-front',(0,-5,.84)),('corrected-side',(5,0,.84)),('corrected-quarter',(2,-5,1.65))]:
 s.camera.location=position;s.camera.rotation_euler=(Vector((0,0,.84))-s.camera.location).to_track_quat('-Z','Y').to_euler();s.render.filepath=str(ART/(name+'.png'));bpy.ops.render.render(write_still=True)
s.render.filepath=str(OUT/'PatchworkPortrait.png');bpy.ops.render.render(write_still=True)
print('PATCHWORK EXPORTED',len(body.data.vertices),'vertices;',len(r.data.bones),'bones; Idle/Walk/Run')
