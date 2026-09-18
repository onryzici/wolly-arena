using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WoollyArena.Editor
{
    public static class PowerVFXReview
    {
        [MenuItem("Woolly/Capture Cartoon Powers")]
        public static void Capture()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before capturing the VFX sheet.");
            var scene = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D image = null;
            var previous = RenderTexture.active;
            try
            {
                var root = new GameObject("VFX presentation"); SceneManager.MoveGameObjectToScene(root, scene);
                var cameraObject = new GameObject("Preview camera", typeof(Camera)); SceneManager.MoveGameObjectToScene(cameraObject, scene);
                var camera = cameraObject.GetComponent<Camera>(); camera.scene = scene;
                camera.transform.position = new Vector3(0, 16, -23); camera.transform.LookAt(new Vector3(0, .7f, 0));
                camera.orthographic = true; camera.orthographicSize = 7.7f; camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.055f, .057f, .078f); camera.useOcclusionCulling = false;
                camera.nearClipPlane = .1f; camera.farClipPlane = 100;
                var effects = root.AddComponent<CartoonPowerVFX>();
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                typeof(CartoonPowerVFX).GetMethod("Awake", flags).Invoke(effects, null);
                typeof(CartoonPowerVFX).GetField("view", flags).SetValue(effects, camera);
                var pool = (Array)typeof(CartoonPowerVFX).GetField("pool", flags).GetValue(effects);
                var draw = typeof(CartoonPowerVFX).GetMethod("Draw", flags);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                string[] names = { "GALAXY", "ENERGY", "STAR" };
                for (int row = 0; row < 3; row++)
                    for (int kind = 0; kind < 3; kind++)
                    {
                        var point = new Vector3((kind - 1) * 7, 0, (1 - row) * 6);
                        effects.Play(kind, point + new Vector3(-2, 4, 0), point, 1.35f);
                        draw.Invoke(effects, new[] { pool.GetValue(row * 3 + kind), (object)(row == 0 ? (kind == 1 ? .06f : .20f) : row == 1 ? .43f : .87f) });
                        if (row != 0) continue;
                        var labelObject = new GameObject(names[kind]); SceneManager.MoveGameObjectToScene(labelObject, scene);
                        var label = labelObject.AddComponent<TextMeshPro>(); label.font = font; label.text = names[kind]; label.fontSize = 5;
                        label.alignment = TextAlignmentOptions.Center; label.color = Color.Lerp(CartoonPowerVFX.ColorFor(kind), Color.white, .6f);
                        label.rectTransform.sizeDelta = new Vector2(5, 1);
                        label.transform.SetPositionAndRotation(point + new Vector3(0, 4.1f, 1.5f), camera.transform.rotation);
                        label.ForceMeshUpdate();
                    }
                target = new RenderTexture(1920, 1080, 24) { antiAliasing = 4 }; camera.targetTexture = target;
                bool asyncCompilation = ShaderUtil.allowAsyncCompilation;
                ShaderUtil.allowAsyncCompilation = false;
                try { camera.Render(); camera.Render(); }
                finally { ShaderUtil.allowAsyncCompilation = asyncCompilation; }
                RenderTexture.active = target; image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); image.Apply();
                Directory.CreateDirectory("Logs"); File.WriteAllBytes("Logs/cartoon-power-vfx.png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                if (image) Object.DestroyImmediate(image);
                EditorSceneManager.ClosePreviewScene(scene);
                if (target) Object.DestroyImmediate(target);
            }
        }
    }
}
