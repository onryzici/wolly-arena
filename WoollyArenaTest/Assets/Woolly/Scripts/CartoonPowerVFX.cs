using UnityEngine;
namespace WoollyArena {
// Compact shock rings and traveling bolts replace the previous screen-filling ribbons.
public sealed class CartoonPowerVFX : MonoBehaviour {
 sealed class Effect { public LineRenderer line,ring;public Vector3 from,to;public float born,size;public int kind;public bool active; }
 readonly Effect[] pool=new Effect[16];Material material;Camera view;int cursor;
 public int ActiveCount {get;private set;}
 public static Color ColorFor(int kind)=>kind==0?new Color(.65f,.38f,1):kind==1?new Color(.25f,.85f,1):new Color(1,.7f,.22f);
 void Awake(){material=new Material(Resources.Load<Shader>("PowerInk"));view=Camera.main;for(int i=0;i<pool.Length;i++)pool[i]=new Effect{line=Line("Arc bolt",9),ring=Line("Impact ring",33)};}
 LineRenderer Line(string name,int count){var go=new GameObject(name);go.transform.SetParent(transform,false);var l=go.AddComponent<LineRenderer>();l.sharedMaterial=material;l.positionCount=count;l.useWorldSpace=true;l.numCapVertices=3;l.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;l.receiveShadows=false;l.enabled=false;return l;}
 public void Play(int kind,Vector3 from,Vector3 to,float size=1){var e=pool[cursor++%pool.Length];e.kind=kind;e.from=from;e.to=to+Vector3.up*.55f;e.size=size;e.born=Time.time;e.active=true;Draw(e,0);}
 void Draw(Effect e,float u){
  Color color=ColorFor(e.kind);color.a=1-u;
  e.line.enabled=u<.45f;e.line.startColor=Color.white;e.line.endColor=color;e.line.widthMultiplier=.065f*(1-u);
  for(int i=0;i<9;i++){float t=i/8f;var point=Vector3.Lerp(e.from,e.to,t);if(e.kind==1&&i>0&&i<8)point+=Vector3.up*Mathf.Sin(i*8.2f)*.12f;e.line.SetPosition(i,point);}
  e.ring.enabled=u>.1f;e.ring.startColor=e.ring.endColor=color;e.ring.widthMultiplier=.09f*(1-u);
  float radius=(.1f+Mathf.Sin(Mathf.Clamp01(u)*Mathf.PI*.5f)*(e.kind==0?1.1f:.5f))*e.size;
  for(int i=0;i<33;i++){float a=i*Mathf.PI/16;Vector3 radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));e.ring.SetPosition(i,e.to-Vector3.up*.48f+radial*radius);}
 }
 void Update(){ActiveCount=0;foreach(var e in pool){if(!e.active)continue;float u=(Time.time-e.born)/.38f;if(u>=1){e.active=false;e.line.enabled=e.ring.enabled=false;}else{Draw(e,u);ActiveCount++;}}}
 public void Clear(){foreach(var e in pool)if(e!=null){e.active=false;e.line.enabled=e.ring.enabled=false;}ActiveCount=0;}
 void OnDestroy(){if(material)Destroy(material);}
}
}
