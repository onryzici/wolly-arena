using UnityEngine;
namespace WoollyArena {
 public sealed class ArenaBiome:MonoBehaviour {
  public static int ActiveBiome {get;private set;}
  public static int Selected {get=>Mathf.Clamp(PlayerPrefs.GetInt("Woolly.Biome",0),0,1);set{PlayerPrefs.SetInt("Woolly.Biome",Mathf.Clamp(value,0,1));PlayerPrefs.Save();}}
  public static string Title(int biome)=>biome==1?"BUZUL KORUSU":"KIZIL KANYON";
  Material groundMaterial,iceMaterial;
  public static void CreateForRun(){
   int biome=Selected;
   if(SurvivalRun.CheckpointsEnabled&&RunSession.ResumeRequested&&RunCheckpointStore.TryLoad(RunSession.SavePath,out var saved))biome=saved.Biome;
   ActiveBiome=biome;if(biome==1)new GameObject("Buzul Korusu").AddComponent<ArenaBiome>().Build();
  }
  void Build(){
   var ground=GameObject.Find("Ground");var env=GameObject.Find("Arena_Geometry");if(!ground||!env)return;
   var renderer=ground.GetComponent<Renderer>();groundMaterial=new Material(renderer.sharedMaterial);groundMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("Biomes/FrostGround"));renderer.sharedMaterial=groundMaterial;
   iceMaterial=new Material(Resources.Load<Shader>("CombatModel")){color=new Color(.36f,.7f,.79f)};
   // Apply geometry before the director builds navigation: visible ice and collision agree.
   Vector3[] positions={new Vector3(-4,0,4.6f),new Vector3(4,0,4.6f),new Vector3(-5,0,-.4f),new Vector3(5,0,-.4f),new Vector3(-3,0,-5),new Vector3(3,0,-5)};
   int i=0;foreach(var box in env.GetComponentsInChildren<BoxCollider>()){
    if(box.name!="Cover")continue;box.transform.position=positions[i++%positions.Length]+Vector3.up*.5f;box.transform.localScale=new Vector3(2.4f,1,1.3f);
    var r=box.GetComponent<Renderer>();r.enabled=false;var wall=CraftedModel.Attach("IceWall",transform);if(wall)wall.position=box.transform.position-Vector3.up*.5f;
    var cluster=CraftedModel.Attach("IceCluster",transform,.8f);if(cluster)cluster.position=box.transform.position+new Vector3(0,.35f,0);
   }
   foreach(var prop in Object.FindObjectsByType<BreakableProp>()){
    // Reskin barrels/crates as frost-covered supplies while retaining their breakable colliders.
    foreach(var r in prop.GetComponentsInChildren<Renderer>())r.sharedMaterial=iceMaterial;
   }
   for(int side=-1;side<=1;side+=2)for(int n=0;n<9;n++){
    var tree=CraftedModel.Attach("FrostFir",transform,1+(n%3)*.16f);if(tree)tree.position=new Vector3(side*(10.8f+(n%2)*.25f),0,-9+n*2.3f);
    if(n<8){var back=CraftedModel.Attach("FrostFir",transform,.9f+(n%3)*.12f);if(back)back.position=new Vector3(-9+n*2.6f,0,side*10.4f);}
   }
   for(int n=0;n<8;n++){var ice=CraftedModel.Attach("IceCluster",transform,.55f);if(ice)ice.position=new Vector3((n%2==0?-1:1)*9.5f,0,-8+(n/2)*5);}
  }
  void OnDestroy(){if(groundMaterial)Destroy(groundMaterial);if(iceMaterial)Destroy(iceMaterial);}
 }
}
