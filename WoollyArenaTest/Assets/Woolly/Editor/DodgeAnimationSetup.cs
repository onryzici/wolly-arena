using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WoollyArena.Editor
{
    // Bakes a skeletal shoulder roll. The game plays a regular editable animation clip.
    public static class DodgeAnimationSetup
    {
        const string Root = "Assets/Woolly/";
        const string ClipPath = Root + "Animations/Dodge Roll.anim";
        const float Duration = .52f;

        public static void BuildAndReview()
        {
            Build();
            CapturePoses();
            LobbySetup.BuildAndCapture();
        }

        [MenuItem("Woolly/Build Dodge Animation")]
        public static void Build()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Woolly_Player.prefab");
            var model = Object.Instantiate(prefab.GetComponent<ArenaPlayer>().visual.gameObject);
            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            model.transform.localScale = Vector3.one;
            var animator = model.GetComponent<Animator>();
            animator.enabled = false;
            var idle = AssetDatabase.LoadAllAssetsAtPath(Root + "Art/Woolly.fbx").OfType<AnimationClip>().First(c => c.name == "Idle");
            var bones = model.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("mixamorig:") || t.name.StartsWith("Grip_")).ToArray();
            var byName = bones.ToDictionary(t => t.name);
            Transform Bone(string name) => byName["mixamorig:" + name];
            var hips = Bone("Hips");
            var body = model.GetComponentsInChildren<SkinnedMeshRenderer>().First(r => r.name == "Woolly_Body");
            var baked = new Mesh();
            var rotations = bones.ToDictionary(b => b, b => new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve(), new AnimationCurve() });
            var positions = bones.ToDictionary(b => b, b => new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve() });
            float lowest = float.PositiveInfinity;

            for (int frame = 0; frame <= 40; frame++)
            {
                float u = frame / 40f;
                idle.SampleAnimation(model, 0);
                float crouch = Smooth(0, .17f, u) * (1 - Smooth(.78f, 1, u));
                float tuck = Smooth(.16f, .32f, u) * (1 - Smooth(.68f, .91f, u));
                var leftFoot = Bone("LeftFoot").position;
                var rightFoot = Bone("RightFoot").position;
                var leftHand = Bone("LeftHand").position;
                var rightHand = Bone("RightHand").position;
                var leftFootRotation = Bone("LeftFoot").rotation;
                var rightFootRotation = Bone("RightFoot").rotation;
                hips.position -= Vector3.up * (.26f * crouch);
                Bend(Bone("Spine"), 32 * crouch + 24 * tuck);
                Bend(Bone("Spine1"), 14 * tuck);
                Bend(Bone("Head"), 22 * tuck);

                foreach (string side in new[] { "Left", "Right" })
                {
                    float sign = side == "Left" ? 1 : -1;
                    var foot = side == "Left" ? leftFoot : rightFoot;
                    var target = Vector3.Lerp(foot + new Vector3(0, .025f * crouch, -.035f * crouch), hips.position + new Vector3(sign * .17f, -.13f, -.19f), tuck);
                    Solve(Bone(side + "UpLeg"), Bone(side + "Leg"), Bone(side + "Foot"), target, hips.position + new Vector3(sign * .18f, -.05f, .8f));
                    Bone(side + "Foot").rotation = Quaternion.AngleAxis(-25 * tuck, Vector3.right) * (side == "Left" ? leftFootRotation : rightFootRotation);
                    var chest = Bone("Spine2").position;
                    var hand = side == "Left" ? leftHand : rightHand;
                    var handTarget = Vector3.Lerp(hand, chest + new Vector3(sign * .095f, -.06f, .15f), crouch);
                    Solve(Bone(side + "Arm"), Bone(side + "ForeArm"), Bone(side + "Hand"), handTarget, chest + new Vector3(sign * .55f, -.3f, .22f));
                }

                float spin = Smooth(.16f, .84f, u) * 360;
                var turn = Quaternion.AngleAxis(spin, new Vector3(1, 0, .22f).normalized);
                var center = hips.position + new Vector3(0, .18f, .13f);
                hips.position = center + turn * (hips.position - center);
                hips.rotation = turn * hips.rotation;

                // Keep the deformed body in contact with the floor throughout the roll.
                body.BakeMesh(baked);
                float bottom = baked.vertices.Min(v => body.transform.TransformPoint(v).y);
                hips.position += Vector3.up * ((.018f - bottom) * crouch);
                lowest = Mathf.Min(lowest, bottom + (.018f - bottom) * crouch);
                foreach (var bone in bones)
                {
                    var q = bone.localRotation;
                    for (int k = 0; k < 4; k++) rotations[bone][k].AddKey(u * Duration, q[k]);
                    var p = bone.localPosition;
                    for (int k = 0; k < 3; k++) positions[bone][k].AddKey(u * Duration, p[k]);
                }
            }

            var clip = new AnimationClip { name = "Dodge Roll", frameRate = 60 };
            foreach (var bone in bones)
            {
                string path = AnimationUtility.CalculateTransformPath(bone, model.transform);
                for (int k = 0; k < 4; k++) Bind(clip, path, "m_LocalRotation." + "xyzw"[k], rotations[bone][k]);
                for (int k = 0; k < 3; k++) Bind(clip, path, "m_LocalPosition." + "xyz"[k], positions[bone][k]);
            }
            clip.EnsureQuaternionContinuity();
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false; settings.keepOriginalPositionY = true; settings.keepOriginalPositionXZ = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            if (existing) { EditorUtility.CopySerialized(clip, existing); Object.DestroyImmediate(clip); clip = existing; }
            else AssetDatabase.CreateAsset(clip, ClipPath);
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Root + "Animations/Woolly.controller");
            var machine = controller.layers[0].stateMachine;
            var state = machine.states.Select(s => s.state).FirstOrDefault(s => s.name == "Dodge");
            if (!state) state = machine.AddState("Dodge", new Vector3(450, 100));
            state.motion = clip; state.speedParameter = "DodgePlayback"; state.speedParameterActive = true;
            if (!controller.parameters.Any(p => p.name == "DodgePlayback")) controller.AddParameter(new AnimatorControllerParameter { name = "DodgePlayback", type = AnimatorControllerParameterType.Float, defaultFloat = 1 });
            EditorUtility.SetDirty(controller); EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/dodge-animation-build.txt", $"Skeletal clip: {bones.Length} bones, 41 poses, {Duration:F2}s.\nMinimum body height: {lowest:F4} m.\n");
            Object.DestroyImmediate(baked); Object.DestroyImmediate(model);
            Debug.Log("DODGE_ANIMATION_BUILD_OK");
        }

        static float Smooth(float from, float to, float value) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(from, to, value));
        static void Bend(Transform bone, float degrees) { bone.rotation = Quaternion.AngleAxis(degrees, Vector3.right) * bone.rotation; }
        static void Solve(Transform upper, Transform lower, Transform end, Vector3 target, Vector3 pole)
        {
            var origin = upper.position;
            float a = Vector3.Distance(origin, lower.position), b = Vector3.Distance(lower.position, end.position);
            var delta = target - origin;
            float d = Mathf.Clamp(delta.magnitude, Mathf.Abs(a - b) + .001f, a + b - .001f);
            var axis = delta.normalized;
            var side = Vector3.ProjectOnPlane(pole - origin, axis).normalized;
            float along = (a * a - b * b + d * d) / (2 * d);
            var joint = origin + axis * along + side * Mathf.Sqrt(Mathf.Max(0, a * a - along * along));
            upper.rotation = Quaternion.FromToRotation(lower.position - origin, joint - origin) * upper.rotation;
            lower.rotation = Quaternion.FromToRotation(end.position - lower.position, origin + axis * d - lower.position) * lower.rotation;
        }
        static void Bind(AnimationClip clip, string path, string property, AnimationCurve curve)
        {
            var keys = curve.keys;
            if (keys.All(k => Mathf.Abs(k.value - keys[0].value) < .000001f))
                curve = new AnimationCurve(new Keyframe(0, keys[0].value), new Keyframe(Duration, keys[0].value));
            for (int i = 0; i < curve.length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
        }

        static void CapturePoses()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Woolly_Player.prefab").GetComponent<ArenaPlayer>();
            var model = Object.Instantiate(source.visual.gameObject);
            model.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            model.GetComponent<Animator>().enabled = false;
            var camera = new GameObject("Roll review camera").AddComponent<Camera>();
            camera.transform.position = new Vector3(2.7f, 2, 4);
            camera.transform.LookAt(new Vector3(0, .75f, 0));
            camera.orthographic = true; camera.orthographicSize = 1.05f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.2f, .27f, .36f);
            var light = new GameObject("Review light").AddComponent<Light>(); light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45, -35, 0); light.intensity = 1.3f;
            RenderSettings.ambientLight = Color.white * .7f;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/Path Sand.mat");
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            var target = new RenderTexture(640, 640, 24); camera.targetTexture = target;
            for (int i = 0; i <= 6; i++)
            {
                clip.SampleAnimation(model, clip.length * i / 6f);
                foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.forceMatrixRecalculationPerRender = true;
                camera.Render(); RenderTexture.active = target;
                var image = new Texture2D(640, 640, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 640, 640), 0, 0); image.Apply();
                File.WriteAllBytes("Logs/roll-pose-" + i + ".png", image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            camera.targetTexture = null; RenderTexture.active = null; Object.DestroyImmediate(target);
        }
    }
}
