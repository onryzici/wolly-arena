using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 public sealed class PunkVeraImport:AssetPostprocessor {
  public override uint GetVersion()=>5;
  bool PlayableModel=>assetPath=="Assets/Woolly/Resources/Characters/PunkVera.fbx"||assetPath=="Assets/Woolly/Resources/Characters/Patchwork.fbx";
  void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Woolly/Resources/Characters/"))return;var texture=(TextureImporter)assetImporter;texture.npotScale=TextureImporterNPOTScale.None;texture.maxTextureSize=assetPath.Contains("Portrait")?1024:2048;texture.mipmapEnabled=!assetPath.Contains("Portrait");texture.alphaIsTransparency=assetPath.Contains("Portrait");texture.wrapMode=TextureWrapMode.Clamp;
   if(assetPath.Contains("Portrait")){texture.textureType=TextureImporterType.Sprite;texture.spriteImportMode=SpriteImportMode.Single;texture.alphaSource=TextureImporterAlphaSource.FromInput;texture.textureCompression=TextureImporterCompression.Uncompressed;var settings=new TextureImporterSettings();texture.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;texture.SetTextureSettings(settings);}
}
  void OnPreprocessModel(){if(!PlayableModel)return;var model=(ModelImporter)assetImporter;model.animationType=ModelImporterAnimationType.Generic;model.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;model.importAnimation=true;model.optimizeGameObjects=false;model.importCameras=false;model.importLights=false;model.materialImportMode=ModelImporterMaterialImportMode.None;}
  void OnPreprocessAnimation(){if(!PlayableModel)return;var model=(ModelImporter)assetImporter;var clips=model.defaultClipAnimations;foreach(var clip in clips){clip.loopTime=true;clip.loopPose=true;clip.lockRootRotation=true;clip.lockRootPositionXZ=true;clip.lockRootHeightY=true;clip.keepOriginalOrientation=true;clip.keepOriginalPositionXZ=true;clip.keepOriginalPositionY=true;}model.clipAnimations=clips;}
 }
}
