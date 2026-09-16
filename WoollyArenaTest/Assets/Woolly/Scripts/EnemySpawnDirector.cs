using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
namespace WoollyArena {
public sealed class EnemySpawnDirector:MonoBehaviour {
 public EnemyAgent prefab;public Transform player;public int maxEnemies=4;public float respawnDelay=3;public Vector3[] spawnPoints;public List<EnemyAgent> Enemies=new List<EnemyAgent>();
 public int AgentTypeId{get;private set;}public ShotEffects Effects{get;private set;}float nextSpawn;int serial,lastPoint=-1;NavMeshData navigationData;NavMeshDataInstance navigationInstance;
 void Start(){
  // Legacy target props are removed before either navigation or enemy spawning.
  foreach(var dummy in Object.FindObjectsByType<TargetDummy>()){dummy.gameObject.SetActive(false);Destroy(dummy.gameObject);}
  Effects=Object.FindAnyObjectByType<ShotEffects>();
  if(!BuildNavigation()){Debug.LogError("Enemy navigation could not be built; spawning paused.",this);enabled=false;return;}
  foreach(var prop in Object.FindObjectsByType<BreakableProp>()){
   var box=prop.GetComponent<BoxCollider>();if(!box)continue;var obstacle=prop.GetComponent<NavMeshObstacle>();if(!obstacle)obstacle=prop.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=box.center;obstacle.size=box.size;obstacle.carving=true;obstacle.carveOnlyStationary=false;
  }
  if(player){var obstacle=player.GetComponent<NavMeshObstacle>();if(!obstacle)obstacle=player.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Capsule;obstacle.center=Vector3.up*.8f;obstacle.height=1.6f;obstacle.radius=.38f;obstacle.carving=false;}
  for(int i=0;i<maxEnemies;i++)Spawn();nextSpawn=Time.time+respawnDelay;
 }
 bool BuildNavigation(){
  var sources=new List<NavMeshBuildSource>();var ground=GameObject.Find("Ground");var environment=GameObject.Find("Arena_Geometry");if(!ground||!environment||NavMesh.GetSettingsCount()==0)return false;
  void AddBox(BoxCollider box,int area){if(!box||!box.enabled)return;sources.Add(new NavMeshBuildSource{shape=NavMeshBuildSourceShape.Box,transform=box.transform.localToWorldMatrix*Matrix4x4.Translate(box.center),size=box.size,area=area});}
  AddBox(ground.GetComponent<BoxCollider>(),0);
  foreach(var box in environment.GetComponentsInChildren<BoxCollider>())if(box.name=="Cover"||box.name=="Boundary")AddBox(box,1);
  var settings=NavMesh.GetSettingsByIndex(0);AgentTypeId=settings.agentTypeID;settings.agentRadius=.48f;settings.agentHeight=1.6f;settings.agentClimb=.15f;settings.agentSlope=35;settings.overrideVoxelSize=true;settings.voxelSize=.10f;
  navigationData=NavMeshBuilder.BuildNavMeshData(settings,sources,new Bounds(Vector3.zero,new Vector3(21.2f,6,20)),Vector3.zero,Quaternion.identity);
  if(!navigationData)return false;navigationInstance=NavMesh.AddNavMeshData(navigationData);return navigationInstance.valid;
 }
 bool Spawn(){
  if(!prefab||spawnPoints==null||spawnPoints.Length==0)return false;int start=Random.Range(0,spawnPoints.Length);
  for(int i=0;i<spawnPoints.Length;i++){int index=(start+i)%spawnPoints.Length;if(index==lastPoint)continue;if(!NavMesh.SamplePosition(spawnPoints[index],out var navPoint,.75f,NavMesh.AllAreas))continue;var p=navPoint.position;
   if(player&&Vector3.Distance(player.position,p)<3)continue;bool occupied=false;foreach(var e in Enemies)if(e&&Vector3.Distance(e.transform.position,p)<1.5f)occupied=true;
   if(occupied||Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.35f,.32f,1,QueryTriggerInteraction.Ignore))continue;
   var bot=Instantiate(prefab,p,Quaternion.identity,transform);bot.name="Enemy "+(++serial);int slot=0;while(Enemies.Exists(e=>e&&e.CombatSlot==slot))slot++;bot.CombatSlot=slot;bot.target=player;bot.director=this;bot.vitals.displayName="RAIDER "+serial;Enemies.Add(bot);lastPoint=index;return true;
  }return false;
 }
 void Update(){int removed=Enemies.RemoveAll(e=>!e);if(removed>0)nextSpawn=Time.time+respawnDelay;if(Enemies.Count<maxEnemies&&Time.time>=nextSpawn){Spawn();nextSpawn=Time.time+respawnDelay;}}
 void OnDestroy(){if(navigationInstance.valid)navigationInstance.Remove();if(navigationData)Destroy(navigationData);}
}
}
