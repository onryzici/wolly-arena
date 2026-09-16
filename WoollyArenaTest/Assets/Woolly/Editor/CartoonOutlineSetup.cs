using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
namespace WoollyArena.Editor {
public static class CartoonOutlineSetup {
 static Material Material(string name,float width,Color color){var p="Assets/Woolly/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Woolly/CartoonOutline"));AssetDatabase.CreateAsset(m,p);}m.SetFloat("_Width",width);m.SetColor("_OutlineColor",color);EditorUtility.SetDirty(m);return m;}
 public static string Apply(){
 var bodyMat=Material("Character ink",.019f,new Color(.13f,.075f,.11f,1));var propMat=Material("Environment ink",.012f,new Color(.30f,.17f,.14f,1));
 var player=Object.FindFirstObjectByType<ArenaPlayer>();
 foreach(var source in player.visual.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>!r.name.EndsWith("_Outline")).ToArray()){
  if(source.transform.Find(source.name+"_Outline"))continue;var g=new GameObject(source.name+"_Outline");g.layer=source.gameObject.layer;g.transform.SetParent(source.transform,false);var r=g.AddComponent<SkinnedMeshRenderer>();r.sharedMesh=source.sharedMesh;r.bones=source.bones;r.rootBone=source.rootBone;r.localBounds=source.localBounds;r.sharedMaterials=Enumerable.Repeat(bodyMat,source.sharedMesh.subMeshCount).ToArray();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
 }
 var art=GameObject.Find("Environment_Art");var list=player.visual.GetComponentsInChildren<MeshFilter>().Concat(art?art.GetComponentsInChildren<MeshFilter>().Where(f=>AssetDatabase.GetAssetPath(f.sharedMesh).EndsWith(".fbx")):new MeshFilter[0]).ToArray();int count=0;
 foreach(var f in list){if(f.name.EndsWith("_Outline")||f.name.Contains("Grass")||f.transform.parent.name.Contains("Grass"))continue;if(f.transform.Find(f.name+"_Outline"))continue;var g=new GameObject(f.name+"_Outline");g.layer=f.gameObject.layer;g.transform.SetParent(f.transform,false);g.AddComponent<MeshFilter>().sharedMesh=f.sharedMesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterials=Enumerable.Repeat(f.transform.IsChildOf(player.visual)?bodyMat:propMat,f.sharedMesh.subMeshCount).ToArray();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;count++;}
 PrefabUtility.SaveAsPrefabAsset(player.gameObject,"Assets/Woolly/Prefabs/Woolly_Player.prefab");EditorSceneManager.MarkSceneDirty(player.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Character contour and "+count+" prop outlines saved";
 }
}
}
