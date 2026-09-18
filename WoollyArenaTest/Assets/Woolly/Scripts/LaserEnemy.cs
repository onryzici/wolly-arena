using UnityEngine;
namespace WoollyArena {
 public sealed class LaserEnemy:MonoBehaviour {
  EnemyAgent agent;SurvivalRun run;LineRenderer warning,bolt,core;Material material;
  Vector3 aim,position,direction;float next,fireAt,travel;bool charging,flying,spell;Transform orb;Color spellColor;
  readonly RaycastHit[] hits=new RaycastHit[32];
  public void Initialize(EnemyAgent owner,bool magic=false){
   spell=magic;spellColor=spell?new Color(.25f,.85f,1):new Color(1,.25f,.65f);
   agent=owner;run=agent.director.Run;agent.LaserRanged=true;next=Time.time+2+(agent.CombatSlot%4)*.3f;
   material=new Material(Resources.Load<Shader>("PowerInk"));material.SetColor("_Tint",Color.white);
   warning=Make("Laser aim warning",.025f,new Color(1,.25f,.45f,.45f));bolt=Make("Laser bolt glow",.16f,new Color(1,.12f,.5f));core=Make("Laser bolt core",.045f,Color.white);
   if(spell){
    orb=CreatureModel.Part(transform,PrimitiveType.Sphere,Vector3.zero,Vector3.one*.19f,material);orb.name="Mage spell orb";
    var orbTint=new MaterialPropertyBlock();orbTint.SetColor("_Tint",spellColor);orb.GetComponent<Renderer>().SetPropertyBlock(orbTint);orb.gameObject.SetActive(false);
    warning.startColor=warning.endColor=new Color(.25f,.85f,1,.45f);bolt.startColor=bolt.endColor=spellColor;
    return;
   }
   var eye=CreatureModel.Part(agent.visual,PrimitiveType.Sphere,new Vector3(0,.78f,.30f),Vector3.one*.2f,material);eye.name="Prism emitter";var tint=new MaterialPropertyBlock();tint.SetColor("_Tint",new Color(1,.15f,.65f));eye.GetComponent<Renderer>().SetPropertyBlock(tint);
   for(int i=-1;i<=1;i++){var shard=CreatureModel.Part(agent.visual,PrimitiveType.Cube,new Vector3(i*.22f,1.23f,.05f),new Vector3(.10f,.25f,.10f),material);shard.localRotation=Quaternion.Euler(0,0,i*25);shard.GetComponent<Renderer>().SetPropertyBlock(tint);}
  }
  LineRenderer Make(string name,float width,Color color){var go=new GameObject(name);go.transform.SetParent(transform,false);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=material;l.positionCount=2;l.useWorldSpace=true;l.startWidth=l.endWidth=width;l.startColor=l.endColor=color;l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;l.enabled=false;return l;}
  void Update(){
   if(!run||run.IsPaused)return;
   if(agent.Defeated||run.Phase!=SurvivalRun.RunPhase.Wave){Cancel();return;}
   var origin=transform.position+Vector3.up*.8f;
   if(flying){
    float distance=(spell?6.5f:9)*Time.deltaTime;int count=Physics.SphereCastNonAlloc(position,.10f,direction,hits,distance,~0,QueryTriggerInteraction.Ignore);RaycastHit nearest=default;bool hit=false;float best=float.PositiveInfinity;
    for(int i=0;i<count;i++){var c=hits[i].collider;if(c.GetComponentInParent<EnemyAgent>())continue;if(hits[i].distance<best){nearest=hits[i];best=nearest.distance;hit=true;}}
    if(hit){position=nearest.point;var player=nearest.collider.GetComponentInParent<ArenaPlayer>();if(player){var health=player.GetComponent<CharacterVitals>();if(health.Damage(agent.shotDamage,direction))health.ProtectFor(.25f);}run.ActionFX.Burst(position,spellColor,8);run.ActionFX.Cue(2);flying=false;}
    else{position+=direction*distance;travel+=distance;if(travel>16)flying=false;}
    if(orb){orb.gameObject.SetActive(flying);orb.position=position;}bolt.enabled=core.enabled=flying;if(flying){bolt.SetPosition(0,position);bolt.SetPosition(1,position-direction*(spell?.3f:.75f));core.SetPosition(0,position);core.SetPosition(1,position-direction*(spell?.2f:.65f));}return;
   }
   if(charging){
    warning.SetPosition(0,origin);warning.SetPosition(1,origin+(aim-origin).normalized*12);
    if(Time.time<fireAt)return;
    charging=false;warning.enabled=false;agent.AttackWindup=false;direction=(aim-origin).normalized;position=origin;travel=0;flying=true;next=Time.time+3.4f;
    run.ActionFX.Cue(1);run.ActionFX.Burst(origin,spellColor,6);return;
   }
   if(Time.time<next||!agent.target||Vector3.Distance(transform.position,agent.target.position)>10)return;
   aim=agent.target.position+Vector3.up*.8f;
   // Only telegraph a shot when scenery does not block the route; the aim stays fixed while charging.
   var ray=aim-origin;int obstacles=Physics.RaycastNonAlloc(origin,ray.normalized,hits,ray.magnitude,~0,QueryTriggerInteraction.Ignore);
   for(int i=0;i<obstacles;i++)if(!hits[i].collider.GetComponentInParent<EnemyAgent>()&&!hits[i].collider.GetComponentInParent<ArenaPlayer>()){next=Time.time+.5f;return;}
   charging=true;agent.AttackWindup=true;fireAt=Time.time+.9f;warning.SetPosition(0,origin);warning.SetPosition(1,origin+(aim-origin).normalized*12);warning.enabled=true;run.ActionFX.Cue(0);
  }
  void Cancel(){charging=flying=false;if(agent)agent.AttackWindup=false;if(warning)warning.enabled=false;if(bolt)bolt.enabled=false;if(core)core.enabled=false;if(orb)orb.gameObject.SetActive(false);}
  void OnDisable(){Cancel();}void OnDestroy(){if(material)Destroy(material);}
 }
}
