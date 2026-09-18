using UnityEngine;
namespace WoollyArena {
public sealed class ShotEffects:MonoBehaviour {
 public Material tracerMaterial,sparkMaterial;
 const int Count=24;LineRenderer[] cores,halos;float[] born,duration;Vector3[] starts,ends,normals;bool[] impact,hostile;int cursor;ParticleSystem flash,sparks,smoke;Material muzzleMaterial,impactMaterial,smokeMaterial;float nextSmoke;
 static readonly int BaseColor=Shader.PropertyToID("_BaseColor");MaterialPropertyBlock tint;
 void Awake(){
  cores=new LineRenderer[Count];halos=new LineRenderer[Count];born=new float[Count];duration=new float[Count];starts=new Vector3[Count];ends=new Vector3[Count];normals=new Vector3[Count];impact=new bool[Count];hostile=new bool[Count];tint=new MaterialPropertyBlock();
  for(int i=0;i<Count;i++){cores[i]=Line("Bullet core "+i);halos[i]=Line("Bullet rim "+i);cores[i].sortingOrder=2;halos[i].sortingOrder=1;}
  muzzleMaterial=Textured("Muzzle");impactMaterial=Textured("Spark");
  flash=Particles("Muzzle flash",24,muzzleMaterial,false);sparks=Particles("Impact sparks",96,impactMaterial,false);
  smokeMaterial=Textured("Smoke");smoke=Particles("Small barrel puffs",8,smokeMaterial,true);
 }
 Material Textured(string name){var m=new Material(Resources.Load<Shader>("CombatParticle"));m.SetTexture("_BaseMap",Resources.Load<Texture2D>("VFX/"+name));return m;}
 LineRenderer Line(string name){var go=new GameObject(name);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=tracerMaterial;line.positionCount=2;line.numCapVertices=5;line.useWorldSpace=true;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;line.enabled=false;return line;}
 ParticleSystem Particles(string name,int count,Material material,bool grows){
  var g=new GameObject(name);g.transform.SetParent(transform,false);var p=g.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=p.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.scalingMode=ParticleSystemScalingMode.Shape;main.maxParticles=count;main.startSpeed=0;var emission=p.emission;emission.enabled=false;var shape=p.shape;shape.enabled=false;
  var col=p.colorOverLifetime;col.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.85f,.25f),new GradientAlphaKey(0,1)});col.color=gradient;
  var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,grows?.6f:1),new Keyframe(1,grows?1.8f:.15f)));var rr=p.GetComponent<ParticleSystemRenderer>();rr.sharedMaterial=material;rr.enabled=material.GetTexture("_BaseMap")!=null;rr.minParticleSize=0;rr.maxParticleSize=grows?.008f:.025f;rr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;p.Play();return p;
 }
 void Tint(LineRenderer line,Color color){tint.Clear();tint.SetColor(BaseColor,color);line.SetPropertyBlock(tint);}
 public void Play(Vector3 origin,Vector3 end,bool impacted=false,Vector3 normal=default,bool enemyShot=false){
  int i=cursor;cursor=(cursor+1)%Count;starts[i]=origin;ends[i]=end;normals[i]=normal;impact[i]=impacted;hostile[i]=enemyShot;born[i]=Time.time;duration[i]=Mathf.Clamp(Vector3.Distance(origin,end)/55,.055f,.38f);
  Tint(cores[i],enemyShot?new Color(1,.87f,.7f):new Color(1,1,.72f));Tint(halos[i],enemyShot?new Color(.96f,.22f,.12f):new Color(1,.59f,.065f));cores[i].enabled=halos[i].enabled=true;Draw(i,.025f);
  var direction=(end-origin).normalized;var muzzle=new ParticleSystem.EmitParams{position=origin+direction*.055f,startSize=.48f,startLifetime=.075f,startColor=enemyShot?new Color(1,.4f,.12f):new Color(1,.77f,.18f),rotation=Random.Range(0,360)};flash.Emit(muzzle,1);
  if(!enemyShot&&Time.time>=nextSmoke){nextSmoke=Time.time+.12f;
   smoke.Emit(new ParticleSystem.EmitParams{position=origin+direction*.08f,velocity=direction*.12f+Vector3.up*.22f,startSize=.075f,startLifetime=.26f,startColor=new Color(.82f,.76f,.65f,.16f),rotation=Random.Range(0,360)},1);
  }
 }
 void Draw(int i,float u){var delta=ends[i]-starts[i];float length=delta.magnitude;float head=Mathf.Clamp01(u);float tail=Mathf.Max(0,head-.72f/Mathf.Max(length,.001f));var a=starts[i]+delta*tail;var b=starts[i]+delta*head;
  cores[i].SetPosition(0,a);cores[i].SetPosition(1,b);halos[i].SetPosition(0,a);halos[i].SetPosition(1,b);
  // Set actual widths, rather than resetting widthMultiplier to a one-metre trail.
  cores[i].startWidth=.045f;cores[i].endWidth=.085f;halos[i].startWidth=.09f;halos[i].endWidth=.14f;
 }
 void Impact(int i){
  var color=hostile[i]?new Color(1,.42f,.18f):new Color(1,.78f,.24f);var origin=ends[i]+normals[i]*.035f;
  var pop=new ParticleSystem.EmitParams{position=origin,startSize=.38f,startLifetime=.12f,startColor=color};sparks.Emit(pop,1);
  for(int k=0;k<5;k++){var e=new ParticleSystem.EmitParams{position=origin,velocity=normals[i]*Random.Range(.7f,1.3f)+Random.insideUnitSphere*1.2f,startSize=Random.Range(.08f,.16f),startLifetime=Random.Range(.13f,.22f),startColor=color,rotation=Random.Range(0,360)};sparks.Emit(e,1);}
 }
 void Update(){for(int i=0;i<Count;i++)if(cores[i].enabled){float u=(Time.time-born[i])/duration[i];if(u>=1){cores[i].enabled=halos[i].enabled=false;if(impact[i])Impact(i);impact[i]=false;}else Draw(i,u);}}
 void OnDestroy(){if(smokeMaterial)Destroy(smokeMaterial);if(muzzleMaterial)Destroy(muzzleMaterial);if(impactMaterial)Destroy(impactMaterial);}
}
}
