using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using System.Linq;
namespace WoollyArena.Editor {
public static class MobileSetup {
 const string Root="Assets/Woolly/";
 static RectTransform Rect(string name,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size){var g=new GameObject(name,typeof(RectTransform));var r=(RectTransform)g.transform;r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;return r;}
 static Sprite Disc(){string path=Root+"Art/ControlDisc.png";if(!System.IO.File.Exists(path)){var t=new Texture2D(128,128,TextureFormat.RGBA32,false);for(int y=0;y<128;y++)for(int x=0;x<128;x++){float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),Vector2.one*64);t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(63-d)));}t.Apply();System.IO.File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.alphaIsTransparency=true;i.mipmapEnabled=false;i.SaveAndReimport();}return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
 static void Label(Transform parent,string text,Vector2 pos){var r=Rect(text,parent,new Vector2(.5f,.5f),pos,new Vector2(220,40));var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");t.text=text;t.fontSize=20;t.alignment=TextAlignmentOptions.Center;t.raycastTarget=false;}
 static TouchStick Stick(Transform parent,string name,Vector2 anchor,Vector2 pos,Color color){var r=Rect(name,parent,anchor,pos,Vector2.one*210);var im=r.gameObject.AddComponent<UnityEngine.UI.Image>();im.sprite=Disc();im.color=new Color(color.r,color.g,color.b,.24f);var stick=r.gameObject.AddComponent<TouchStick>();var k=Rect("Thumb",r,Vector2.one*.5f,Vector2.zero,Vector2.one*94);var ki=k.gameObject.AddComponent<UnityEngine.UI.Image>();ki.sprite=im.sprite;ki.color=color;ki.raycastTarget=false;stick.knob=k;Label(r,name,new Vector2(0,-128));return stick;}
 public static string Configure(){
 if(EditorApplication.isPlaying)throw new System.Exception("Stop Play first");
 var player=Object.FindFirstObjectByType<ArenaPlayer>();var canvas=Object.FindFirstObjectByType<Canvas>();if(!canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>())canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
 if(!Object.FindFirstObjectByType<EventSystem>()){var es=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();}
 var safe=canvas.transform.Find("SafeArea") as RectTransform;if(!safe){safe=Rect("SafeArea",canvas.transform,Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaPanel>();var children=canvas.transform.Cast<Transform>().Where(t=>t!=safe).ToArray();foreach(var t in children)t.SetParent(safe,false);}
 var controls=Object.FindFirstObjectByType<MobileControls>();if(!controls){var root=Rect("TouchControls",safe,Vector2.zero,Vector2.zero,Vector2.zero);root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;controls=root.gameObject.AddComponent<MobileControls>();controls.move=Stick(root,"MOVE",Vector2.zero,new Vector2(160,180),new Color(.25f,.65f,1,.8f));controls.aim=Stick(root,"AIM / FIRE",Vector2.right,new Vector2(-160,180),new Color(1,.38f,.25f,.85f));var rr=Rect("Reload",root,Vector2.right,new Vector2(-350,105),Vector2.one*92);var im=rr.gameObject.AddComponent<UnityEngine.UI.Image>();im.sprite=Disc();im.color=new Color(.12f,.2f,.28f,.85f);var button=rr.gameObject.AddComponent<UnityEngine.UI.Button>();Label(rr,"RELOAD",Vector2.zero);UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick,controls.Reload);}
 controls.player=player;player.mobile=controls;
 foreach(var t in canvas.GetComponentsInChildren<TMP_Text>())if(t.text.StartsWith("WASD")){t.text="LEFT STICK  MOVE / RUN     RIGHT STICK  AIM / FIRE";t.fontSize=15;t.rectTransform.anchoredPosition=new Vector2(28,12);}
 var hud=canvas.GetComponent<ArenaHUD>();hud.ammo.rectTransform.anchorMin=hud.ammo.rectTransform.anchorMax=new Vector2(1,1);hud.ammo.rectTransform.pivot=Vector2.one;hud.ammo.rectTransform.anchoredPosition=new Vector2(-28,-108);
 var sun=GameObject.Find("Sun").GetComponent<Light>();sun.color=new Color(1,.93f,.82f);sun.intensity=1.45f;sun.transform.rotation=Quaternion.Euler(48,-32,0);sun.shadows=LightShadows.Soft;sun.shadowStrength=.72f;sun.shadowBias=.035f;sun.shadowNormalBias=.22f;
 RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.57f,.66f,.78f);RenderSettings.ambientEquatorColor=new Color(.39f,.46f,.52f);RenderSettings.ambientGroundColor=new Color(.25f,.27f,.3f);
 var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/Mobile_RPAsset.asset");rp.supportsHDR=true;rp.msaaSampleCount=2;rp.renderScale=1;rp.shadowDistance=30;rp.shadowCascadeCount=2;rp.mainLightShadowmapResolution=2048;var so=new SerializedObject(rp);so.FindProperty("m_SoftShadowsSupported").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();GraphicsSettings.defaultRenderPipeline=rp;QualitySettings.renderPipeline=rp;EditorUtility.SetDirty(rp);
 var cam=Camera.main;var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.volumeLayerMask=1;cam.allowHDR=true;
 var volume=Object.FindFirstObjectByType<Volume>();if(!volume)volume=new GameObject("Mobile Color Grade").AddComponent<Volume>();volume.isGlobal=true;volume.gameObject.layer=0;
 string vp=Root+"Materials/MobileGrade.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(vp);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,vp);}volume.sharedProfile=profile;
 if(!profile.TryGet<Tonemapping>(out var tone)){tone=profile.Add<Tonemapping>();AssetDatabase.AddObjectToAsset(tone,profile);}tone.mode.Override(TonemappingMode.Neutral);
 if(!profile.TryGet<ColorAdjustments>(out var grade)){grade=profile.Add<ColorAdjustments>();AssetDatabase.AddObjectToAsset(grade,profile);}grade.postExposure.Override(.15f);grade.contrast.Override(8);grade.saturation.Override(8);EditorUtility.SetDirty(profile);
 PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS,"com.trexoinnovation.woollyarena");PlayerSettings.productName="Woolly Arena";PlayerSettings.bundleVersion="0.1.0";PlayerSettings.iOS.buildNumber="1";PlayerSettings.iOS.targetOSVersionString="15.0";PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);PlayerSettings.SetArchitecture(NamedBuildTarget.iOS,1);PlayerSettings.iOS.appleEnableAutomaticSigning=true;
 PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=true;PlayerSettings.allowedAutorotateToLandscapeRight=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS,new[]{GraphicsDeviceType.Metal});
 PrefabUtility.SaveAsPrefabAsset(player.gameObject,Root+"Prefabs/Woolly_Player.prefab");EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();DesertArtDirection.Apply();return "Touch UI, mobile lighting and iOS settings saved";
 }
 public static void Export(){var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"Scenes/TrainingArena.unity"},locationPathName="Builds/iOS",target=BuildTarget.iOS,options=BuildOptions.Development});System.IO.File.WriteAllText("Logs/ios-export.txt",report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nDuration: "+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("iOS export failed");}
}
}
