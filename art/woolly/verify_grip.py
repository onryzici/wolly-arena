import bpy,json,math
from pathlib import Path
p=Path('/Users/trexoinnovation/mobil-lamb-test/art/woolly');bpy.ops.wm.open_mainfile(filepath=str(p/'Woolly_Adventurer_GripRig_v9.blend'))
s=bpy.context.scene;r=bpy.data.objects['Woolly_Rig'];o=bpy.data.objects['Woolly_Body'];socket=bpy.data.objects['Revolver_HandSocket'];rel=[];poses=[]
for frame in [1,19,37,55,73]:
 s.frame_set(frame);bpy.context.view_layer.update();rel.append(r.pose.bones['mixamorig:RightHand'].matrix.inverted()@socket.matrix_world)
 ev=o.evaluated_get(bpy.context.evaluated_depsgraph_get());m=ev.to_mesh();poses.append([v.co.copy() for v in m.vertices]);assert all(math.isfinite(c) for v in m.vertices for c in v.co);ev.to_mesh_clear()
seam=max((a-b).length for a,b in zip(poses[0],poses[-1]));drift=max(abs(mat[i][j]-rel[0][i][j]) for mat in rel for i in range(4) for j in range(4))
r.animation_data.action=bpy.data.actions['Idle_Breathing_3s'];s.frame_set(1);opened=max(abs(r.pose.bones[n].rotation_quaternion.angle) for n in r.pose.bones.keys() if n.startswith('Grip_R_'))
report={'mesh_loop_seam':seam,'weapon_relative_to_hand_max_drift':drift,'unarmed_finger_rotation_max_radians':opened,'bone_count':len(r.data.bones),'mesh_vertices':len(o.data.vertices),'finite_mesh_samples':5}
(p/'grip_validation.json').write_text(json.dumps(report,indent=2));print(report)
