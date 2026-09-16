using UnityEngine;
using UnityEngine.InputSystem;
namespace WoollyArena {
[RequireComponent(typeof(CharacterController))]
public sealed class ArenaPlayer:MonoBehaviour {
 public Transform visual,muzzle; public Animator animator; public Camera view; public ShotEffects effects;
 public float walkSpeed=1.08f,runSpeed=3.36f,acceleration=15,turnSpeed=720,range=25;
 public MobileControls mobile;
 public WeaponState weapon=new WeaponState();
 public int Hits {get;private set;} public float Speed {get;private set;} public Vector3 AimPoint {get;private set;}
 public bool SimulatedInput; public Vector2 TestMove,TestAim; public bool TestRun,TestFire;
 CharacterController motor; Vector3 velocity; float gravity;
 CharacterVitals vitals;Vector3 spawnPosition;float respawnAt=-1;Vector3 lastMoveDirection;bool pendingShot;
 Transform hips,spine;float legYaw;float playback=1;
 static readonly int SpeedId=Animator.StringToHash("Speed"),FireId=Animator.StringToHash("Fire");
 void Awake(){motor=GetComponent<CharacterController>();vitals=GetComponent<CharacterVitals>();spawnPosition=transform.position;lastMoveDirection=visual.forward;if(!view)view=Camera.main;foreach(var t in visual.GetComponentsInChildren<Transform>()){if(t.name=="mixamorig:Hips")hips=t;if(t.name=="mixamorig:Spine")spine=t;}}
 public static Vector2 ClampInput(Vector2 input)=>Vector2.ClampMagnitude(input,1);
 void Update(){
  if(vitals&&vitals.Health==0){
   velocity=Vector3.zero;Speed=0;animator.SetFloat(SpeedId,0);
   if(respawnAt<0)respawnAt=Time.time+2;
   if(Time.time>=respawnAt){motor.enabled=false;transform.position=spawnPosition;motor.enabled=true;gravity=0;vitals.Heal(vitals.maxHealth);vitals.ProtectFor(2);weapon=new WeaponState();respawnAt=-1;if(mobile){mobile.move.ResetInput();mobile.aim.ResetInput();}}
   return;
  }
  var k=Keyboard.current;var mouse=Mouse.current;
  Vector2 input=SimulatedInput?TestMove:k==null?Vector2.zero:new Vector2((k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0),(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0));
  bool run=SimulatedInput?TestRun:k!=null&&(k.leftShiftKey.isPressed||k.rightShiftKey.isPressed);
  bool touch=mobile&&mobile.Active;
  if(!SimulatedInput&&touch&&mobile.move.Held){input=mobile.move.Value;run=input.magnitude>.72f;}
  var right=view.transform.right;right.y=0;right.Normalize();var forward=view.transform.forward;forward.y=0;forward.Normalize();
  input=ClampInput(input);var desired=(right*input.x+forward*input.y)*(run?runSpeed:walkSpeed);
  velocity=Vector3.MoveTowards(velocity,desired,acceleration*Time.deltaTime);
  gravity=motor.isGrounded?-2:Mathf.Max(gravity-25*Time.deltaTime,-30);
  var before=transform.position;motor.Move((velocity+Vector3.up*gravity)*Time.deltaTime);
  var actual=transform.position-before;actual.y=0;Speed=actual.magnitude/Mathf.Max(Time.deltaTime,.0001f);
  Vector3 aim=visual.forward;
  if(SimulatedInput){aim=new Vector3(TestAim.x,0,TestAim.y);AimPoint=transform.position+aim*range;}
  else if(touch&&(Application.isMobilePlatform||mobile.move.Held||mobile.aim.Held)){if(desired.sqrMagnitude>.02f)lastMoveDirection=desired.normalized;aim=lastMoveDirection;AimPoint=transform.position+aim*range;}
  else if(mouse!=null){var ray=view.ScreenPointToRay(mouse.position.ReadValue());if(new Plane(Vector3.up,transform.position+Vector3.up*1).Raycast(ray,out float d)){AimPoint=ray.GetPoint(d);aim=AimPoint-transform.position;aim.y=0;}}
  if(aim.sqrMagnitude>.05f)visual.rotation=Quaternion.RotateTowards(visual.rotation,Quaternion.LookRotation(aim),turnSpeed*Time.deltaTime);
  if(Speed>.15f){float dot=Vector3.Dot(velocity,visual.forward);playback=dot<-.2f?-1:1;var travel=velocity*playback;float targetYaw=Vector3.SignedAngle(visual.forward,travel,Vector3.up);legYaw=Mathf.MoveTowardsAngle(legYaw,targetYaw,540*Time.deltaTime);}else legYaw=Mathf.MoveTowardsAngle(legYaw,0,360*Time.deltaTime);
  animator.SetFloat("Playback",playback*1.2f);
  animator.SetFloat(SpeedId,Speed/1.2f,.09f,Time.deltaTime);weapon.Tick(Time.time);
  if(k!=null&&k.rKey.wasPressedThisFrame)weapon.Reload(Time.time);
  bool mobilePress=touch&&mobile.aim.ConsumePress();
  bool fire=SimulatedInput?TestFire:touch&&(mobile.aim.Held||mobilePress)?true:!Application.isMobilePlatform&&mouse!=null&&mouse.leftButton.isPressed&&!(UnityEngine.EventSystems.EventSystem.current&&UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());
  if(fire&&weapon.TryFire(Time.time)){pendingShot=true;animator.ResetTrigger(FireId);animator.SetTrigger(FireId);}
 }
 void LateUpdate(){
  if(pendingShot){pendingShot=false;Fire();}
  if(!hips||!spine)return;
  // Keep the aim pose while rotating the animated lower body toward travel.
  var upper=spine.rotation;hips.rotation=Quaternion.AngleAxis(legYaw,Vector3.up)*hips.rotation;spine.rotation=upper;
 }
 void Fire(){
  // Aim from the barrel, and never shoot through cover between body and barrel.
  Vector3 direction=visual.forward;var origin=muzzle.position;var chest=transform.position+Vector3.up*1.05f;
  int mask=~(1<<gameObject.layer);var end=origin+direction*range;RaycastHit hit;bool impacted=false;
  if(Physics.Linecast(chest,origin,out hit,mask,QueryTriggerInteraction.Ignore)||Physics.Raycast(origin,direction,out hit,range,mask,QueryTriggerInteraction.Ignore)){
   impacted=true;end=hit.point;var target=hit.collider.GetComponentInParent<TargetDummy>();if(target){target.Damage(25);Hits++;}else{var prop=hit.collider.GetComponentInParent<BreakableProp>();var enemy=hit.collider.GetComponentInParent<EnemyAgent>();if(prop){prop.Damage(25);Hits++;}else if(enemy){enemy.Damage(25);Hits++;}}
  }
  effects.Play(origin,end,impacted,impacted?hit.normal:Vector3.up); 
 }
}
}
