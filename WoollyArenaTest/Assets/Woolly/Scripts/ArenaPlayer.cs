using UnityEngine;
using UnityEngine.InputSystem;
namespace WoollyArena {
[RequireComponent(typeof(CharacterController))]
public sealed class ArenaPlayer:MonoBehaviour {
 public Transform visual,muzzle; public Animator animator; public Camera view; public ShotEffects effects;
 public float walkSpeed=1.08f,runSpeed=3.36f,acceleration=15,turnSpeed=720,range=25;
 public MobileControls mobile;
 public CharacterStats Stats {get;set;} public LoadoutCombat Loadout {get;set;}
 public float hitPushDistance=.16f;
 HitReaction hitReaction;
 public DodgeAbility Dodge {get;private set;}
 public bool CanDodge=>!CombatPaused&&Dodge&&Dodge.Ready&&vitals&&vitals.Health>0&&motor&&motor.enabled&&motor.isGrounded;
 bool dodgeRequested;
 public WeaponState weapon=new WeaponState();
 public bool autoCombat; public int shotDamage=25; public bool respawnOnDeath=true;
 bool combatPaused;
 public bool CombatPaused {
  get=>combatPaused;
  set {combatPaused=value;if(!value)return;pendingShot=false;dodgeRequested=false;bufferedFireUntil=-1;autoTarget=null;velocity=Vector3.zero;Speed=0;if(animator){animator.ResetTrigger(FireId);animator.SetFloat(SpeedId,0);}if(Dodge)Dodge.ResetAbility();}
 }
 EnemySpawnDirector director; EnemyAgent autoTarget; float bufferedFireUntil=-1;
 public EnemyAgent AutoTarget=>autoTarget;
 public int Hits {get;private set;} public float Speed {get;private set;} public Vector3 AimPoint {get;private set;}
 public bool SimulatedInput; public Vector2 TestMove,TestAim; public bool TestRun,TestFire;
 CharacterController motor; Vector3 velocity; float gravity;
 CharacterVitals vitals;Vector3 spawnPosition;float respawnAt=-1;Vector3 lastMoveDirection;bool pendingShot;
 public Vector3 LastMoveDirection=>lastMoveDirection.sqrMagnitude>.001f?lastMoveDirection:Vector3.forward;
 Transform hips,spine;float legYaw;float playback=1;
 static readonly int SpeedId=Animator.StringToHash("Speed"),FireId=Animator.StringToHash("Fire");
 void Awake(){PlayableCharacter.ApplyToPlayer(this);motor=GetComponent<CharacterController>();vitals=GetComponent<CharacterVitals>();spawnPosition=transform.position;lastMoveDirection=visual.forward;if(!view)view=Camera.main;foreach(var t in visual.GetComponentsInChildren<Transform>()){if(t.name=="mixamorig:Hips")hips=t;if(t.name=="mixamorig:Spine")spine=t;}hitReaction=gameObject.AddComponent<HitReaction>();visual=hitReaction.Initialize(visual,vitals,hitPushDistance);Dodge=GetComponent<DodgeAbility>();if(!Dodge)Dodge=gameObject.AddComponent<DodgeAbility>();Dodge.Initialize(this,vitals,hitReaction);CharacterAccessories.Apply(animator.transform,PlayableCharacter.Active);}
 public bool RequestDodge(){if(!CanDodge)return false;dodgeRequested=true;return true;}
 void OnApplicationFocus(bool focused){if(!focused)dodgeRequested=false;}
 void OnApplicationPause(bool paused){if(paused)dodgeRequested=false;}
 void OnDisable(){dodgeRequested=false;pendingShot=false;if(Dodge)Dodge.ResetAbility();}
 public static Vector2 ClampInput(Vector2 input)=>Vector2.ClampMagnitude(input,1);
 void Update(){
  if(CombatPaused){pendingShot=false;dodgeRequested=false;bufferedFireUntil=-1;Speed=0;velocity=Vector3.zero;animator.SetFloat(SpeedId,0);return;}
  if(vitals&&vitals.Health==0){
   velocity=Vector3.zero;Speed=0;pendingShot=false;dodgeRequested=false;Dodge.ResetAbility();animator.SetFloat(SpeedId,0);
   if(!respawnOnDeath)return;
   if(respawnAt<0)respawnAt=Time.time+2;
   if(Time.time>=respawnAt){motor.enabled=false;transform.position=spawnPosition;motor.enabled=true;gravity=0;hitReaction.ResetReaction();vitals.Heal(vitals.maxHealth);vitals.ProtectFor(2);weapon=new WeaponState();respawnAt=-1;if(mobile){mobile.move.ResetInput();mobile.aim.ResetInput();}}
   return;
  }
  var k=Keyboard.current;var mouse=Mouse.current;
  Vector2 input=SimulatedInput?TestMove:k==null?Vector2.zero:new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
  bool run=SimulatedInput?TestRun:autoCombat||k!=null&&(k.leftShiftKey.isPressed||k.rightShiftKey.isPressed);
  bool touch=mobile&&mobile.Active;
  if(!SimulatedInput&&touch&&mobile.move.Held){input=mobile.move.Value;run=autoCombat||input.magnitude>.72f;}
  var right=view.transform.right;right.y=0;right.Normalize();var forward=view.transform.forward;forward.y=0;forward.Normalize();
  input=ClampInput(input);var desired=(right*input.x+forward*input.y)*(run?runSpeed:walkSpeed);
  if(input.sqrMagnitude>.04f)lastMoveDirection=desired.normalized;
  Vector3 aim=visual.forward;
  if(SimulatedInput){aim=new Vector3(TestAim.x,0,TestAim.y);AimPoint=transform.position+aim*range;}
  else if(touch&&(Application.isMobilePlatform||mobile.move.Held||mobile.aim.Held)){aim=LastMoveDirection;AimPoint=transform.position+aim*range;}
  else if(mouse!=null){var ray=view.ScreenPointToRay(mouse.position.ReadValue());if(new Plane(Vector3.up,transform.position+Vector3.up*1).Raycast(ray,out float d)){AimPoint=ray.GetPoint(d);aim=AimPoint-transform.position;aim.y=0;}}
  if(autoCombat&&!SimulatedInput){
   autoTarget=FindTarget();
   if(!autoTarget&&desired.sqrMagnitude>.02f){aim=desired.normalized;AimPoint=transform.position+aim*range;}
   if(autoTarget){AimPoint=autoTarget.transform.position+Vector3.up*autoTarget.AimHeight;if(!Loadout){aim=AimPoint-transform.position;aim.y=0;}}
   if(Loadout)aim=desired.sqrMagnitude>.02f?desired.normalized:visual.forward;
  }
  if(!Dodge.IsDodging&&aim.sqrMagnitude>.05f)visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(aim),turnSpeed*Time.deltaTime);
  bool request=dodgeRequested||(!SimulatedInput&&k!=null&&k.spaceKey.wasPressedThisFrame);dodgeRequested=false;
  if(request&&Dodge.TryBegin(input.sqrMagnitude>.04f?desired:LastMoveDirection,motor.isGrounded)){velocity=Vector3.zero;pendingShot=false;}
  bool dodging=Dodge.IsDodging;
  if(!dodging)velocity=Vector3.MoveTowards(velocity,desired,acceleration*Time.deltaTime);
  gravity=motor.isGrounded?-2:Mathf.Max(gravity-25*Time.deltaTime,-30);
  var before=transform.position;var displacement=dodging?Dodge.ConsumeDisplacement():velocity*Time.deltaTime;displacement+=hitReaction.ConsumeDisplacement(Time.deltaTime);
  motor.Move(displacement+Vector3.up*(gravity*Time.deltaTime));
  var actual=transform.position-before;actual.y=0;Speed=actual.magnitude/Mathf.Max(Time.deltaTime,.0001f);if(dodging)Dodge.ReportMovement(actual);
  if(dodging&&!Dodge.IsDodging)velocity=desired;
  if(dodging)legYaw=0;
  else if(Speed>.15f){float dot=Vector3.Dot(velocity,visual.forward);playback=dot<-.2f?-1:1;var travel=velocity*playback;float targetYaw=Vector3.SignedAngle(visual.forward,travel,Vector3.up);legYaw=Mathf.MoveTowardsAngle(legYaw,targetYaw,540*Time.deltaTime);}else legYaw=Mathf.MoveTowardsAngle(legYaw,0,360*Time.deltaTime);
  animator.SetFloat("Playback",playback*1.2f);
  animator.SetFloat(SpeedId,dodging?runSpeed/1.2f:Speed/1.2f,.06f,Time.deltaTime);weapon.Tick(Time.time);
  if(k!=null&&k.rKey.wasPressedThisFrame)weapon.Reload(Time.time);
  bool mobilePress=touch&&mobile.aim.ConsumePress();
  bool fire=SimulatedInput?TestFire:touch&&(mobile.aim.Held||mobilePress)?true:!Application.isMobilePlatform&&mouse!=null&&mouse.leftButton.isPressed&&!(UnityEngine.EventSystems.EventSystem.current&&UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());
  if(mobilePress)bufferedFireUntil=Time.time+.35f;
  if(autoCombat&&!SimulatedInput)fire=autoTarget&&Vector3.Dot(visual.forward,(AimPoint-transform.position).normalized)>.9f;
  else fire|=Time.time<=bufferedFireUntil;
  if(autoCombat&&Loadout&&!SimulatedInput)fire=false;
  if(!dodging&&fire&&weapon.TryFire(Time.time)){bufferedFireUntil=-1;pendingShot=true;animator.ResetTrigger(FireId);animator.SetTrigger(FireId);}
 }
 void LateUpdate(){
  if(pendingShot){pendingShot=false;if(!CombatPaused&&vitals&&vitals.Health>0)Fire();}
  if(Loadout || !hips||!spine)return;
  // Keep the aim pose while rotating the animated lower body toward travel.
  var upper=spine.rotation;hips.rotation=Quaternion.AngleAxis(legYaw,Vector3.up)*hips.rotation;spine.rotation=upper;
 }
 public EnemyAgent FindTarget(){
  if(!director)director=Object.FindAnyObjectByType<EnemySpawnDirector>();
  if(!director)return null;
  EnemyAgent best=null;float nearest=range*range;
  foreach(var enemy in director.Enemies){
   if(!enemy||!enemy.isActiveAndEnabled||enemy.Defeated||!enemy.vitals||enemy.vitals.Health<=0)continue;
   var origin=Loadout?transform.position+Vector3.up*.9f:muzzle.position;
   var point=enemy.transform.position+Vector3.up*enemy.AimHeight;float distance=(point-origin).sqrMagnitude;
   if(distance>=nearest)continue;
   var delta=point-origin;
   if(Physics.Raycast(origin,delta.normalized,out var hit,delta.magnitude+.1f,~(1<<gameObject.layer),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<EnemyAgent>()==enemy){best=enemy;nearest=distance;}
  }
  return best;
 }
 public void FireEquipment(Vector3 origin,Vector3 direction,int damage,float distance){
  if(CombatPaused||!vitals||vitals.Health<=0)return;
  var chest=transform.position+Vector3.up*1.05f;int mask=~(1<<gameObject.layer);RaycastHit hit;
  bool impacted=Physics.Linecast(chest,origin,out hit,mask,QueryTriggerInteraction.Ignore)||Physics.Raycast(origin,direction,out hit,distance,mask,QueryTriggerInteraction.Ignore);
  var end=impacted?hit.point:origin+direction*distance;
  if(impacted){var enemy=hit.collider.GetComponentInParent<EnemyAgent>();var prop=hit.collider.GetComponentInParent<BreakableProp>();if(enemy&&enemy.Damage(damage,direction,Stats && Stats.LastCritical))Hits++;else if(prop)prop.Damage(damage);}
  if(effects)effects.Play(origin,end,impacted,impacted?hit.normal:Vector3.up);
  if(animator && !Loadout)animator.SetTrigger(FireId);
 }
 void Fire(){
  // Aim from the barrel, and never shoot through cover between body and barrel.
  Vector3 direction=autoCombat&&!SimulatedInput&&autoTarget?(autoTarget.transform.position+Vector3.up*autoTarget.AimHeight-muzzle.position).normalized:visual.forward;var origin=muzzle.position;var chest=transform.position+Vector3.up*1.05f;
  int mask=~(1<<gameObject.layer);var end=origin+direction*range;RaycastHit hit;bool impacted=false;
  if(Physics.Linecast(chest,origin,out hit,mask,QueryTriggerInteraction.Ignore)||Physics.Raycast(origin,direction,out hit,range,mask,QueryTriggerInteraction.Ignore)){
   impacted=true;end=hit.point;var target=hit.collider.GetComponentInParent<TargetDummy>();if(target){target.Damage(shotDamage);Hits++;}else{var prop=hit.collider.GetComponentInParent<BreakableProp>();var enemy=hit.collider.GetComponentInParent<EnemyAgent>();if(prop){prop.Damage(shotDamage);Hits++;}else if(enemy&&enemy.Damage(shotDamage,direction)){Hits++;}}
  }
  effects.Play(origin,end,impacted,impacted?hit.normal:Vector3.up); 
 }
}
}
