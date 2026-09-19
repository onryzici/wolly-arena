using UnityEngine;
namespace WoollyArena {
 public sealed class KayKitArena:MonoBehaviour {
  public static void Dress(){new GameObject("KayKit arena dressing").AddComponent<KayKitArena>().Build();}
  void Build(){
   string[] suppliesModels={"barrel_large_decorated","crates_stacked","keg_decorated","box_stacked"};
   int index=0;foreach(var prop in Object.FindObjectsByType<BreakableProp>()){
    var model=KayKitAsset.Attach(suppliesModels[index++%suppliesModels.Length],prop.transform);
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
    Place("rubble_large",new Vector3(side*10.9f,0,-8.1f),.32f,side*27);
    Place("sword_shield_broken",new Vector3(side*10.65f,.02f,-2.1f),.85f,side*40);
    Place("sword_shield_gold",new Vector3(side*10.8f,.04f,6.1f),.8f,side*65);
    Place("coin_stack_large",new Vector3(side*10.65f,.025f,3.3f),.8f,side*15);
    Place("bottle_A_green",new Vector3(side*10.65f,.03f,-4.9f),.9f,side*20);
   }
  }
  void Place(string asset,Vector3 point,float scale,float yaw){var model=KayKitAsset.Attach(asset,transform,scale);if(!model)return;model.position=point;model.rotation=Quaternion.Euler(0,yaw,0);}
 }
}
