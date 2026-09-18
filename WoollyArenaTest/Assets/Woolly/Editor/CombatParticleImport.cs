using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 public sealed class CombatParticleImport:AssetPostprocessor {
  public override uint GetVersion()=>2;
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/Woolly/Resources/VFX/"))return;
   var i=(TextureImporter)assetImporter;i.textureType=TextureImporterType.Default;i.textureShape=TextureImporterShape.Texture2D;i.alphaSource=TextureImporterAlphaSource.FromInput;i.alphaIsTransparency=true;i.wrapMode=TextureWrapMode.Clamp;i.mipmapEnabled=false;i.maxTextureSize=512;i.textureCompression=TextureImporterCompression.CompressedHQ;
  }
 }
}
