using UnityEngine;
namespace WoollyArena {
 // Bounded spark/beam pools; music is local to the run and honors the existing sound setting.
 public sealed class ArenaActionFX:MonoBehaviour {
  sealed class Flash {public LineRenderer line;public Vector3 position,velocity;public float life,maxLife;public Color color;}
  sealed class Impact {public Transform root;public MeshRenderer face;public LineRenderer ring;public float life;public Color color;public float size;}
  readonly Impact[] impacts=new Impact[24];int impactCursor;Mesh impactMesh;MaterialPropertyBlock tint;Camera view;
  readonly Flash[] sparks=new Flash[96];int cursor;Material ink,impactInk;AudioSource music;SurvivalRun run;
  CombatAccentFX accents;
  public CombatAccentFX Accents=>accents;

  public void Initialize(SurvivalRun owner){
   run=owner;view=Camera.main;tint=new MaterialPropertyBlock();ink=new Material(Resources.Load<Shader>("PowerInk"));ink.SetColor("_Tint",Color.white);
   accents=gameObject.AddComponent<CombatAccentFX>();accents.Initialize(owner);
   for(int i=0;i<sparks.Length;i++)sparks[i]=new Flash{line=Line("Impact spark",.055f)};
   impactMesh=MakeImpactMesh();impactInk=new Material(Resources.Load<Shader>("CombatParticle"));impactInk.SetTexture("_BaseMap",Resources.Load<Texture2D>("VFX/Impact"));
   for(int i=0;i<impacts.Length;i++){
    var go=new GameObject("Comic impact");go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=impactMesh;
    var face=go.AddComponent<MeshRenderer>();face.sharedMaterial=impactInk;face.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;face.receiveShadows=false;face.enabled=false;
    var ring=Line("Impact shock ring",.04f);ring.positionCount=25;ring.loop=true;ring.useWorldSpace=false;ring.transform.SetParent(go.transform,false);
    for(int j=0;j<25;j++){float a=j*Mathf.PI*2/25;ring.SetPosition(j,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0));}
    impacts[i]=new Impact{root=go.transform,face=face,ring=ring};
   }
   music=gameObject.AddComponent<AudioSource>();music.clip=Resources.Load<AudioClip>("Audio/RoboWestern");music.loop=true;music.playOnAwake=false;music.spatialBlend=0;music.volume=0;if(music.clip)music.Play();

  }
  LineRenderer Line(string name,float width){var obj=new GameObject(name);obj.transform.SetParent(transform,false);var line=obj.AddComponent<LineRenderer>();line.sharedMaterial=ink;line.positionCount=2;line.useWorldSpace=true;line.startWidth=line.endWidth=width;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.enabled=false;return line;}
  public void Cue(int kind){run.Sfx.Play(kind==0?CombatCue.Charge:kind==1?CombatCue.Laser:kind==2?CombatCue.Power:CombatCue.Wave);}
  public void Accent(CombatAccent kind,Vector3 at,Vector3 direction=default){if(accents)accents.Play(kind,at,direction);}

  public void Burst(Vector3 at,Color color,int count=12){
   var hit=impacts[impactCursor++%impacts.Length];hit.root.position=at;hit.life=.28f;hit.color=color;hit.size=count>=20?.8f:count>=10?.48f:.27f;hit.root.localScale=Vector3.one*hit.size;hit.face.enabled=true;hit.ring.enabled=true;
   if(view)hit.root.rotation=view.transform.rotation*Quaternion.Euler(0,0,Random.Range(0,360));
   tint.SetColor("_Tint",Color.white);hit.face.SetPropertyBlock(tint);
   for(int i=0;i<Mathf.Min(count,32);i++){var f=sparks[cursor++%sparks.Length];f.position=at;f.velocity=Random.onUnitSphere*Random.Range(1.5f,4);f.life=f.maxLife=Random.Range(.16f,.4f);f.color=color;f.line.SetPosition(0,at);f.line.SetPosition(1,at);f.line.enabled=true;}
  }
  void Update(){
   if(!run)return;float target=run.IsPaused?.035f:run.Phase==SurvivalRun.RunPhase.Wave?(run.BossAlive?.32f:.26f):run.Phase==SurvivalRun.RunPhase.Upgrade||run.Phase==SurvivalRun.RunPhase.LevelUp?.09f:0;
   if(Time.unscaledTime-run.Sfx.LastHeavySound<.22f)target*=.65f;
   music.volume=Mathf.MoveTowards(music.volume,target,Time.unscaledDeltaTime*.25f);
   if(run.IsPaused)return;
   foreach(var h in impacts){if(h.life<=0)continue;h.life-=Time.deltaTime;float age=1-Mathf.Clamp01(h.life/.28f);h.face.enabled=h.life>.15f;h.ring.enabled=h.life>0;
    h.root.localScale=Vector3.one*h.size*Mathf.Lerp(.65f,1.8f,age);var color=Color.Lerp(Color.white,h.color,Mathf.Clamp01(age*4));color.a=1-age;
    tint.SetColor("_Tint",color);h.face.SetPropertyBlock(tint);h.ring.startColor=h.ring.endColor=new Color(h.color.r,h.color.g,h.color.b,(1-age)*.65f);h.ring.widthMultiplier=.035f*(1-age);
   }
   foreach(var f in sparks){if(f.life<=0)continue;f.life-=Time.deltaTime;f.line.enabled=f.life>0;if(f.life<=0)continue;f.position+=f.velocity*Time.deltaTime;f.velocity+=Vector3.down*4*Time.deltaTime;var c=f.color;c.a=f.life/f.maxLife;f.line.startColor=f.line.endColor=c;f.line.SetPosition(0,f.position);f.line.SetPosition(1,f.position-f.velocity*.045f);}
  }
  static Mesh MakeImpactMesh(){
   var mesh=new Mesh{name="Textured impact card"};mesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};
   mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};mesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();return mesh;
  }
  void OnDestroy(){if(ink)Destroy(ink);if(impactMesh)Destroy(impactMesh);if(impactInk)Destroy(impactInk);}
 }
}
