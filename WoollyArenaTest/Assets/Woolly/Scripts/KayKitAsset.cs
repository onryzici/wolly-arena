using UnityEngine;
namespace WoollyArena {
 public sealed class KayKitAsset:MonoBehaviour {
  Material material;Renderer[] renderers;MaterialPropertyBlock block;
  public static Transform Attach(string name,Transform parent,float scale=1){
   var prefab=Resources.Load<GameObject>("KayKit/"+name);if(!prefab)return null;
   var instance=Instantiate(prefab,parent,false);instance.name=name;instance.transform.localScale*=scale;
   var owner=instance.AddComponent<KayKitAsset>();owner.material=new Material(Resources.Load<Shader>("KayKitSurface"));owner.material.SetTexture("_BaseMap",Resources.Load<Texture2D>("KayKit/"+(name.StartsWith("Skeleton_")?"skeleton_texture":"dungeon_texture")));owner.material.color=Color.white;owner.material.SetTextureScale("_BaseMap",Vector2.one);owner.material.SetTextureOffset("_BaseMap",Vector2.zero);
   if(!owner.material.GetTexture("_BaseMap"))Debug.LogError("KayKit palette texture missing: "+name,instance);
   owner.renderers=instance.GetComponentsInChildren<Renderer>();owner.block=new MaterialPropertyBlock();
   foreach(var r in owner.renderers){var skinned=r as SkinnedMeshRenderer;var filter=r.GetComponent<MeshFilter>();var mesh=skinned?skinned.sharedMesh:filter?filter.sharedMesh:null;var materials=new Material[Mathf.Max(1,mesh?mesh.subMeshCount:r.sharedMaterials.Length)];for(int i=0;i<materials.Length;i++)materials[i]=owner.material;r.sharedMaterials=materials;}
   return instance.transform;
  }
  public void Flash(float value){block.SetFloat("_HitFlash",value);foreach(var r in renderers)r.SetPropertyBlock(block);}
  void OnDestroy(){if(material)Destroy(material);}
 }
}
