using UnityEngine;
using System.Collections.Generic;
namespace WoollyArena {
 // Imported geometry with explicit URP materials; no dependency on FBX material remapping.
 public sealed class CraftedModel:MonoBehaviour {
  readonly List<Material> owned=new List<Material>();Renderer[] meshes;MaterialPropertyBlock flash;
  public static Transform Attach(string name,Transform parent,float scale=1){
   var prefab=Resources.Load<GameObject>("Models/"+name);if(!prefab)return null;
   var instance=Instantiate(prefab,parent,false);instance.name=name;instance.transform.localScale*=scale;
   instance.AddComponent<CraftedModel>().Prepare();return instance.transform;
  }
  void Prepare(){
   meshes=GetComponentsInChildren<Renderer>();flash=new MaterialPropertyBlock();var shader=Resources.Load<Shader>("CombatModel");
   foreach(var renderer in meshes){var source=renderer.sharedMaterials;var mapped=new Material[source.Length];
    for(int i=0;i<source.Length;i++){string label=source[i]?source[i].name:"";int at=label.IndexOf("C_");Color color=Color.white;if(at>=0&&label.Length>=at+8)ColorUtility.TryParseHtmlString("#"+label.Substring(at+2,6),out color);var material=new Material(shader){color=color};if(label.Contains("EnemyVertexPalette"))material.SetFloat("_UseVertexColors",1);mapped[i]=material;owned.Add(material);}
    renderer.sharedMaterials=mapped;
   }
  }
  public void SetFlash(float amount){if(flash==null)return;flash.SetFloat("_HitFlash",amount);foreach(var r in meshes)r.SetPropertyBlock(flash);}
  void OnDestroy(){foreach(var material in owned)if(material)Destroy(material);}
 }
}
