using UnityEngine;
namespace WoollyArena {
 public sealed class KayKitEnemy:MonoBehaviour {
  EnemyAgent agent;Animation clips;KayKitAsset surface;string current;Vector3 previous;float flashUntil,nextFlash;
  public static bool Attach(EnemyAgent bot,int kind,Transform parent){
   string[] names={"Skeleton_Minion","Skeleton_Rogue","Skeleton_Warrior","Skeleton_Mage","Skeleton_Warrior","Skeleton_Mage"};
   var model=KayKitAsset.Attach(names[Mathf.Clamp(kind,0,5)],parent,.70f);if(!model)return false;
   var p=model.gameObject.AddComponent<KayKitEnemy>();p.agent=bot;p.surface=model.GetComponent<KayKitAsset>();p.clips=model.GetComponentInChildren<Animation>();p.previous=bot.transform.position;bot.vitals.Damaged+=p.Hit;
   foreach(var joint in model.GetComponentsInChildren<Transform>())if(joint.name=="handslot.r"){
    KayKitAsset.Attach(kind==3||kind==5?"Skeleton_Staff":kind==2||kind==4?"Skeleton_Axe":"Skeleton_Blade",joint);break;
   }
   p.Set("Idle");return true;
  }
  void Hit(Vector3 direction){if(Time.time<nextFlash)return;flashUntil=Time.time+.045f;nextFlash=Time.time+.18f;}
  void Set(string name){if(!clips||!clips[name]||current==name)return;current=name;clips.CrossFade(name,.10f);}
  void LateUpdate(){
   if(!agent||!clips)return;if(agent.director&&agent.director.Run&&agent.director.Run.IsPaused)return;
   surface.Flash(Time.time<flashUntil?.4f:0);
   if(!agent.enabled){clips.enabled=false;return;}
   if(agent.Defeated){Set("Death_A");return;}
   float speed=Time.deltaTime>0?Vector3.Distance(previous,agent.transform.position)/Time.deltaTime:0;previous=agent.transform.position;
   float attack=agent.MeleeAttackProgress;
   if(attack>=0){Set("1H_Melee_Attack_Chop");var state=clips["1H_Melee_Attack_Chop"];if(state){state.speed=0;state.normalizedTime=attack;clips.Sample();}}
   else if(agent.AttackWindup){Set("Spellcast_Shoot");if(clips[current])clips[current].speed=.8f;}
   else {Set(speed>.15f?"Running_A":"Idle");if(clips[current])clips[current].speed=speed>.15f?Mathf.Clamp(speed/2,.65f,1.6f):1;}
  }
  void OnDestroy(){if(agent&&agent.vitals)agent.vitals.Damaged-=Hit;}
 }
}
