using UnityEngine;
namespace WoollyArena {
public sealed class CreatureModel : MonoBehaviour {
 EnemyAgent agent; Transform[] joints; Quaternion[] rest; Vector3[] restPositions; Vector3 previous; float stride, gait; CraftedModel crafted; Material skin,dark,eye;Transform body;int kind;float phase;Vector3 size; float flashUntil,nextFlash; CharacterVitals vitals; Color skinColor;
 public void Initialize(EnemyAgent bot,int variant){
  agent=bot;previous=bot.transform.position;kind=variant;phase=Random.value*6;foreach(var r in bot.visual.GetComponentsInChildren<Renderer>())r.enabled=false;
  skin=new Material(Resources.Load<Shader>("CombatModel"));dark=new Material(skin);eye=new Material(skin);
  skin.color=kind==0?new Color(.48f,.7f,.24f):kind==1?new Color(.65f,.22f,.32f):kind==2?new Color(.34f,.24f,.56f):new Color(.69f,.48f,.27f);dark.color=new Color(.09f,.065f,.12f);eye.color=new Color(1,.85f,.39f);
  skinColor=skin.color;vitals=bot.vitals;vitals.Damaged+=Flash;
  body=new GameObject("Creature silhouette").transform;body.SetParent(bot.visual.Find("Hit reaction pivot") ?? bot.visual,false);
  body.localScale=Vector3.one*.7f;bot.UseSmallCreatureBody();
  bot.nameLabel.gameObject.SetActive(false);bot.healthLabel.gameObject.SetActive(false);
  bot.nameplate.transform.localPosition=Vector3.up*1.25f;
  if(KayKitEnemy.Attach(bot,kind,body))return;
  string[] models={"MossMaw","SpineRaptor","HornBrute","StitchReaper","FrostWarden","ThornMatriarch"};
  var imported=CraftedModel.Attach(models[Mathf.Clamp(kind,0,5)],body);
  if(imported){crafted=imported.GetComponent<CraftedModel>();joints=imported.GetComponentsInChildren<Transform>();rest=new Quaternion[joints.Length];restPositions=new Vector3[joints.Length];for(int i=0;i<joints.Length;i++){rest[i]=joints[i].localRotation;restPositions[i]=joints[i].localPosition;}return;}
  size=kind==0?new Vector3(.8f,.65f,.7f):kind==1?new Vector3(.55f,.8f,.65f):kind==2?new Vector3(1.1f,1.25f,.85f):new Vector3(.7f,1,.45f);
  Part(body,PrimitiveType.Sphere,new Vector3(0,.7f,0),size,skin);
  for(int s=-1;s<=1;s+=2){Part(body,PrimitiveType.Sphere,new Vector3(s*.16f,.88f,.33f),new Vector3(.21f,.23f,.12f),eye);Part(body,PrimitiveType.Sphere,new Vector3(s*.16f,.88f,.395f),new Vector3(.07f,.12f,.04f),dark);
   Part(body,PrimitiveType.Cube,new Vector3(s*.23f,.18f,.04f),new Vector3(.2f,.23f,.34f),dark);
   var horn=Part(body,PrimitiveType.Capsule,new Vector3(s*.32f,1.18f,0),new Vector3(.13f,.3f,.13f),kind==3?dark:skin);horn.localRotation=Quaternion.Euler(0,0,-s*28);
   var arm=Part(body,PrimitiveType.Capsule,new Vector3(s*(kind==2?.58f:.39f),.65f,.04f),new Vector3(.18f,kind==2?.4f:.24f,.2f),skin);arm.localRotation=Quaternion.Euler(0,0,s*30);
  }
  Part(body,PrimitiveType.Cube,new Vector3(0,.63f,.37f),new Vector3(.27f,.08f,.06f),dark);
  if(kind==1)for(int i=0;i<4;i++)Part(body,PrimitiveType.Cube,new Vector3(0,.5f+i*.17f,-.35f),new Vector3(.13f,.15f,.35f),dark);
  if(kind==3){Part(body,PrimitiveType.Cube,new Vector3(0,.7f,.4f),new Vector3(.07f,.5f,.05f),dark);Part(body,PrimitiveType.Cube,new Vector3(0,.7f,.41f),new Vector3(.4f,.06f,.05f),dark);}
 }
 public static Transform Part(Transform parent,PrimitiveType type,Vector3 p,Vector3 scale,Material material){var go=GameObject.CreatePrimitive(type);go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;}
 void Flash(Vector3 direction){if(Time.time<nextFlash)return;flashUntil=Time.time+.045f;nextFlash=Time.time+.18f;}
 void LateUpdate(){
  if(!agent||agent.Defeated||!agent.enabled||(agent.director&&agent.director.Run&&agent.director.Run.IsPaused))return;
  if(crafted)crafted.SetFlash(Time.time<flashUntil?.4f:0);skin.color=Time.time<flashUntil?Color.white:skinColor;
  float distance=Vector3.Distance(agent.transform.position,previous);previous=agent.transform.position;
  float speed=Time.deltaTime>0?distance/Time.deltaTime:0;
  gait=Mathf.MoveTowards(gait,Mathf.Clamp01(speed/1.5f),Time.deltaTime*8);
  stride+=Mathf.Min(distance,.12f)*(kind==1?11:8);float step=Mathf.Sin(stride);
  float attack=agent.MeleeAttackProgress;float swing=attack<0?0:attack<.55f?-attack/.55f:Mathf.Sin((attack-.55f)/.45f*Mathf.PI)*1.5f;
  if(joints==null)return;
  for(int i=0;i<joints.Length;i++){
   string n=joints[i].name;float x=0,y=0,z=0;
   if(n=="LegL"||n=="LegR")x=step*(n=="LegL"?1:-1)*32*gait;
   else if(n=="ArmL"||n=="ArmR")x=-step*(n=="ArmL"?1:-1)*24*gait-swing*65;
   else if(n.StartsWith("Spider")){int side=n.Contains("L")?1:-1;float offset=n.EndsWith("1")?Mathf.PI:0;y=Mathf.Sin(stride+offset)*side*16*gait;z=Mathf.Max(0,Mathf.Cos(stride+offset)*side)*12*gait;}
   else if(n=="Torso"){x=6*gait+swing*12;z=step*2*gait;}
   else if(n=="Head"){x=-3*gait-swing*7;y=Mathf.Sin(Time.time*1.5f+phase)*2*(1-gait);}
   // FBX meshes retain Blender local axes (X right, Y depth, Z up).
   joints[i].localRotation=rest[i]*Quaternion.Euler(x,z,y);
   joints[i].localPosition=restPositions[i];
   if(n=="LegL"||n=="LegR"){
    float lift=(kind==2||kind==4?.58f:.42f)*(1-Mathf.Cos(x*Mathf.Deg2Rad))+Mathf.Max(0,Mathf.Cos(stride)*(n=="LegL"?1:-1))*.10f*gait;
    joints[i].position+=Vector3.up*lift*body.lossyScale.y;
   }
  }
 }
 void OnDestroy(){if(vitals)vitals.Damaged-=Flash;Destroy(skin);Destroy(dark);Destroy(eye);}
}
}
