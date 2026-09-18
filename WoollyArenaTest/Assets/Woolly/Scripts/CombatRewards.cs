using UnityEngine;
using TMPro;
namespace WoollyArena {
// Fixed budgets keep long waves from accumulating transient objects.
public sealed class CombatRewards : MonoBehaviour {
 sealed class Bit { public Transform t; public Vector3 velocity; public float until; public int kind,value; public Vector3 scale; public MeshFilter mesh; public Renderer renderer; public int bounces; public bool groundMark,magnetized; public float born; public Vector3 rest; }
 sealed class Number { public TMP_Text text; public Vector3 origin; public float born,size,life,side; }
 public int GoldDropped {get;private set;} public int GoldCollected {get;private set;}
 readonly Bit[] bits=new Bit[408]; readonly Number[] numbers=new Number[64];
 Material[] materials; SurvivalRun run; Camera view; int markCursor,fragmentCursor,lootCursor,numberCursor; Mesh[] shapes; Mesh[] stains; MaterialPropertyBlock stainTint;
 ArenaCamera cameraRig;LineRenderer victoryRing;Material ringMaterial;float ringAt=-100;bool vacuum;
 float nextImpact,nextDeathImpact,nextPickupSound,lastKillAt=-10; int killChain,observedLevel,chestWave,regularChests; CharacterVitals playerVitals;
 public void Initialize(SurvivalRun owner) {
  run=owner;observedLevel=run.Build.Level;playerVitals=run.Player.GetComponent<CharacterVitals>();playerVitals.Damaged+=PlayerHurt;view=Camera.main;stainTint=new MaterialPropertyBlock();cameraRig=view?view.GetComponent<ArenaCamera>():null;
  ringMaterial=new Material(Resources.Load<Shader>("PowerInk"));var ringObject=new GameObject("Wave clear shockwave");ringObject.transform.SetParent(transform,false);victoryRing=ringObject.AddComponent<LineRenderer>();victoryRing.sharedMaterial=ringMaterial;victoryRing.positionCount=65;victoryRing.useWorldSpace=true;victoryRing.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;victoryRing.enabled=false;
  Color[] colors={new Color(.35f,.025f,.06f),new Color(.65f,.06f,.12f),new Color(1,.72f,.12f),new Color(.3f,1,.55f),new Color(.52f,.26f,.08f)};
  materials=new Material[colors.Length];for(int i=0;i<colors.Length;i++){materials[i]=new Material(Resources.Load<Shader>("CombatModel"));materials[i].color=colors[i];}
  materials[0].shader=Resources.Load<Shader>("PowerInk");materials[0].SetColor("_Tint",new Color(.43f,.045f,.055f,.74f));
  stains=new Mesh[7];for(int i=0;i<stains.Length;i++)stains[i]=Splat(i);
  shapes=new[]{stains[0],PrimitiveMesh(PrimitiveType.Sphere),Coin(),Heart(),Chest()};
  for(int i=0;i<bits.Length;i++){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="Pooled combat fragment";go.transform.SetParent(transform);var c=go.GetComponent<Collider>();c.enabled=false;Destroy(c);bits[i]=new Bit{t=go.transform,mesh=go.GetComponent<MeshFilter>(),renderer=go.GetComponent<Renderer>()};go.SetActive(false);}
  var hud=Object.FindAnyObjectByType<ArenaHUD>();
  for(int i=0;i<numbers.Length;i++){var go=new GameObject("Combat number");go.transform.SetParent(transform);var t=go.AddComponent<TextMeshPro>();t.font=hud.ammo.font;t.fontSize=5;t.alignment=TextAlignmentOptions.Center;t.fontStyle=FontStyles.Bold;t.outlineWidth=.22f;t.outlineColor=new Color32(32,18,30,255);t.rectTransform.sizeDelta=new Vector2(4,1);numbers[i]=new Number{text=t};go.SetActive(false);}
 }
 Bit Emit(int kind,Vector3 position,Vector3 scale,float life,Vector3 velocity,int value=0){
  // Never overwrite uncollected loot. Bank it before recycling its slot.
  int slot=kind==0?markCursor++%120:kind==1?120+fragmentCursor++%192:312+lootCursor++%96;var b=bits[slot];if(b.t.gameObject.activeSelf&&b.kind>=2)Collect(b,false);
  if(kind==2||kind==4)GoldDropped+=value;
  b.bounces=0;b.groundMark=false;b.magnetized=false;b.born=Time.time;b.rest=position;b.kind=kind;b.value=value;b.scale=scale;b.velocity=velocity;b.until=Time.time+life;b.t.position=position;b.t.rotation=Quaternion.Euler(0,Random.Range(0,360),0);b.t.localScale=scale;b.renderer.sharedMaterial=materials[kind];b.mesh.sharedMesh=kind==0?stains[Random.Range(0,stains.Length)]:shapes[kind];b.renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;b.renderer.SetPropertyBlock(null);b.t.gameObject.SetActive(true);return b;
 }
 public void Popup(Vector3 point,string label,Color color,float size=1,float life=.7f){
  var n=numbers[numberCursor++%numbers.Length];n.origin=point+Vector3.up*1.35f+new Vector3(Random.Range(-.25f,.25f),0,0);n.born=Time.time;n.size=size;n.life=life;n.side=Random.Range(-.45f,.45f);if(run.ShopUI&&run.ShopUI.FeedbackFont)n.text.font=run.ShopUI.FeedbackFont;n.text.text=label;n.text.color=color;n.text.gameObject.SetActive(true);
 }
 void PlayerHurt(Vector3 direction){run.Sfx.Play(CombatCue.Hurt);if(cameraRig)cameraRig.Punch(.12f);Popup(run.Player.transform.position,"−"+playerVitals.LastDamage,new Color(1,.3f,.25f),1.05f);}
 void LevelFeedback(){if(run.Build.Level<=observedLevel)return;observedLevel=run.Build.Level;run.Sfx.Play(CombatCue.Level);run.ActionFX?.Accent(CombatAccent.LevelUp,run.Player.transform.position);Popup(run.Player.transform.position,"SEVİYE "+observedLevel+"!",new Color(.45f,1,.7f),1.35f,1.1f);}
 public void Hit(Vector3 point,int damage,bool critical,Vector3 direction=default){
  if(critical)run.ActionFX?.Accent(CombatAccent.Critical,point,direction);
  run.ActionFX?.Burst(point+Vector3.up*.65f,critical?new Color(1,.7f,.2f):new Color(.7f,.9f,1),critical?12:4);
  Popup(point,damage.ToString()+(critical?"!":""),critical?new Color(1,.74f,.16f):Color.white,critical?1.4f:1,critical?.85f:.65f);
  direction.y=0;if(direction.sqrMagnitude<.01f)direction=Vector3.forward;direction.Normalize();
  int count=critical?7:3;
  for(int i=0;i<count;i++){var b=Emit(1,point+Vector3.up*.65f,new Vector3(.035f,.035f,Random.Range(.1f,.2f)),.7f,direction*Random.Range(2,4)+Random.insideUnitSphere*.7f+Vector3.up*1.3f);b.groundMark=critical&&i==0;}
  if(Time.time>=nextImpact){nextImpact=Time.time+.12f;if(cameraRig)cameraRig.Punch(critical?.08f:.025f);run.Sfx.Play(critical?CombatCue.Critical:CombatCue.Hit,point);}
 }
 public void Gore(Vector3 point,Vector3 direction,bool finishing=false){
  direction.y=0;if(direction.sqrMagnitude<.01f)direction=Random.insideUnitSphere;direction.y=0;direction.Normalize();
  Emit(0,point+Vector3.up*Random.Range(.026f,.033f),new Vector3(Random.Range(.8f,1.2f),1,Random.Range(.5f,.9f)),30,Vector3.zero);
  for(int i=0;i<5;i++){var offset=direction*(.2f+i*.17f)+Random.insideUnitSphere*.18f;float size=Random.Range(.07f,.22f);Emit(0,new Vector3(point.x+offset.x,.037f,point.z+offset.z),new Vector3(size,1,size*.8f),25,Vector3.zero);}
  int count=finishing?5:10;
  for(int i=0;i<count;i++){
   float size=Random.Range(.065f,.16f);var b=Emit(1,point+Vector3.up*.5f,new Vector3(size,size*.8f,size*1.6f),Random.Range(1.1f,1.9f),direction*Random.Range(1,3)+Random.insideUnitSphere*1.8f+Vector3.up*Random.Range(2,4));b.groundMark=i<3;
  }
  if(!finishing&&Time.time>=nextDeathImpact){run.Sfx.Play(CombatCue.Death,point);nextDeathImpact=Time.time+.12f;if(cameraRig)cameraRig.Punch(.085f);}
 }
 public void BeginVictorySweep(){vacuum=true;ringAt=Time.time;if(cameraRig)cameraRig.Punch(.14f);}
 public void BossReward(Vector3 point){run.ActionFX?.Accent(CombatAccent.BossDefeat,point);Emit(4,point+Vector3.up*.3f,Vector3.one*.8f,120,Vector3.zero,RunBalance.BossGold(run.Wave));Popup(point,"BOSS YENİLDİ!",new Color(1,.8f,.25f),1.4f,1.3f);}
 public void Death(Vector3 point,Vector3 direction=default){
  run.ActionFX?.Accent(CombatAccent.Defeat,point,direction);
  Gore(point,direction);
  killChain=Time.time-lastKillAt<1.5f?killChain+1:1;lastKillAt=Time.time;
  if(killChain==5||killChain==10||killChain==20||killChain%25==0)Popup(point,killChain+" SERİ!",new Color(1,.55f,.22f),1.25f,1);
  LevelFeedback();
  if(Random.value<RunBalance.GoldChance(run.Wave))Emit(2,point+Vector3.up*.18f,new Vector3(.26f,.26f,.26f),120,Vector3.zero,1);
  if(chestWave!=run.Wave){chestWave=run.Wave;regularChests=0;}
  if(regularChests<RunBalance.ChestLimit&&Random.value<RunBalance.ChestChance){regularChests++;var b=Emit(4,point+new Vector3(-.3f,.25f,0),Vector3.one*.6f,120,Vector3.zero,RunBalance.ChestGold(run.Wave));Popup(b.t.position,"SANDIK!",new Color(1,.78f,.2f));}
 }
 void Collect(Bit b,bool show){
  if(show&&b.kind>=2&&b.kind!=3)run.ActionFX?.Accent(CombatAccent.Pickup,b.t.position);
  if(b.kind==3){b.t.gameObject.SetActive(false);return;}
  else {
   GoldCollected+=b.value;run.Build.AddMaterials(b.value);
   if(b.kind==4){if(show){Popup(b.t.position,"+"+b.value+"  SANDIK",new Color(1,.8f,.2f),1.35f,1.1f);run.Sfx.Play(CombatCue.Chest);LevelFeedback();}}
   else if(show){Popup(b.t.position,"+"+b.value,new Color(1,.8f,.2f),.85f,.55f);if(Time.time>=nextPickupSound){nextPickupSound=Time.time+.065f;run.Sfx.Play(CombatCue.Coin);}}
  }
  b.t.gameObject.SetActive(false);
 }
 public void FinishWave(bool bank){vacuum=false;foreach(var b in bits)if(b!=null&&b.t.gameObject.activeSelf&&b.kind>=2){if(bank)Collect(b,false);else b.t.gameObject.SetActive(false);}}
 void Update(){if(!run||run.IsPaused)return;
  float ringAge=Time.time-ringAt;victoryRing.enabled=ringAge<.7f;
  if(victoryRing.enabled){float u=Mathf.Clamp01(ringAge/.7f);victoryRing.startWidth=victoryRing.endWidth=.15f*(1-u);victoryRing.startColor= victoryRing.endColor=new Color(1,.65f,.22f,1-u);for(int i=0;i<65;i++){float angle=i*Mathf.PI/32;victoryRing.SetPosition(i,run.Player.transform.position+new Vector3(Mathf.Cos(angle)*u*18,.05f,Mathf.Sin(angle)*u*18));}}

  foreach(var n in numbers){if(!n.text.gameObject.activeSelf)continue;float u=(Time.time-n.born)/n.life;if(u>=1){n.text.gameObject.SetActive(false);continue;}n.text.transform.position=n.origin+Vector3.up*u*.95f+(view?view.transform.right:Vector3.right)*(n.side*u);n.text.transform.localScale=Vector3.one*n.size*(u<.15f?Mathf.Lerp(.6f,1.2f,u/.15f):Mathf.Lerp(1.2f,.8f,u));n.text.alpha=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.5f,1,u));if(view)n.text.transform.rotation=view.transform.rotation;}
  foreach(var b in bits){if(!b.t.gameObject.activeSelf)continue;if(b.kind>=2){
    if(run.Phase!=SurvivalRun.RunPhase.Wave&&!vacuum)continue;
    float age=Time.time-b.born;var target=run.Player.transform.position+Vector3.up*.3f;
    bool useful=b.kind!=3||playerVitals.Health<playerVitals.maxHealth;
    if(!vacuum&&age<.28f){float u=age/.28f;b.t.position=b.rest+Vector3.up*(Mathf.Sin(u*Mathf.PI)*.55f);b.t.localScale=b.scale*Mathf.Lerp(.3f,1,u);}
    else {
     float d=Vector3.Distance(b.t.position,target);
     if(vacuum||(useful&&d<2.5f))b.magnetized=true;
     if(b.magnetized&&(useful||vacuum)){
      if(d<.55f){Collect(b,true);continue;}
      b.t.position=Vector3.MoveTowards(b.t.position,target,Time.deltaTime*(vacuum?18:Mathf.Lerp(15,5,Mathf.Clamp01(d/2.5f))));
     }else{b.magnetized=false;b.t.position=b.rest+Vector3.up*(.045f+Mathf.Sin(age*4)*.045f);}
     b.t.localScale=b.scale*(1+.055f*Mathf.Sin(age*5));
    }
    b.t.Rotate(0,90*Time.deltaTime,0);
   }
   else if(b.kind==1){
    if(b.bounces<2){b.velocity+=Vector3.down*12*Time.deltaTime;b.t.position+=b.velocity*Time.deltaTime;b.t.Rotate(240*Time.deltaTime,170*Time.deltaTime,0);
     if(b.t.position.y<.05f){b.t.position=new Vector3(b.t.position.x,.05f,b.t.position.z);b.velocity=new Vector3(b.velocity.x*.45f,Mathf.Abs(b.velocity.y)*.3f,b.velocity.z*.45f);b.bounces++;
      if(b.groundMark){b.groundMark=false;Emit(0,b.t.position-Vector3.up*.01f,new Vector3(.16f,1,.12f),18,Vector3.zero);}
     }
    }
   }

   if(Time.time>b.until){if(b.kind>=2)Collect(b,false);else b.t.gameObject.SetActive(false);}else if(b.kind==0){stainTint.SetColor("_Tint",new Color(.43f,.045f,.055f,.74f*Mathf.Clamp01((b.until-Time.time)/4)));b.renderer.SetPropertyBlock(stainTint);}else if(b.kind==1)b.t.localScale=b.scale*Mathf.Clamp01((b.until-Time.time)*3);
  }
 }
 static Mesh PrimitiveMesh(PrimitiveType type){var g=GameObject.CreatePrimitive(type);var mesh=g.GetComponent<MeshFilter>().sharedMesh;g.SetActive(false);Destroy(g);return mesh;}
 static Mesh Combine(params CombineInstance[] parts){var mesh=new Mesh();mesh.CombineMeshes(parts);return mesh;}
 static CombineInstance Piece(Mesh mesh,Vector3 p,Vector3 scale,Quaternion rotation)=>new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(p,rotation,scale)};
 static Mesh Coin(){var cylinder=PrimitiveMesh(PrimitiveType.Cylinder);return Combine(Piece(cylinder,Vector3.zero,new Vector3(1,.16f,1),Quaternion.Euler(90,0,0)));}
 static Mesh Heart(){var sphere=PrimitiveMesh(PrimitiveType.Sphere);var cube=PrimitiveMesh(PrimitiveType.Cube);return Combine(Piece(sphere,new Vector3(-.22f,.16f,0),new Vector3(.65f,.65f,.35f),Quaternion.identity),Piece(sphere,new Vector3(.22f,.16f,0),new Vector3(.65f,.65f,.35f),Quaternion.identity),Piece(cube,new Vector3(0,-.08f,0),new Vector3(.6f,.6f,.27f),Quaternion.Euler(0,0,45)));}
 static Mesh Chest(){var cube=PrimitiveMesh(PrimitiveType.Cube);return Combine(Piece(cube,Vector3.zero,new Vector3(1,.6f,.7f),Quaternion.identity),Piece(cube,new Vector3(0,.35f,0),new Vector3(1.06f,.18f,.76f),Quaternion.identity),Piece(cube,new Vector3(0,.12f,.39f),new Vector3(.2f,.25f,.12f),Quaternion.identity),Piece(cube,new Vector3(-.34f,0,0),new Vector3(.1f,.7f,.76f),Quaternion.identity),Piece(cube,new Vector3(.34f,0,0),new Vector3(.1f,.7f,.76f),Quaternion.identity));}
 static Mesh Splat(int variant){
  const int count=64;var vertices=new Vector3[count*2+1];var colors=new Color[vertices.Length];var triangles=new int[count*9];colors[0]=Color.white;
  for(int i=0;i<count;i++){
   float a=i*Mathf.PI*2/count;
   float r=.45f+.045f*Mathf.Sin(a*3+variant*1.7f)+.035f*Mathf.Cos(a*5-variant*.8f)+.018f*Mathf.Sin(a*9+variant);
   var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));vertices[1+i]=direction*r*.9f;vertices[1+count+i]=direction*r;
   colors[1+i]=new Color(1,1,1,.95f);colors[1+count+i]=new Color(1,1,1,0);
   int next=(i+1)%count,k=i*9;triangles[k]=0;triangles[k+1]=1+next;triangles[k+2]=1+i;
   triangles[k+3]=1+i;triangles[k+4]=1+next;triangles[k+5]=1+count+next;
   triangles[k+6]=1+i;triangles[k+7]=1+count+next;triangles[k+8]=1+count+i;
  }
  var mesh=new Mesh{vertices=vertices,colors=colors,triangles=triangles};mesh.RecalculateNormals();return mesh;
 }

 void OnDestroy(){if(playerVitals)playerVitals.Damaged-=PlayerHurt;if(ringMaterial)Destroy(ringMaterial);if(shapes!=null)for(int i=0;i<shapes.Length;i++)if(i>1&&shapes[i])Destroy(shapes[i]);if(stains!=null)foreach(var stain in stains)if(stain)Destroy(stain);if(materials!=null)foreach(var m in materials)if(m)Destroy(m);}
}
}
