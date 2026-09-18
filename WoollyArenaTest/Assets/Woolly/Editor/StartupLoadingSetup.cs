using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WoollyArena.Editor {
 public static class StartupLoadingSetup {
  public const string ScenePath="Assets/Woolly/Scenes/Startup.unity";
  public static readonly string[] Scenes={ScenePath,"Assets/Woolly/Scenes/Lobby.unity","Assets/Woolly/Scenes/TrainingArena.unity"};
  public static void Prepare(){
   const string artPath="Assets/Woolly/Resources/MenuSkin/LoadingTrio4K.png";
   var importer=(TextureImporter)AssetImporter.GetAtPath(artPath);
   if(!importer)throw new InvalidOperationException("Startup artwork is missing");
   importer.maxTextureSize=4096;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;importer.isReadable=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
   importer.SaveAndReimport();
   var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(artPath);
   if(!texture||texture.width!=3840||texture.height!=2160)throw new InvalidOperationException("Startup wallpaper must remain 3840 x 2160");
   var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath("8e7e1d3065361de429c2dd7e8683134e"));
   if(!font)throw new InvalidOperationException("Lobby font is missing");
   if(!font.HasCharacters("WOOLLY ARENA Yükleniyor… Lobi hazırlanıyor… Hazır!",out uint[] missing,true))throw new InvalidOperationException("Startup font lacks Turkish glyphs");
   if(Application.isBatchMode)EditorSceneManager.OpenScene(Scenes[1]);
   var previous=SceneManager.GetActiveScene();
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
   try {
    SceneManager.SetActiveScene(scene);
    var root=new GameObject("Startup Loading");root.AddComponent<StartupLoadingScreen>().font=font;
    var camera=new GameObject("Startup Camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.04f,.07f,.09f);camera.cullingMask=0;
    if(!EditorSceneManager.SaveScene(scene,ScenePath))throw new InvalidOperationException("Could not save startup scene");
   }finally{EditorSceneManager.CloseScene(scene,true);if(previous.IsValid())SceneManager.SetActiveScene(previous);}
   EditorBuildSettings.scenes=Array.ConvertAll(Scenes,path=>new EditorBuildSettingsScene(path,true));
   // Native launch art covers engine initialization; the Startup scene covers lobby loading.
   PlayerSettings.SplashScreen.show=false;
   PlayerSettings.iOS.SetiPhoneLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
   PlayerSettings.iOS.SetiPadLaunchScreenType(iOSLaunchScreenType.ImageAndBackgroundRelative);
   PlayerSettings.iOS.SetLaunchScreenImage(texture,iOSLaunchScreenImageType.iPhoneLandscapeImage);
   PlayerSettings.iOS.SetLaunchScreenImage(texture,iOSLaunchScreenImageType.iPhonePortraitImage);
   PlayerSettings.iOS.SetLaunchScreenImage(texture,iOSLaunchScreenImageType.iPadImage);
   Review(font,1920,1080,0);
   Review(font,1600,738,58);
   AssetDatabase.SaveAssets();
   File.WriteAllText("Logs/startup-setup.txt","PASS: 3840x2160 artwork; native launch image; Startup → Lobby → TrainingArena; existing lobby font.\n");
  }
  static void Review(TMP_FontAsset font,int width,int height,int inset){
   var root=new GameObject("Startup Review");var view=root.AddComponent<StartupLoadingScreen>();view.font=font;
   var cameraObject=new GameObject("Startup Review Camera");var camera=cameraObject.AddComponent<Camera>();camera.transform.position=new Vector3(1000,1000,-10);camera.orthographic=true;camera.orthographicSize=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.cullingMask=1<<29;
   var target=new RenderTexture(width,height,24);Texture2D image=null;var previous=RenderTexture.active;
   try{
    camera.targetTexture=target;view.BuildView();var canvas=root.GetComponentInChildren<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
    foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=29;
    view.ApplyLayout(width,height,new Rect(inset,15,width-2*inset,height-15));
    var bar=root.GetComponentsInChildren<UnityEngine.UI.Image>();foreach(var b in bar)if(b.name=="Progress")b.fillAmount=.65f;
    Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes("Logs/startup-review-"+width+".png",image.EncodeToPNG());
   }finally{RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);}
  }
 }
}
