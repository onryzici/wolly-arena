using UnityEngine;
namespace WoollyArena {
public sealed class TargetDummy:MonoBehaviour {
 public int Health {get;private set;}=100; public int Knockdowns {get;private set;} public Transform healthFill;
 Renderer[] renderers;MaterialPropertyBlock block;float flashUntil,resetAt;Collider body;
 void Awake(){renderers=GetComponentsInChildren<Renderer>();block=new MaterialPropertyBlock();body=GetComponent<Collider>();}
 public void Damage(int amount){if(Health==0)return;Health=Mathf.Max(0,Health-amount);flashUntil=Time.time+.1f;if(Health==0){Knockdowns++;resetAt=Time.time+1.5f;body.enabled=false;}Refresh();}
 void Refresh(){if(healthFill)healthFill.localScale=new Vector3(.8f*Health/100f,.07f,.08f);}
 void Update(){if(Health==0&&Time.time>=resetAt){Health=100;body.enabled=true;Refresh();}foreach(var r in renderers){block.Clear();if(Time.time<flashUntil)block.SetColor("_BaseColor",Color.white);else if(Health==0)block.SetColor("_BaseColor",new Color(.15f,.18f,.22f));r.SetPropertyBlock(block);}}
}
}
