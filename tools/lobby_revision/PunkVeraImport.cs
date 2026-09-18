using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 public sealed class PunkVeraImport:AssetPostprocessor {
  public override uint GetVersion()=>3;
  bool Vera=>assetPath=="Assets/Woolly/Resources/Characters/PunkVera.fbx"||assetPath=="Assets/Woolly/Resources/Characters/PunkVeraLobby.fbx";
  void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Woolly/Resources/Characters/"))return;var texture=(TextureImporter)assetImporter;texture.npotScale=TextureImporterNPOTScale.None;texture.maxTextureSize=assetPath.Contains("Portrait")?1024:2048;texture.mipmapEnabled=!assetPath.Contains("Portrait");texture.alphaIsTransparency=assetPath.Contains("Portrait");texture.wrapMode=TextureWrapMode.Clamp;}
  void OnPreprocessModel(){if(!Vera)return;var model=(ModelImporter)assetImporter;model.animationType=ModelImporterAnimationType.Generic;model.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;model.importAnimation=true;model.optimizeGameObjects=false;model.importCameras=false;model.importLights=false;model.materialImportMode=ModelImporterMaterialImportMode.None;}
  void OnPreprocessAnimation(){if(!Vera)return;var model=(ModelImporter)assetImporter;var clips=model.defaultClipAnimations;foreach(var clip in clips){clip.loopTime=true;clip.loopPose=true;clip.lockRootRotation=true;clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.keepOriginalOrientation=true;clip.keepOriginalPositionXZ=true;clip.keepOriginalPositionY=true;}model.clipAnimations=clips;}
 }
}
