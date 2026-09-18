using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 public sealed class CombatAudioImport:AssetPostprocessor {
  void OnPreprocessAudio(){
   if(!assetPath.StartsWith("Assets/Woolly/Resources/Audio/"))return;
   var importer=(AudioImporter)assetImporter;bool music=(assetPath.Contains("ArenaRush")||assetPath.Contains("RoboWestern"));
   importer.forceToMono=!music;importer.loadInBackground=music;
   var settings=importer.defaultSampleSettings;settings.preloadAudioData=!music;settings.loadType=music?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;
   settings.compressionFormat=music?AudioCompressionFormat.Vorbis:AudioCompressionFormat.PCM;settings.quality=.75f;importer.defaultSampleSettings=settings;
  }
 }
}
