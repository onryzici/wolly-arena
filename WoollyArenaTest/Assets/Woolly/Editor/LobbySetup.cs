using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using TMPro;
using System.Linq;
namespace WoollyArena.Editor {
public static class LobbySetup {
 const string Root="Assets/Woolly/";
 public static void Build(){
 string[] names={"Button01_l_Yellow","Button01_l_DarkGray","Button01_s_Blue","Menu_MiddleBtn_Bg","ItemIcon_Money_Coin","ItemIcon_Gem_Diamond_Purple","ItemIcon_Trophy_Gold","ItemIcon_Shop","ItemIcon_Friend","ItemIcon_Gear","ItemIcon_Battle","ItemIcon_Chest_Gold","ItemIcon_MemoPad","ItemIcon_Star_Blue"};
 var sprites=new Sprite[names.Length];for(int i=0;i<names.Length;i++){string path=Root+"UI/Lobby/"+names[i]+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.CompressedHQ;if(i<4)importer.spriteBorder=new Vector4(25,25,25,25);importer.SaveAndReimport();sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);}
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
 var camera=new GameObject("Lobby Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(0,1.35f,-7);camera.transform.rotation=Quaternion.identity;camera.orthographic=true;camera.orthographicSize=2.05f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.89f,.48f,.17f);camera.gameObject.AddComponent<AudioListener>();
 var sun=new GameObject("Key Light").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.91f,.78f);sun.transform.rotation=Quaternion.Euler(35,-35,0);RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.65f,.69f,.76f);
 var player=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Woolly_Player.prefab").GetComponent<ArenaPlayer>();var hero=Object.Instantiate(player.visual.gameObject);hero.name="Lobby Character";hero.transform.position=new Vector3(0,.35f,0);hero.transform.rotation=Quaternion.Euler(0,160,0);var animator=hero.GetComponentInChildren<Animator>();
 var bg=GameObject.CreatePrimitive(PrimitiveType.Quad);bg.name="Warm Lobby Backdrop";bg.transform.position=new Vector3(0,1.35f,3);bg.transform.localScale=new Vector3(24,12,1);Object.DestroyImmediate(bg.GetComponent<Collider>());var mat=new Material(Shader.Find("Woolly/LobbyBackdrop"));AssetDatabase.CreateAsset(mat,Root+"UI/Lobby/Backdrop.mat");bg.GetComponent<MeshRenderer>().sharedMaterial=mat;
 var shadow=GameObject.CreatePrimitive(PrimitiveType.Quad);shadow.name="Character Contact Shadow";shadow.transform.position=new Vector3(0,.08f,.3f);shadow.transform.localScale=new Vector3(1.65f,.28f,1);Object.DestroyImmediate(shadow.GetComponent<Collider>());shadow.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/Contact Shadow.mat");
 var lobby=new GameObject("Lobby",typeof(RectTransform)).AddComponent<LobbyScreen>();lobby.font=lobbyFont();lobby.art=sprites;lobby.hero=hero.transform;lobby.animator=animator;lobby.Construct();
 new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
 EditorSceneManager.SaveScene(scene,Root+"Scenes/Lobby.unity");
 var arena=EditorSceneManager.OpenScene(Root+"Scenes/TrainingArena.unity");var canvas=Object.FindObjectsByType<Canvas>().First(c=>c.renderMode!=RenderMode.WorldSpace);var old=GameObject.Find("ReturnToLobby");if(old)Object.DestroyImmediate(old);
 var go=new GameObject("ReturnToLobby",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button),typeof(ArenaLobbyReturn));go.transform.SetParent(canvas.transform,false);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-18);r.sizeDelta=new Vector2(125,48);var image=go.GetComponent<UnityEngine.UI.Image>();image.sprite=sprites[1];image.type=UnityEngine.UI.Image.Type.Sliced;var label=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(go.transform,false);var tr=(RectTransform)label.transform;tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;var t=label.GetComponent<TextMeshProUGUI>();t.font=lobbyFont();t.text="LOBİ";t.fontSize=22;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;UnityEditor.Events.UnityEventTools.AddPersistentListener(go.GetComponent<UnityEngine.UI.Button>().onClick,go.GetComponent<ArenaLobbyReturn>().Return);EditorSceneManager.SaveScene(arena);
 EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"Scenes/Lobby.unity",true),new EditorBuildSettingsScene(Root+"Scenes/TrainingArena.unity",true)};AssetDatabase.SaveAssets();
 System.IO.File.WriteAllText("Logs/lobby-validation.txt","Lobby and arena saved; two build scenes; 14 sprite references imported; editable Canvas, 3D character, input EventSystem and arena return button created.\n");
 }
 static TMP_FontAsset lobbyFont(){
 var path=Root+"UI/Lobby/Lobby Font.asset";var f=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(f)return f;
 f=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Root+"UI/Fonts/LilitaOne-Regular.ttf"),60,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,2048,2048);f.name="Lobby Font";
 f.TryAddCharacters(new string(Enumerable.Range(32,224).Select(x=>(char)x).ToArray())+"ĞğİıŞş•≡",out string missing);
 var fallback=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf"),60,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024);fallback.name="Turkish Fallback";fallback.TryAddCharacters("ĞğİıŞş•≡",out string absent);fallback.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(fallback,Root+"UI/Lobby/Turkish Fallback.asset");AssetDatabase.AddObjectToAsset(fallback.material,fallback);foreach(var tex in fallback.atlasTextures)AssetDatabase.AddObjectToAsset(tex,fallback);
 f.fallbackFontAssetTable=new System.Collections.Generic.List<TMP_FontAsset>{fallback};f.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(f,path);AssetDatabase.AddObjectToAsset(f.material,f);foreach(var tex in f.atlasTextures)AssetDatabase.AddObjectToAsset(tex,f);return f;
 }
}
}
