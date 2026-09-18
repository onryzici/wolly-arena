using System;
using UnityEditor;
using UnityEngine;
namespace WoollyArena.Editor {
 // Runs only on an explicitly requested export; rejects oversized or broken skinned poses.
 public static class KayKitScaleChecks {
  public static void Run(){
   foreach(string name in new[]{"Skeleton_Minion","Skeleton_Rogue","Skeleton_Warrior","Skeleton_Mage"}){
    var root=new GameObject("Scale review "+name);var baked=new Mesh();
    try{
     var model=KayKitAsset.Attach(name,root.transform);if(!model)throw new Exception("Missing "+name);
     var importer=(ModelImporter)AssetImporter.GetAtPath("Assets/Woolly/Resources/KayKit/"+name+".fbx");
     if(!importer.useFileScale)throw new Exception(name+": file unit conversion must be enabled; reimport required");
     var clips=AssetDatabase.LoadAllAssetsAtPath(importer.assetPath);int count=0;
     foreach(var asset in clips)if(asset is AnimationClip clip&&!clip.name.StartsWith("__preview")){
      count++;
      for(int step=0;step<=16;step++){
       clip.SampleAnimation(model.gameObject,clip.length*step/16);
       var bounds=new Bounds();bool first=true;
       foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>()){
        skin.BakeMesh(baked);
        foreach(var vertex in baked.vertices){var point=root.transform.InverseTransformPoint(skin.transform.TransformPoint(vertex));
         if(float.IsNaN(point.x)||float.IsNaN(point.y)||float.IsNaN(point.z)||float.IsInfinity(point.sqrMagnitude))throw new Exception(name+": non-finite skin pose");
         if(first){bounds=new Bounds(point,Vector3.zero);first=false;}else bounds.Encapsulate(point);
        }
       }
       float span=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
       if(first||span<.2f||span>4||bounds.center.magnitude>4)throw new Exception(name+" / "+clip.name+": invalid animated bounds "+bounds);
       foreach(var socket in model.GetComponentsInChildren<Transform>())if(socket.name=="handslot.r"){
        var scale=socket.lossyScale;float biggest=Mathf.Max(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));
        if(biggest<.9f||biggest>1.1f)throw new Exception(name+": weapon socket scale "+scale);
       }
      }
     }
     if(count!=7)throw new Exception(name+": expected seven validated clips, got "+count);
    }finally{UnityEngine.Object.DestroyImmediate(baked);UnityEngine.Object.DestroyImmediate(root);}
   }
  }
 }
}
