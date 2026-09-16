using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WoollyArena.Editor
{
    public static class LobbySetup
    {
        const string Root = "Assets/Woolly/";
        const string Art = Root + "UI/Lobby/";
        const string LobbyPath = Root + "Scenes/Lobby.unity";
        static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            { "Button_Tapered_Yellow", new Vector4(90, 90, 90, 90) },
            { "Button_Round03_Dark", new Vector4(23, 25, 23, 23) },
            { "BorderFrame_Round20_Single_Dark", new Vector4(20, 21, 20, 20) },
            { "Menu_BottomBtn_TabFocus", new Vector4(25, 0, 25, 25) },
            { "Slider_Basic02_Fill_Blue", new Vector4(0, 3, 0, 3) },
            { "Button01_s_Yellow", new Vector4(20, 20, 20, 20) },
            { "Button01_s_Purple", new Vector4(20, 20, 20, 20) }
        };

        [MenuItem("Woolly/Rebuild Lobby")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before rebuilding the lobby.");
            Directory.CreateDirectory("Logs");
            // The arena and its prefabs are deliberately never opened or rebuilt here.
            string arenaPath = Root + "Scenes/TrainingArena.unity";
            byte[] originalArena = File.ReadAllBytes(arenaPath);
            ImportArt();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Lobby Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 1.35f, -7);
            camera.orthographic = true;
            camera.orthographicSize = 2.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(73, 87, 247, 255);
            camera.gameObject.AddComponent<AudioListener>();

            var sun = new GameObject("Key Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            sun.color = new Color(1, .95f, .87f);
            sun.transform.rotation = Quaternion.Euler(35, -35, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.7f, .74f, .82f);

            var player = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Woolly_Player.prefab").GetComponent<ArenaPlayer>();
            var hero = Object.Instantiate(player.visual.gameObject);
            hero.name = "Lobby Character";
            hero.transform.position = new Vector3(0, .35f, 0);
            hero.transform.rotation = Quaternion.Euler(0, 160, 0);
            var animator = hero.GetComponentInChildren<Animator>();

            var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backdrop.name = "Panoramic Canyon Landscape";
            backdrop.transform.position = new Vector3(0, 1.35f, 4);
            backdrop.transform.localScale = new Vector3(40, 24, 1);
            Object.DestroyImmediate(backdrop.GetComponent<Collider>());
            var material = AssetDatabase.LoadAssetAtPath<Material>(Art + "Backdrop.mat");
            if (!material) { material = new Material(Shader.Find("Woolly/LobbyBackdrop")); AssetDatabase.CreateAsset(material, Art + "Backdrop.mat"); }
            material.shader = Shader.Find("Woolly/LobbyBackdrop");
            string backdropPath = Art + "Canyon Lobby.png";
            AssetDatabase.ImportAsset(backdropPath);
            var backdropImporter = (TextureImporter)AssetImporter.GetAtPath(backdropPath);
            backdropImporter.textureType = TextureImporterType.Default;
            backdropImporter.npotScale = TextureImporterNPOTScale.None;
            backdropImporter.maxTextureSize = 2048;
            backdropImporter.mipmapEnabled = false;
            backdropImporter.wrapMode = TextureWrapMode.Clamp;
            backdropImporter.textureCompression = TextureImporterCompression.Uncompressed;
            backdropImporter.SaveAndReimport();
            material.SetTexture("_Backdrop", AssetDatabase.LoadAssetAtPath<Texture2D>(backdropPath));
            EditorUtility.SetDirty(material);
            backdrop.GetComponent<MeshRenderer>().sharedMaterial = material;

            var shadow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            shadow.name = "Character Contact Shadow";
            Object.DestroyImmediate(shadow.GetComponent<Collider>());
            shadow.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/Contact Shadow.mat");

            var lobby = new GameObject("Lobby", typeof(RectTransform)).AddComponent<LobbyScreen>();
            lobby.font = LobbyFont();
            lobby.outlinedFontMaterial = OutlineMaterial(lobby.font);
            lobby.art = AssetDatabase.FindAssets("t:Sprite", new[] { Art.TrimEnd('/') })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct()
                .Select(AssetDatabase.LoadAssetAtPath<Sprite>).Where(s => s).ToArray();
            lobby.hero = hero.transform;
            lobby.animator = animator;
            lobby.presentationCamera = camera;
            lobby.contactShadow = shadow.transform;
            lobby.backdropRenderer = backdrop.GetComponent<MeshRenderer>();
            lobby.heroLocalBounds = MeasureHero(hero.transform, animator);
            lobby.Construct();
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Canvas.ForceUpdateCanvases();
            lobby.ApplyLayout();
            EditorSceneManager.SaveScene(scene, LobbyPath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(LobbyPath, true), new EditorBuildSettingsScene(arenaPath, true) };
            AssetDatabase.SaveAssets();

            Check(originalArena.SequenceEqual(File.ReadAllBytes(arenaPath)), "Arena scene was preserved");
            Check(lobby.art.All(s => s), "All UI sprites resolve");
            Check(lobby.font.HasCharacters("MAĞAZA ARKADAŞLAR GÖREVLER LOBİ SAVAŞ YÜKLENİYOR ÇAYLAK ÖDÜL", out uint[] missing, true), "Turkish font glyphs");
            Check(Object.FindObjectsByType<EventSystem>().Length == 1, "Exactly one EventSystem");
            Check(lobby.content.Find("Play").GetComponent<Button>() != null, "Play button exists");
            File.WriteAllText("Logs/lobby-validation.txt", "Lobby rebuilt from GUI Pro Super Casual references.\nArena scene preserved byte-for-byte.\nUI sprites, Turkish glyphs, EventSystem and Play button validated.\n");
            Debug.Log("LOBBY_BUILD_OK");
        }

        static void ImportArt()
        {
            foreach (string file in Directory.GetFiles(Art + "SuperCasual", "*.png"))
            {
                string path = file.Replace('\\', '/');
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                string name = Path.GetFileNameWithoutExtension(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = name == "ArenaPreview" ? 1024 : 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteBorder = Borders.TryGetValue(name, out var border) ? border : Vector4.zero;
                importer.GetSourceTextureWidthAndHeight(out int width, out int height);
                if (importer.spriteBorder.x + importer.spriteBorder.z >= width || importer.spriteBorder.y + importer.spriteBorder.w >= height)
                    throw new InvalidOperationException("Invalid sprite slicing: " + name);
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
            }
        }

        static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        static Bounds MeasureHero(Transform hero, Animator animator)
        {
            animator.Rebind();
            animator.Update(0);
            Bounds bounds = default;
            bool first = true;
            foreach (var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh = new Mesh();
                renderer.BakeMesh(mesh);
                foreach (var vertex in mesh.vertices)
                {
                    var point = hero.InverseTransformPoint(renderer.transform.TransformPoint(vertex));
                    if (first) { bounds = new Bounds(point, Vector3.zero); first = false; }
                    else bounds.Encapsulate(point);
                }
                Object.DestroyImmediate(mesh);
            }
            return bounds;
        }

        static TMP_FontAsset LobbyFont()
        {
            const string path = Root + "UI/Fonts/Sen/Sen ExtraBold SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font) return font;
            font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Root + "UI/Fonts/Sen/Sen-ExtraBold.ttf"), 64, 10, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
            font.name = "Sen ExtraBold SDF";
            string characters = new string(Enumerable.Range(32, 224).Select(c => (char)c).ToArray()) + "ĞğİıŞş•∞";
            font.TryAddCharacters(characters, out string missing);
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(font, path);
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
            return font;
        }

        static Material OutlineMaterial(TMP_FontAsset font)
        {
            const string path = Root + "UI/Fonts/Sen/Sen Outline.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) { material = new Material(font.material); AssetDatabase.CreateAsset(material, path); }
            material.SetColor(ShaderUtilities.ID_OutlineColor, new Color32(8, 13, 29, 255));
            material.SetFloat(ShaderUtilities.ID_FaceDilate, .22f);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, .20f);
            material.EnableKeyword("OUTLINE_ON");
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void BuildAndCapture()
        {
            Build();
            Capture(1600, 900);
            Capture(2400, 1080);
            Capture(1440, 1080);
            EditorSceneManager.OpenScene(LobbyPath);
        }

        static void Capture(int width, int height)
        {
            var lobby = Object.FindAnyObjectByType<LobbyScreen>();
            var camera = lobby.presentationCamera;
            var canvas = lobby.GetComponent<Canvas>();
            var scaler = lobby.GetComponent<CanvasScaler>();
            scaler.enabled = false;
            var target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
            canvas.scaleFactor = height / 900f;
            Canvas.ForceUpdateCanvases();
            lobby.ApplyLayout();
            Canvas.ForceUpdateCanvases();
            lobby.ApplyLayout();
            Canvas.ForceUpdateCanvases();
            Debug.Log("CAPTURE " + width + "x" + height + " content=" + lobby.content.localScale + " hero=" + lobby.hero.localScale + " frame=" + lobby.heroFrame.rect + " camera=" + camera.pixelRect);
            foreach (var text in lobby.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate();
            foreach (var renderer in lobby.hero.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.forceMatrixRecalculationPerRender = true;
            lobby.animator.Update(0);
            camera.Render();
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            image.Apply();
            File.WriteAllBytes("Logs/lobby-" + width + "x" + height + ".png", image.EncodeToPNG());
            var corners = new Vector3[4];
            foreach (var button in lobby.content.GetComponentsInChildren<Button>())
            {
                ((RectTransform)button.transform).GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(camera, corner);
                    Check(screen.x >= -3 && screen.y >= -3 && screen.x <= width + 3 && screen.y <= height + 3, "Button outside viewport: " + button.name);
                }
            }
            File.AppendAllText("Logs/lobby-validation.txt", width + "x" + height + ": all buttons fit viewport.\n");
            camera.targetTexture = null;
            RenderTexture.active = previous;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(target);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
            scaler.enabled = true;
        }
    }
}
