using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 public sealed class MenuSkinImport:AssetPostprocessor {
  void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/Woolly/Resources/MenuSkin/"))return;var t=(TextureImporter)assetImporter;t.textureType=TextureImporterType.Default;t.textureShape=TextureImporterShape.Texture2D;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.textureCompression=TextureImporterCompression.Uncompressed;t.wrapMode=TextureWrapMode.Clamp;}
 }
}
