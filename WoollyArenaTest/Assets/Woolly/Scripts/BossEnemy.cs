using UnityEngine;
namespace WoollyArena {
 public sealed class BossEnemy:MonoBehaviour {
  EnemyAgent agent;bool queen,winding;float nextAttack,impactAt;Vector3 center;LineRenderer ring;Material material;
  const float Radius=2.6f;
  public void Initialize(EnemyAgent owner,bool isQueen){
   agent=owner;queen=isQueen;nextAttack=Time.time+3;
   var go=new GameObject("Boss attack warning");go.transform.SetParent(transform,false);ring=go.AddComponent<LineRenderer>();ring.useWorldSpace=true;ring.positionCount=65;ring.startWidth=ring.endWidth=.075f;
   material=new Material(Resources.Load<Shader>("PowerInk"));ring.sharedMaterial=material;ring.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;ring.enabled=false;
  }
  void Update(){
   var run=agent.director.Run;if(!run||run.IsPaused)return;
   if(agent.Defeated||run.Phase!=SurvivalRun.RunPhase.Wave){ring.enabled=false;agent.AttackWindup=false;enabled=false;return;}
   if(!winding&&Time.time>=nextAttack){winding=true;agent.AttackWindup=true;impactAt=Time.time+1.05f;center=queen?agent.target.position:transform.position;center.y=.065f;ring.enabled=true;}
   if(!winding)return;
   float u=Mathf.Clamp01(1-(impactAt-Time.time)/1.05f);ring.startWidth=ring.endWidth=.06f+u*.10f;
   material.SetColor("_Tint",Color.Lerp(new Color(1,.75f,.2f,.55f),new Color(1,.22f,.15f,.95f),u));
   for(int i=0;i<65;i++){float a=i*Mathf.PI/32;ring.SetPosition(i,center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*Radius);}
   if(Time.time<impactAt)return;
   winding=false;agent.AttackWindup=false;ring.enabled=false;nextAttack=Time.time+(queen?3.8f:4.8f)*(agent.vitals.Health<agent.vitals.maxHealth*.5f?.72f:1);
   var delta=agent.target.position-center;delta.y=0;
   if(delta.sqrMagnitude<Radius*Radius){var vitals=agent.target.GetComponent<CharacterVitals>();if(vitals.Damage(agent.shotDamage+6,delta.normalized))vitals.ProtectFor(.4f);}
   // Fixed pooled spray announces the impact without introducing persistent physics objects.
   run.Rewards.Gore(center,Vector3.forward,true);run.ActionFX.Burst(center+Vector3.up*.2f,queen?new Color(1,.25f,.5f):new Color(.3f,.8f,1),32);run.ActionFX.Cue(2);
   if(Camera.main)Camera.main.GetComponent<ArenaCamera>()?.Punch(.09f);
  }
  void OnDisable(){if(ring)ring.enabled=false;if(agent)agent.AttackWindup=false;}
  void OnDestroy(){if(material)Destroy(material);}
 }
}
