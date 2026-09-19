using UnityEngine;
using UnityEngine.AI;
using TMPro;
namespace WoollyArena {
public sealed class EnemyAgent:MonoBehaviour {
 public Transform visual,target;public Animator animator;public CharacterVitals vitals;public TMP_Text nameLabel,healthLabel;public RectTransform healthFill;public Canvas nameplate;public EnemySpawnDirector director;
 public float moveSpeed=2.35f,attackRange=6.5f,shotInterval=1.35f;public int shotDamage=125;public int CombatSlot{get;set;}
 public EnemyRole Role {get;set;}
 public int SquadId {get;set;}
 public int SpawnSector {get;set;}=-1;
 public Vector3 TacticalGoal {get;private set;}
 public float AimHeight { get; private set; } = .9f;
 public void UseSmallCreatureBody(){AimHeight=.65f;hitbox.height=1.1f;hitbox.center=Vector3.up*.55f;hitbox.radius=.25f;navigation.height=1.1f;navigation.radius=.38f;}
 public void HaltForWaveClear(){pendingShot=false;hitbox.enabled=false;if(navigation.enabled&&navigation.isOnNavMesh){navigation.isStopped=true;navigation.velocity=Vector3.zero;}animator.SetFloat("Speed",0);enabled=false;}
 public bool melee;
 public bool LaserRanged;
 public bool IsElite {get;set;}
 public readonly CombatAilments Ailments=new CombatAilments();
 Renderer[] deathRenderers;
 float lastVisibleHit=-100;
 public void HideDefeatedBody(){if(deathRenderers==null)deathRenderers=visual.GetComponentsInChildren<Renderer>();foreach(var renderer in deathRenderers)if(renderer)renderer.enabled=false;}
 public void Ignite(int damage){if(!Defeated)Ailments.Ignite(Time.time,damage);}
 public void Chill(float strength){if(!Defeated)Ailments.Chill(Time.time,strength);}
 public bool IsBoss {get;private set;}
 public bool AttackWindup {get;set;}
 readonly MeleeStrike meleeStrike=new MeleeStrike();
 public float MeleeAttackProgress=>meleeStrike.Progress(Time.time);
 public void ConfigureBoss(){IsBoss=true;hitReaction.impulseCooldown=.9f;AimHeight=1.2f;hitbox.height=2.0f;hitbox.center=Vector3.up;hitbox.radius=.45f;navigation.radius=.45f;navigation.height=2;nameplate.transform.localPosition=Vector3.up*2.35f;nameLabel.gameObject.SetActive(true);healthLabel.gameObject.SetActive(true);foreach(var t in visual.GetComponentsInChildren<Transform>())if(t.name=="Creature silhouette")t.localScale=Vector3.one*1.15f;}

 public bool Defeated{get;private set;}
 public float hitPushDistance=.38f;
 HitReaction hitReaction;
 float born,deathAt,nextPath,nextShot,settledAt=-1,legYaw;bool pendingShot,hasGoal;Vector3 baseScale,combatGoal,lastGoalTarget;NavMeshAgent navigation;CapsuleCollider hitbox;Transform muzzle,hips,spine;CharacterVitals targetVitals;ShotEffects effects;Camera view;
 NavMeshPath candidatePath;readonly RaycastHit[] hitBuffer=new RaycastHit[32];
 void Awake(){candidatePath=new NavMeshPath();baseScale=visual.localScale;born=Time.time;nextShot=born+Random.Range(1.1f,1.8f);view=Camera.main;var oldMotor=GetComponent<CharacterController>();if(oldMotor)oldMotor.enabled=false;
  hitbox=gameObject.AddComponent<CapsuleCollider>();hitbox.center=Vector3.up*.8f;hitbox.height=1.6f;hitbox.radius=.32f;hitbox.enabled=false;
  navigation=gameObject.AddComponent<NavMeshAgent>();navigation.enabled=false;navigation.radius=.52f;navigation.height=1.6f;navigation.speed=moveSpeed;navigation.acceleration=8;navigation.angularSpeed=480;navigation.updateRotation=false;navigation.stoppingDistance=.28f;navigation.autoRepath=true;navigation.obstacleAvoidanceType=ObstacleAvoidanceType.HighQualityObstacleAvoidance;
  foreach(var t in visual.GetComponentsInChildren<Transform>()){if(t.name=="Muzzle")muzzle=t;if(t.name=="mixamorig:Hips")hips=t;if(t.name=="mixamorig:Spine")spine=t;}
  animator.SetFloat("Speed",0);animator.SetFloat("Playback",1);
  hitReaction=gameObject.AddComponent<HitReaction>();visual=hitReaction.Initialize(visual,vitals,hitPushDistance);
  hitReaction.impulseCooldown=.45f;
 }
 public bool Damage(int amount,Vector3 direction=default,bool critical=false,bool interrupt=true,bool periodic=false){
  int healthBefore=vitals.Health;
  if(amount<=0||Defeated||(director&&director.Run&&director.Run.Phase!=SurvivalRun.RunPhase.Wave)||Time.time-born<.35f)return false;
  hitReaction.SuppressImpulse=!interrupt;
  if(!periodic&&director&&director.Run&&(IsBoss||IsElite))amount=RunBalance.ArmoredDamage(amount,director.Run.Wave,IsBoss,IsElite);
  bool damaged=vitals.Damage(amount,direction);hitReaction.SuppressImpulse=false;
  if(!damaged)return false;
  lastVisibleHit=Time.time;
  if(director&&director.Run)director.Run.Rewards.Hit(transform.position,healthBefore-vitals.Health,critical,direction);
  if(hitReaction.InterruptedLastHit){pendingShot=false;meleeStrike.Cancel();settledAt=-1;nextShot=Mathf.Max(nextShot,Time.time+.22f);animator.ResetTrigger("Fire");
  if(navigation.enabled&&navigation.isOnNavMesh){navigation.isStopped=true;navigation.velocity=Vector3.zero;}}
  if(vitals.Health==0){Defeated=true;Ailments.Clear();if(director&&director.Run){director.Run.RegisterKill();director.Run.Rewards.Death(transform.position,direction);director.Run.DeathFX.BreakApart(this,direction,critical);if(IsBoss)director.Run.Rewards.BossReward(transform.position);}hitbox.enabled=false;nameplate.enabled=false;deathAt=Time.time;}
  return true;
 }
 void MoveKnockback(){
  var displacement=hitReaction.ConsumeDisplacement(Time.deltaTime);float distance=displacement.magnitude;
  if(distance<.00001f||!navigation.enabled||!navigation.isOnNavMesh)return;
  var direction=displacement/distance;var origin=transform.position;
  // Constrain the swept body against live props as well as the baked navigation edge.
  int count=Physics.CapsuleCastNonAlloc(origin+Vector3.up*.36f,origin+Vector3.up*1.24f,.32f,direction,hitBuffer,distance+.025f,~0,QueryTriggerInteraction.Ignore);
  for(int i=0;i<count;i++)if(!hitBuffer[i].collider.transform.IsChildOf(transform))distance=Mathf.Min(distance,Mathf.Max(0,hitBuffer[i].distance-.025f));
  if(navigation.Raycast(origin+direction*distance,out var edge))distance=Mathf.Min(distance,Mathf.Max(0,Vector3.Distance(origin,edge.position)-.025f));
  navigation.Move(direction*distance);
 }
 bool FirstHit(Vector3 origin,Vector3 direction,float distance,out RaycastHit closest){closest=default;float nearest=float.PositiveInfinity;int count=Physics.RaycastNonAlloc(origin,direction,hitBuffer,distance,~0,QueryTriggerInteraction.Ignore);for(int i=0;i<count;i++){var hit=hitBuffer[i];if(hit.collider.transform.IsChildOf(transform))continue;if(hit.distance<nearest){nearest=hit.distance;closest=hit;}}return nearest<float.PositiveInfinity;}
 bool SightFrom(Vector3 point){var d=target.position+Vector3.up*1.05f-point;return FirstHit(point,d.normalized,d.magnitude+.2f,out var h)&&h.collider.GetComponentInParent<ArenaPlayer>();}
 float NearestAlly(Vector3 point){float result=100;if(director)foreach(var e in director.Enemies)if(e&&e!=this&&!e.Defeated)result=Mathf.Min(result,Vector3.Distance(point,e.transform.position));return result;}
 void ChooseGoal(){
  nextPath=Time.time+.65f+CombatSlot*.035f;float best=float.PositiveInfinity;bool found=false;float baseAngle=CombatSlot*Mathf.PI*.5f+.35f;
  for(int i=0;i<5;i++){float shift=i==0?0:i%2==1?((i+1)/2)*.30f:-(i/2)*.30f;float angle=baseAngle+shift;float radius=4.5f+(CombatSlot%2)*.45f;var candidate=target.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*radius;
   if(!NavMesh.SamplePosition(candidate,out var sample,1.15f,NavMesh.AllAreas))continue;if(!NavMesh.CalculatePath(transform.position,sample.position,NavMesh.AllAreas,candidatePath)||candidatePath.status!=NavMeshPathStatus.PathComplete)continue;
   float crowded=Mathf.Max(0,1.5f-NearestAlly(sample.position));float score=Vector3.Distance(transform.position,sample.position)*.15f+Mathf.Abs(shift)*2+crowded*10+(SightFrom(sample.position+Vector3.up*1.05f)?0:8);
   if(score<best){best=score;combatGoal=sample.position;found=true;}}
  // Search toward the player's reachable region when every firing slot is inaccessible.
  if(!found&&NavMesh.SamplePosition(target.position,out var fallback,1,NavMesh.AllAreas)){combatGoal=fallback.position;found=true;}
  if(found){hasGoal=true;navigation.SetDestination(combatGoal);lastGoalTarget=target.position;}
 }
 void ChooseTacticalGoal(bool sight){
  nextPath=Time.time+.32f+(CombatSlot%7)*.035f;
  var desired=EnemyTactics.Goal(Role,transform.position,target.position,director?director.PlayerVelocity:Vector3.zero,CombatSlot,sight);
  if(!NavMesh.SamplePosition(desired,out var nav,1.2f,NavMesh.AllAreas)||!NavMesh.CalculatePath(transform.position,nav.position,NavMesh.AllAreas,candidatePath)||candidatePath.status!=NavMeshPathStatus.PathComplete){
   if(!NavMesh.SamplePosition(target.position,out nav,1,NavMesh.AllAreas))return;
  }
  TacticalGoal=nav.position;navigation.SetDestination(TacticalGoal);
 }
 void Update(){
  if(director&&director.Run&&director.Run.IsPaused)return;
  if(!Defeated&&(!director||!director.Run||director.Run.Phase==SurvivalRun.RunPhase.Wave)){int burn=Ailments.TickBurn(Time.time);if(burn>0)Damage(burn,Vector3.zero,false,false,true);}
  if(Defeated){MoveKnockback();visual.localScale=baseScale*Mathf.Max(0,1-(Time.time-deathAt)/.22f);if(Time.time-deathAt>.25f)Destroy(gameObject);return;}
  float u=Mathf.Clamp01((Time.time-born)/.28f);visual.localScale=baseScale*Mathf.SmoothStep(.15f,1,u);
  if(u>=1&&!navigation.enabled&&NavMesh.SamplePosition(transform.position,out var point,1,NavMesh.AllAreas)){transform.position=point.position;if(director)navigation.agentTypeID=director.AgentTypeId;navigation.avoidancePriority=35+(CombatSlot%4)*12;navigation.enabled=true;hitbox.enabled=true;combatGoal=transform.position;}
  if(!target||!navigation.enabled||!navigation.isOnNavMesh)return;if(!targetVitals)targetVitals=target.GetComponent<CharacterVitals>();if(!effects&&director)effects=director.Effects;
  bool recoiling=hitReaction.Recoiling;MoveKnockback();
  var delta=target.position-transform.position;delta.y=0;float distance=delta.magnitude;var direction=distance>.001f?delta/distance:visual.forward;bool alive=targetVitals&&targetVitals.Health>0;bool seesPlayer=alive&&SightFrom(transform.position+Vector3.up*1.05f);
  if(melee){
   nameplate.enabled=IsBoss||IsElite||(vitals.Health<vitals.maxHealth*.5f&&Time.time-lastVisibleHit<1);
   nameLabel.text=vitals.displayName;
   navigation.speed=moveSpeed*(1-Ailments.Slow(Time.time,IsBoss));navigation.isStopped=recoiling||AttackWindup||MeleeAttackProgress>=0||!alive||(LaserRanged?seesPlayer&&distance>=4.2f&&distance<=7.2f:distance<(IsBoss?1.35f:.9f));
   if(alive&&!recoiling&&!AttackWindup&&Time.time>=nextPath){ChooseTacticalGoal(seesPlayer);}
   if(direction.sqrMagnitude>.01f)visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(direction),360*Time.deltaTime);
   animator.SetFloat("Speed",navigation.velocity.magnitude,.12f,Time.deltaTime);animator.SetFloat("Playback",1);
   if(!recoiling&&!AttackWindup&&alive&&distance<(IsBoss?1.65f:1.2f)&&seesPlayer&&Time.time>=nextShot){nextShot=Time.time+.95f;meleeStrike.Begin(Time.time);}
   if(meleeStrike.Resolve(Time.time,!recoiling&&!AttackWindup&&alive&&seesPlayer&&distance<(IsBoss?1.8f:1.3f))&&targetVitals.Damage(shotDamage,direction))targetVitals.ProtectFor(.22f);
   healthLabel.text=vitals.Health.ToString();healthFill.anchorMax=new Vector2(Mathf.Clamp01((float)vitals.Health/vitals.maxHealth),1);if(view)nameplate.transform.rotation=view.transform.rotation;
   return;
  }
  bool atSlot=hasGoal&&Vector3.Distance(transform.position,combatGoal)<.65f;bool comfortable=seesPlayer&&distance>3.2f&&distance<attackRange-.3f&&NearestAlly(transform.position)>1.15f;
  navigation.isStopped=recoiling||!alive||(atSlot&&comfortable);
  if(!recoiling&&alive&&Time.time>=nextPath&&(!atSlot||!comfortable||Vector3.Distance(lastGoalTarget,target.position)>.8f))ChooseGoal();
  // Small local spacing correction complements agent avoidance without a shared destination.
  if(!recoiling&&alive&&director){Vector3 separation=Vector3.zero;foreach(var e in director.Enemies){if(!e||e==this||e.Defeated)continue;var away=transform.position-e.transform.position;away.y=0;float d=away.magnitude;if(d>.01f&&d<1.15f)separation+=away/d*(1.15f-d);}if(separation.sqrMagnitude>.001f)navigation.Move(Vector3.ClampMagnitude(separation,.65f)*Time.deltaTime);}
  var velocity=navigation.velocity;float speed=velocity.magnitude;var facing=seesPlayer?direction:speed>.15f?velocity:direction;facing.y=0;if(facing.sqrMagnitude>.01f)visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(facing),360*Time.deltaTime);
  float playback=Vector3.Dot(velocity,visual.forward)<-.15f?-1:1;float yaw=speed>.15f?Vector3.SignedAngle(visual.forward,velocity*playback,Vector3.up):0;legYaw=Mathf.MoveTowardsAngle(legYaw,yaw,420*Time.deltaTime);animator.SetFloat("Speed",speed,.12f,Time.deltaTime);animator.SetFloat("Playback",playback);
  if(!recoiling&&speed<.25f&&seesPlayer){if(settledAt<0)settledAt=Time.time;}else settledAt=-1;
  if(!recoiling&&seesPlayer&&distance<attackRange&&settledAt>=0&&Time.time-settledAt>.2f&&Time.time>=nextShot&&Vector3.Dot(visual.forward,direction)>.985f&&muzzle){nextShot=Time.time+shotInterval+Random.Range(-.12f,.18f);pendingShot=true;animator.ResetTrigger("Fire");animator.SetTrigger("Fire");}
  nameLabel.text=vitals.displayName;healthLabel.text=vitals.Health.ToString();healthFill.anchorMax=new Vector2(Mathf.Clamp01((float)vitals.Health/vitals.maxHealth),1);if(view)nameplate.transform.rotation=view.transform.rotation;
 }
 void LateUpdate(){
  if(director&&director.Run&&director.Run.IsPaused){pendingShot=false;return;}
  if(Defeated)return;if(hips&&spine){var upper=spine.rotation;hips.rotation=Quaternion.AngleAxis(legYaw,Vector3.up)*hips.rotation;spine.rotation=upper;}
  if(!pendingShot)return;pendingShot=false;if(!targetVitals||targetVitals.Health==0)return;var chest=transform.position+Vector3.up*1.05f;var barrel=muzzle.position;var segment=barrel-chest;if(FirstHit(chest,segment.normalized,segment.magnitude,out _))return;
  var destination=target.position+Vector3.up*1.05f;var aim=Quaternion.Euler(0,Random.Range(-1.5f,1.5f),0)*(destination-barrel).normalized;bool hit=FirstHit(barrel,aim,attackRange,out var impact);var end=hit?impact.point:barrel+aim*attackRange;if(hit&&impact.collider.GetComponentInParent<ArenaPlayer>())targetVitals.Damage(shotDamage,aim);if(effects)effects.Play(barrel,end,hit,hit?impact.normal:Vector3.up,true);
 }
}
}
