using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
namespace WoollyArena {
public sealed partial class EnemySpawnDirector:MonoBehaviour {
 public EnemyAgent prefab;public Transform player;public int maxEnemies=4;public float respawnDelay=3;public Vector3[] spawnPoints;public List<EnemyAgent> Enemies=new List<EnemyAgent>();
 public bool survivalMode=true; public SurvivalRun Run{get;private set;}
 public int AgentTypeId{get;private set;}public ShotEffects Effects{get;private set;}float nextSpawn;int serial,lastPoint=-1;NavMeshData navigationData;NavMeshDataInstance navigationInstance;
 void Start(){
  // Legacy target props are removed before either navigation or enemy spawning.
  foreach(var dummy in Object.FindObjectsByType<TargetDummy>()){dummy.gameObject.SetActive(false);Destroy(dummy.gameObject);}
  if(survivalMode){ArenaBiome.CreateForRun();KayKitArena.Dress();}
  Effects=Object.FindAnyObjectByType<ShotEffects>();
  if(!BuildNavigation()){Debug.LogError("Enemy navigation could not be built; spawning paused.",this);enabled=false;return;}
  foreach(var prop in Object.FindObjectsByType<BreakableProp>()){
   var box=prop.GetComponent<BoxCollider>();if(!box)continue;var obstacle=prop.GetComponent<NavMeshObstacle>();if(!obstacle)obstacle=prop.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.center=box.center;obstacle.size=box.size;obstacle.carving=true;obstacle.carveOnlyStationary=false;
  }
  if(player){var obstacle=player.GetComponent<NavMeshObstacle>();if(!obstacle)obstacle=player.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Capsule;obstacle.center=Vector3.up*.8f;obstacle.height=1.6f;obstacle.radius=.38f;obstacle.carving=false;}
  if(survivalMode){
   var hero=player?player.GetComponent<ArenaPlayer>():null;
   if(!hero){Debug.LogError("Survival requires an arena player.",this);enabled=false;return;}
   Run=gameObject.AddComponent<SurvivalRun>();Run.Initialize(this,hero);
  }
  else {for(int i=0;i<maxEnemies;i++)Spawn();nextSpawn=Time.time+respawnDelay;}
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
 public bool Spawn(bool boss=false){
  if(survivalMode&&(!Run||Run.Phase!=SurvivalRun.RunPhase.Wave))return false;
  if(!prefab||spawnPoints==null||spawnPoints.Length==0)return false;int start=Random.Range(0,spawnPoints.Length);
  int attempts=spawnPoints.Length*(survivalMode?3:1);
  for(int i=0;i<attempts;i++){int index=(start+i)%spawnPoints.Length;if(index==lastPoint)continue;var candidate=spawnPoints[index];if(survivalMode&&i>=spawnPoints.Length){var spread=Random.insideUnitCircle*1.8f;candidate+=new Vector3(spread.x,0,spread.y);}if(!NavMesh.SamplePosition(candidate,out var navPoint,.75f,NavMesh.AllAreas))continue;var p=navPoint.position;
   if(player&&Vector3.Distance(player.position,p)<3)continue;bool occupied=false;foreach(var e in Enemies)if(e&&Vector3.Distance(e.transform.position,p)<1.5f)occupied=true;
   if(occupied||Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.35f,.32f,1,QueryTriggerInteraction.Ignore))continue;
   SpawnAt(p,boss);lastPoint=index;return true;
  }return false;
 }
 EnemyAgent SpawnAt(Vector3 p,bool boss,int squad=0,int sector=-1){
   var bot=Instantiate(prefab,p,Quaternion.identity,transform);bot.name="Enemy "+(++serial);int slot=0;while(Enemies.Exists(e=>e&&e.CombatSlot==slot))slot++;bot.CombatSlot=slot;bot.target=player;bot.director=this;bot.SquadId=squad;bot.SpawnSector=sector;bot.vitals.displayName="RAIDER "+serial;if(survivalMode){
    bot.melee=true;int wave=Run.Wave;int kind=boss?(wave%10==0?5:4):(serial-1)%4;
    bot.GetComponent<HitReaction>().impulseCooldown=RunBalance.StaggerCooldown(wave);
    if(!boss&&kind==3&&Enemies.FindAll(e=>e&&!e.Defeated&&e.LaserRanged).Count>=Mathf.Min(4,1+wave/5))kind=0;
    bot.Role=EnemyTactics.Role(kind,slot,boss);
    bot.moveSpeed=(2.3f+Mathf.Min(1.2f,(wave-1)*.07f)+WaveDifficulty.SpeedBonus(wave))*(kind==1?1.4f:kind==2?.7f:boss?.78f:1);
    bot.shotDamage=8+wave+WaveDifficulty.DamageBonus(wave)+(kind==2?5:boss?8:0);
    bot.vitals.displayName=new[]{"İSKELET AKINCI","GÖLGE AVCISI","KEMİK SAVAŞÇI","KEMİK BÜYÜCÜ","İSKELET MUHAFIZI","BAŞ BÜYÜCÜ"}[kind];
    bot.vitals.ResetHealth(boss?WaveDifficulty.BossHealth(wave):Mathf.RoundToInt((32+wave*6)*WaveDifficulty.HealthScale(wave)*(kind==2?2:kind==1?.7f:1)));
    bot.gameObject.AddComponent<CreatureModel>().Initialize(bot,kind);
    if(!boss&&kind==3){
     bot.moveSpeed*=.85f;bot.gameObject.AddComponent<LaserEnemy>().Initialize(bot,true);
    }
    if(boss){bot.ConfigureBoss();bot.gameObject.AddComponent<BossEnemy>().Initialize(bot,kind==5);}
    else if(kind!=3){int elites=0;foreach(var enemy in Enemies)if(enemy&&!enemy.Defeated&&enemy.IsElite)elites++;if(WavePressure.EliteSpawn(wave,serial,elites))bot.gameObject.AddComponent<EliteEnemy>().Initialize(bot);}
   }
   bot.nameLabel.text=bot.vitals.displayName;bot.healthLabel.text=bot.vitals.Health.ToString();
   Enemies.Add(bot);return bot;
 }
 void Update(){if(survivalMode){TickSquads();return;}int removed=Enemies.RemoveAll(e=>!e);if(removed>0)nextSpawn=Time.time+respawnDelay;if(Enemies.Count<maxEnemies&&Time.time>=nextSpawn){Spawn();nextSpawn=Time.time+respawnDelay;}}
 void OnDestroy(){DestroySquads();if(navigationInstance.valid)navigationInstance.Remove();if(navigationData)Destroy(navigationData);}
}
}
