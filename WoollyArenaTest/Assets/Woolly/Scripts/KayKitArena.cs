using UnityEngine;
namespace WoollyArena {
 public sealed class KayKitArena:MonoBehaviour {
  public static void Dress(){new GameObject("KayKit arena dressing").AddComponent<KayKitArena>().Build();}
  void Build(){
   int index=0;foreach(var prop in Object.FindObjectsByType<BreakableProp>()){
    var model=KayKitAsset.Attach(index++%2==0?"barrel_large":"crates_stacked",prop.transform);
    if(!model)continue;
    // Fit within the existing collision footprint; navigation and destruction stay authoritative.
    var box=prop.GetComponent<BoxCollider>();if(box){var bounds=new Bounds();bool first=true;foreach(var r in model.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
     if(!first){var target=box.bounds;float factor=Mathf.Min(target.size.x/Mathf.Max(.01f,bounds.size.x),target.size.z/Mathf.Max(.01f,bounds.size.z),target.size.y/Mathf.Max(.01f,bounds.size.y));model.localScale*=factor;var updated=new Bounds();first=true;foreach(var r in model.GetComponentsInChildren<Renderer>()){if(first){updated=r.bounds;first=false;}else updated.Encapsulate(r.bounds);}model.position+=new Vector3(target.center.x-updated.center.x,target.min.y-updated.min.y,target.center.z-updated.center.z);}
    }
    prop.ReplaceArtwork(model);
   }
   // Decorations are outside the walkable 20 m arena, so they cannot block a spawn or dodge.
   for(int side=-1;side<=1;side+=2){
    for(int row=0;row<3;row++){
     var torch=KayKitAsset.Attach("torch_lit",transform,.65f);if(torch)torch.position=new Vector3(side*10.7f,0,-6+row*7);
     var banner=KayKitAsset.Attach(ArenaBiome.ActiveBiome==1?"banner_blue":"banner_red",transform,.65f);if(banner){banner.position=new Vector3(side*11.0f,1.2f,-7+row*7);banner.rotation=Quaternion.Euler(0,side<0?90:-90,0);}
    }
    var supplies=KayKitAsset.Attach("barrel_small_stack",transform,.7f);if(supplies)supplies.position=new Vector3(side*10.9f,0,-4);
    var chest=KayKitAsset.Attach("chest_gold",transform,.65f);if(chest){chest.position=new Vector3(side*10.9f,0,4);chest.rotation=Quaternion.Euler(0,side<0?60:-60,0);}
   }
  }
 }
}
