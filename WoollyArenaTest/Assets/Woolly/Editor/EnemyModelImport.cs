using UnityEditor;
namespace WoollyArena.Editor {
 // Keep authored limb pivots; mesh merging would silently remove locomotion joints.
 public sealed class EnemyModelImport:AssetPostprocessor {
  void OnPreprocessModel(){
   if(!assetPath.StartsWith("Assets/Woolly/Resources/Models/"))return;
   string name=System.IO.Path.GetFileNameWithoutExtension(assetPath);
   if(name!="MossMaw"&&name!="SpineRaptor"&&name!="HornBrute"&&name!="StitchReaper"&&name!="FrostWarden"&&name!="ThornMatriarch")return;
   var model=(ModelImporter)assetImporter;model.preserveHierarchy=true;model.optimizeGameObjects=false;model.importAnimation=false;model.importCameras=false;model.importLights=false;model.isReadable=false;
  }
 }
}
