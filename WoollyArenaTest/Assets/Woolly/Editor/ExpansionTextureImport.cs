using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 // Preserve the landscape aspect ratio of the generated ground and map thumbnails.
 public sealed class ExpansionTextureImport:AssetPostprocessor {
  public override uint GetVersion()=>1;
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/Woolly/Resources/Biomes/"))return;
   var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
   importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=2048;importer.mipmapEnabled=assetPath.Contains("Ground");importer.filterMode=FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.CompressedHQ;
  }
 }
}
