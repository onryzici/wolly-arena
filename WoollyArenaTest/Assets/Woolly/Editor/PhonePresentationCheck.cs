using System;
using System.IO;
using UnityEngine;
using UnityEditor;
namespace WoollyArena.Editor {
 public static class PhonePresentationCheck {
  public static void Run(){
   foreach(string path in new[]{"KayKit/skeleton_texture","KayKit/dungeon_texture","VFX/Muzzle","VFX/Smoke","VFX/Spark","VFX/Impact"}){
    var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/Woolly/Resources/"+path+".png");if(importer.textureShape!=TextureImporterShape.Texture2D){importer.textureShape=TextureImporterShape.Texture2D;importer.SaveAndReimport();}
    if(!Resources.Load<Texture2D>(path))throw new Exception("Expected Texture2D resource: "+path);
   }
   var backdrop=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Woolly/UI/Lobby/LobbyBackdrop.shader");
   if(!backdrop||ShaderUtil.ShaderHasError(backdrop))throw new Exception("Lobby backdrop shader failed; export blocked");
   var root=new GameObject("Phone build material review");root.transform.position=new Vector3(1000,0,0);
   var cameraObject=new GameObject("Review Camera");var camera=cameraObject.AddComponent<Camera>();camera.transform.position=new Vector3(1000,2.4f,-6);camera.transform.LookAt(new Vector3(1000,1,0));camera.orthographic=true;camera.orthographicSize=2.5f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.09f,.13f);camera.cullingMask=1<<30;
   var target=new RenderTexture(1000,650,24);Texture2D image=null;var previous=RenderTexture.active;
   try {
    var model=KayKitAsset.Attach("Skeleton_Rogue",root.transform);var barrel=KayKitAsset.Attach("barrel_large",root.transform,.65f);if(!model||!barrel)throw new Exception("KayKit prefabs missing");model.localPosition=Vector3.left*.8f;barrel.localPosition=Vector3.right*1.1f;
    string inspect="Before sampling\n";foreach(var r in model.GetComponentsInChildren<Renderer>())inspect+=r.name+" material="+(r.sharedMaterial?r.sharedMaterial.name:"NULL")+" shader="+(r.sharedMaterial?r.sharedMaterial.shader.name:"NULL")+" texture="+(r.sharedMaterial&&r.sharedMaterial.HasProperty("_BaseMap")?r.sharedMaterial.GetTexture("_BaseMap"):null)+"\n";File.WriteAllText("Logs/palette-diagnostic.txt",inspect);
    var clips=AssetDatabase.LoadAllAssetsAtPath("Assets/Woolly/Resources/KayKit/Skeleton_Rogue.fbx");foreach(var a in clips)if(a is AnimationClip clip&&clip.name=="Idle"){foreach(var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))File.AppendAllText("Logs/palette-diagnostic.txt",binding.path+":"+binding.propertyName+"\n");clip.SampleAnimation(model.gameObject,.3f);break;}
    int checkedMeshes=0;foreach(var r in root.GetComponentsInChildren<Renderer>()){
     r.gameObject.layer=30;var m=r.sharedMaterial;if(!m||m.shader.name!="Woolly/KayKitSurface"||!m.GetTexture("_BaseMap"))throw new Exception("Missing KayKit material or palette: "+r.name+" material="+m+" shader="+(m?m.shader.name:"none")+" texture="+(m&&m.HasProperty("_BaseMap")?m.GetTexture("_BaseMap"):null));
     if(ShaderUtil.ShaderHasError(m.shader))throw new Exception("KayKit surface shader compilation failed");checkedMeshes++;
    }
    camera.targetTexture=target;camera.Render();RenderTexture.active=target;image=new Texture2D(1000,650,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1000,650),0,0);image.Apply();File.WriteAllBytes("Logs/phone-material-review.png",image.EncodeToPNG());
    var colors=image.GetPixels32();int colored=0;foreach(var c in colors)if(c.r>c.b*1.3f&&c.r>c.g*1.15f&&c.r>50)colored++;if(colored<1500)throw new Exception("Material render contains insufficient colored pixels: "+colored);
    foreach(string shaderName in new[]{"CombatParticle","KayKitSurface"}){var shader=Resources.Load<Shader>(shaderName);if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("Shader error: "+shaderName);}
    File.WriteAllText("Logs/phone-presentation-check.txt","PASS: "+checkedMeshes+" textured renderers, "+colored+" colored pixels. Unity rendered palette check; no Play mode entered.\n");
   } finally {RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(root);}
  }
 }
}
