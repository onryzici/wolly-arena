"""Bind Vera's mesh to the actual in-game Woolly rig and its unchanged actions.

Offline only: Blender --background --python tools/bind_vera_to_woolly.py
The immutable input preserves Vera's mesh, texture and repaired tail in rest space.
"""
import json
from pathlib import Path
import bpy
import numpy as np
from mathutils import Vector, Matrix

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'art/woolly/punk_vera'
OUT = ROOT / 'WoollyArenaTest/Assets/Woolly/Resources/Characters'
bpy.ops.wm.open_mainfile(filepath=str(ART / 'PunkVera_BindingSource.blend'))
scene = bpy.context.scene
body = bpy.data.objects['PunkVera_Body']
old = bpy.data.objects['PunkVera_Rig']
old_rest = {b.name: (b.head_local.copy(), b.tail_local.copy(), b.length) for b in old.data.bones}
old.animation_data_clear()
for action in list(bpy.data.actions):
    bpy.data.actions.remove(action)
with bpy.data.libraries.load(str(ART.parent / 'Woolly_Unity_Unarmed.blend')) as (src, dst):
    dst.objects = ['Woolly_Rig', 'Woolly_Body']
    dst.actions = ['Idle', 'Walk', 'Run']
rig, woolly = dst.objects
for action in dst.actions:
    action.use_fake_user = True
for obj in dst.objects:
    scene.collection.objects.link(obj)
for track in list(rig.animation_data.nla_tracks):
    rig.animation_data.nla_tracks.remove(track)
rig.animation_data.action = bpy.data.actions['Idle']
scene.render.fps = 24
scene.frame_set(1)

# Map Vera's neutral mesh to Woolly's neutral bone segments. A swing aligns
# segment directions without importing the arbitrary roll of Vera's auto-rig.
maps = {}
for name, (head, tail, length) in old_rest.items():
    target = rig.data.bones.get(name)
    if target is None:
        continue
    direction = (tail-head).normalized()
    rotation = direction.rotation_difference((target.tail_local-target.head_local).normalized()).to_matrix()
    stretch = Matrix.Identity(3)
    for i in range(3):
        for j in range(3):
            stretch[i][j] += (target.length/length-1) * direction[i]*direction[j]
    matrix = (rotation @ stretch).to_4x4()
    matrix.translation = target.head_local - matrix.to_3x3() @ head
    maps[name] = matrix

def smooth(a, b, value):
    u = max(0, min(1, (value-a)/(b-a)))
    return u*u*(3-2*u)

# Boots are rigid footwear. Locate their real cuff geometry rather than the
# old toe/ankle bones, whose axes and pivots cut diagonally through the boots.
boot_maps = {}
for side, sign in [('Left', 1), ('Right', -1)]:
    def cuff(mesh):
        points = np.array([v.co[:] for v in mesh.vertices if v.co.x*sign > .04 and .21 < v.co.z < .25])
        assert len(points) > 20
        return Vector(tuple((np.quantile(points, .1, axis=0)+np.quantile(points, .9, axis=0))/2))
    source_cuff = cuff(body.data)
    target_cuff = cuff(woolly.data)
    matrix = Matrix.Identity(4)
    matrix.translation = target_cuff-source_cuff
    boot_maps[side] = matrix

original_positions = [v.co.copy() for v in body.data.vertices]
for vertex in body.data.vertices:
    position = vertex.co.copy()
    weights = [(body.vertex_groups[g.group].name, g.weight) for g in vertex.groups]
    mapped = Vector()
    total = 0
    for name, weight in weights:
        if name in maps:
            mapped += (maps[name] @ position) * weight
            total += weight
    assert total > .99, ('unmapped vertex', vertex.index, total)
    mapped /= total
    if position.z < .34:
        side = 'Left' if position.x > 0 else 'Right'
        blend = smooth(.24, .34, position.z)
        mapped = (boot_maps[side] @ position).lerp(mapped, blend)
        # Keep the whole shoe and collar rigid; only the exposed calf blends.
        foot_weight = 1-smooth(.245, .32, position.z)
        for group in list(vertex.groups):
            body.vertex_groups[group.group].remove([vertex.index])
        for part, weight in [('Foot', foot_weight), ('Leg', 1-foot_weight)]:
            if weight:
                body.vertex_groups['mixamorig:'+side+part].add([vertex.index], weight, 'REPLACE')
    vertex.co = mapped
body.data.update()
body.parent = rig
body.matrix_parent_inverse = Matrix.Identity(4)
for modifier in body.modifiers:
    if modifier.type == 'ARMATURE':
        modifier.object = rig
        modifier.use_deform_preserve_volume = False
bpy.data.objects.remove(old, do_unlink=True)
rig.name = 'PunkVera_Rig'
woolly.hide_render = True
woolly.hide_set(True)
# The copied skeleton and all its local animation channels remain untouched.
scene.frame_set(1)
bpy.context.view_layer.update()
scene.frame_start, scene.frame_end = 1, 73

# Match the soles to Woolly's planted idle without altering one bone/key.
# Correct the shoe's bind geometry, feathering only into the exposed calf.
for side, sign in [('Left', 1), ('Right', -1)]:
    foot = rig.pose.bones['mixamorig:'+side+'Foot']
    skin = foot.matrix @ foot.bone.matrix_local.inverted()
    indices = [i for i, v in enumerate(original_positions) if v.x*sign > 0 and v.z < .075]
    points = np.array([tuple(skin @ body.data.vertices[i].co) for i in indices])
    supports = []
    for x in np.linspace(points[:,0].min(), points[:,0].max(), 9)[:-1]:
        for y in np.linspace(points[:,1].min(), points[:,1].max(), 11)[:-1]:
            patch = points[(points[:,0]>=x) & (points[:,0]<x+np.ptp(points[:,0])/8) &
                           (points[:,1]>=y) & (points[:,1]<y+np.ptp(points[:,1])/10)]
            if len(patch):
                supports.append(patch[patch[:,2].argmin()])
    supports = np.array(supports)
    coeff = np.linalg.lstsq(np.c_[supports[:,:2], np.ones(len(supports))], supports[:,2], rcond=None)[0]
    normal = Vector((-float(coeff[0]), -float(coeff[1]), 1)).normalized()
    correction = normal.rotation_difference(Vector((0,0,1))).to_matrix().to_4x4()
    pivot = foot.head.copy()
    correction.translation = pivot-correction.to_3x3()@pivot
    bottom = min((correction@Vector(tuple(p))).z for p in points)
    correction.translation.z += .008-bottom
    bind_correction = skin.inverted() @ correction @ skin
    for i, original in enumerate(original_positions):
        if original.x*sign > 0 and original.z < .34:
            vertex = body.data.vertices[i]
            vertex.co = vertex.co.lerp(bind_correction@vertex.co, 1-smooth(.245,.34,original.z))
body.data.update()

def render(name, camera_position, center, scale=1.95):
    scene.camera.location = camera_position
    scene.camera.rotation_euler = (Vector(center)-scene.camera.location).to_track_quat('-Z', 'Y').to_euler()
    scene.camera.data.ortho_scale = scale
    scene.render.resolution_x, scene.render.resolution_y = 800, 900
    scene.render.resolution_percentage = 100
    scene.cycles.samples = 20
    scene.render.filepath = str(ART/name)
    bpy.ops.render.render(write_still=True)

bpy.ops.wm.save_as_mainfile(filepath=str(ART/'PunkVera_WoollyRig.blend'))
render('woolly-rig-front.png', (0,-5,.85), (0,0,.85))
render('woolly-rig-side.png', (5,0,.85), (0,0,.85))
render('woolly-rig-quarter.png', (2.7,-5,2.0), (0,0,.85), 2.1)
