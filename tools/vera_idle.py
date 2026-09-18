"""Retarget Woolly's authored idle onto Vera, with fixed sole contacts.

Loaded by art/woolly/punk_vera/build_vera.py in offline Blender.
Bone axes and proportions differ: copy directions, not local quaternions.
"""
import math
import bpy
from mathutils import Vector


def woolly_idle(rig, source_path, sole_fits, sole_vertices):
    scene = bpy.context.scene
    existing = set(bpy.data.objects)
    with bpy.data.libraries.load(str(source_path), link=False) as (src, dst):
        dst.objects = ['Woolly_Rig']
    reference = dst.objects[0]
    scene.collection.objects.link(reference)
    for track in reference.animation_data.nla_tracks:
        track.mute = True
    assert reference.animation_data.action.name.startswith('Idle_Breathing_3s')
    rig.animation_data.action = None
    prefix = 'mixamorig:'
    sides = ('Left', 'Right')
    rest = {b.name: b.matrix_local.copy() for b in rig.data.bones}

    def length(r, side):
        return sum(r.data.bones[prefix + side + part].length for part in ('UpLeg', 'Leg'))

    ratio = sum(length(rig, s) for s in sides) / sum(length(reference, s) for s in sides)
    scene.frame_set(1)
    ref_ankles = {s: reference.pose.bones[prefix + s + 'Foot'].head.copy() for s in sides}
    ref_center = sum(ref_ankles.values(), Vector()) / 2
    feet = {}
    for side in sides:
        name = prefix + side + 'Foot'
        rotation = sole_fits[side][0].inverted() @ rest[name].to_3x3()
        delta = rotation @ rest[name].to_3x3().inverted()
        bottom = min((delta @ (v - rest[name].translation)).z for v in sole_vertices[side])
        ankle = ref_ankles[side] * ratio
        ankle.z = .008 - bottom
        feet[side] = (ankle, rotation)
    target_center = sum((v[0] for v in feet.values()), Vector()) / 2

    def point_bone(bone, direction, start=None):
        base = rest[bone.name].to_3x3()
        swing = base.col[1].rotation_difference(direction.normalized())
        matrix = (swing.to_matrix() @ base).to_4x4()
        matrix.translation = bone.head if start is None else start
        bone.matrix = matrix
        bpy.context.view_layer.update()

    frames = []
    for frame in range(1, 74):
        scene.frame_set(frame)
        for bone in rig.pose.bones:
            bone.location = (0, 0, 0)
            bone.rotation_mode = 'QUATERNION'
            bone.rotation_quaternion = (1, 0, 0, 0)
            bone.scale = (1, 1, 1)
        bpy.context.view_layer.update()
        # Reconstruct in hierarchy order, preserving Vera's segment lengths.
        for part in ('Hips', 'Spine', 'Spine1', 'Spine2', 'Neck', 'Head',
                     'LeftShoulder', 'LeftArm', 'LeftForeArm', 'LeftHand',
                     'RightShoulder', 'RightArm', 'RightForeArm', 'RightHand'):
            src = reference.pose.bones[prefix + part]
            dst = rig.pose.bones[prefix + part]
            origin = target_center + (src.head - ref_center) * ratio if part == 'Hips' else None
            point_bone(dst, src.tail - src.head, origin)
        for side in sides:
            upper, lower, foot = [rig.pose.bones[prefix + side + part] for part in ('UpLeg', 'Leg', 'Foot')]
            ankle, rotation = feet[side]
            hip = upper.head.copy()
            axis = (ankle - hip).normalized()
            distance = (ankle - hip).length
            a, b = upper.bone.length, lower.bone.length
            assert abs(a-b) < distance < a+b, ('unreachable ankle', side, frame)
            along = (a*a - b*b + distance*distance) / (2*distance)
            src_upper = reference.pose.bones[prefix + side + 'UpLeg']
            pole = src_upper.tail - src_upper.head
            bend = (pole - axis * pole.dot(axis)).normalized()
            knee = hip + axis * along + bend * math.sqrt(max(0, a*a - along*along))
            point_bone(upper, knee - hip, hip)
            point_bone(lower, ankle - knee, knee)
            matrix = rotation.to_4x4()
            matrix.translation = ankle
            foot.matrix = matrix
            bpy.context.view_layer.update()
        frames.append({b.name: (b.location.copy(), b.rotation_quaternion.copy(), b.scale.copy())
                       for b in rig.pose.bones})
    frames[-1] = {n: tuple(v.copy() for v in values) for n, values in frames[0].items()}
    for obj in set(bpy.data.objects) - existing:
        bpy.data.objects.remove(obj, do_unlink=True)
    return frames
