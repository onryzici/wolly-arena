using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 // One-shot request file created only for an explicitly requested phone deployment.
 [InitializeOnLoad] public static class AuthorizedPhoneBuildRequest {
  const string Request="Logs/authorized-phone-build.request";
  static AuthorizedPhoneBuildRequest(){EditorApplication.update+=Check;}
  static void Check(){
   if(!File.Exists(Request)||EditorApplication.isCompiling||EditorApplication.isUpdating||BuildPipeline.isBuildingPlayer)return;
   File.Delete(Request);if(File.Exists("Logs/phone-build-request-error.txt"))File.Delete("Logs/phone-build-request-error.txt");
   if(EditorApplication.isPlayingOrWillChangePlaymode){File.WriteAllText("Logs/phone-build-request-error.txt","Editor is in Play mode; build was not started and Play mode was not changed.");return;}
   try{PhoneBuild.Export();}catch(Exception e){File.WriteAllText("Logs/phone-build-request-error.txt",e.ToString());Debug.LogException(e);}
  }
 }
}
