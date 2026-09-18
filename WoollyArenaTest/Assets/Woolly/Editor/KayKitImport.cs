using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 public sealed class KayKitImport:AssetPostprocessor {
  public override uint GetVersion()=>3;
  bool Included=>assetPath.StartsWith("Assets/Woolly/Resources/KayKit/");
  bool Character=>assetPath.EndsWith("Skeleton_Minion.fbx")||assetPath.EndsWith("Skeleton_Rogue.fbx")||assetPath.EndsWith("Skeleton_Warrior.fbx")||assetPath.EndsWith("Skeleton_Mage.fbx");
  void OnPreprocessModel(){if(!Included)return;var m=(ModelImporter)assetImporter;m.useFileScale=true;m.globalScale=1;m.importAnimation=Character;m.animationType=Character?ModelImporterAnimationType.Legacy:ModelImporterAnimationType.None;m.optimizeGameObjects=false;m.preserveHierarchy=true;m.importCameras=false;m.importLights=false;m.materialImportMode=ModelImporterMaterialImportMode.None;}
  void OnPreprocessAnimation(){if(!Included||!Character)return;var m=(ModelImporter)assetImporter;var clips=m.defaultClipAnimations;foreach(var c in clips){int split=c.name.LastIndexOf('|');if(split>=0)c.name=c.name.Substring(split+1);bool loop=c.name=="Idle"||c.name=="Walking_A"||c.name=="Running_A";c.loopTime=loop;c.wrapMode=loop?WrapMode.Loop:WrapMode.ClampForever;}m.clipAnimations=clips;}
  void OnPreprocessTexture(){if(!Included)return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;t.sRGBTexture=true;t.filterMode=FilterMode.Point;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;}
 }
}
