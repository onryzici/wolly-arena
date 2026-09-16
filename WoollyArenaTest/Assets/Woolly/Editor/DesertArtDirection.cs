using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace WoollyArena.Editor {
public static class DesertArtDirection {
 static Color Hex(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
 static void Material(string name,string hex){var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Woolly/Materials/"+name+".mat");if(!m)return;m.shader=Shader.Find("Woolly/ArenaToon");m.SetColor("_BaseColor",Hex(hex));m.SetFloat("_Smoothness",0);m.SetFloat("_Metallic",0);EditorUtility.SetDirty(m);}
 [MenuItem("Woolly/Apply Warm Desert Palette")]
 public static string Apply(){
 Material("Arena Teal","#F6A16E");Material("Tile Teal","#F6A16E");Material("Path Sand","#F6A16E");
 Material("Cover Navy","#BA5743");Material("Cover Top","#F27C55");Material("Target Coral","#F46558");Material("Target Cream","#FFE6A3");Material("Charcoal","#74463F");Material("Signal Gold","#FFD34E");
 var env=GameObject.Find("Arena_Geometry");if(env)foreach(Transform t in env.transform)if(t.name=="FloorTile"||t.name=="CentralLane")t.gameObject.SetActive(false);
 var sun=GameObject.Find("Sun").GetComponent<Light>();sun.color=Hex("#FFF3DF");sun.intensity=1.15f;sun.transform.rotation=Quaternion.Euler(57,-32,0);sun.shadowStrength=.48f;sun.shadows=LightShadows.Soft;
 RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=Hex("#B9BEC4");RenderSettings.ambientEquatorColor=Hex("#A6A0A0");RenderSettings.ambientGroundColor=Hex("#81746D");
 var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Woolly/Materials/MobileGrade.asset");if(profile.TryGet<ColorAdjustments>(out var grade)){grade.postExposure.Override(0);grade.contrast.Override(0);grade.saturation.Override(0);EditorUtility.SetDirty(grade);}if(profile.TryGet<Tonemapping>(out var tone)){tone.mode.Override(TonemappingMode.None);EditorUtility.SetDirty(tone);}EditorUtility.SetDirty(profile);
 EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Warm sand, terracotta cover, warm ambient and lighter shadows saved";
 }
}
}
