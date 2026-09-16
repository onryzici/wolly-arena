using UnityEngine;
namespace WoollyArena {
public sealed class BreakableProp:MonoBehaviour {
 public int maxHealth=75;public Renderer artwork;public ParticleSystem dust,fragments;public int Health{get;private set;}public bool Broken=>Health<=0;
 Renderer[] visuals;Collider body;MaterialPropertyBlock block;float flashUntil;
 void Awake(){visuals=GetComponentsInChildren<MeshRenderer>();Health=maxHealth;body=GetComponent<Collider>();block=new MaterialPropertyBlock();}
 public void Damage(int amount){if(Broken||amount<=0)return;Health=Mathf.Max(0,Health-amount);flashUntil=Time.time+.10f;if(!Broken)return;body.enabled=false;var obstacle=GetComponent<UnityEngine.AI.NavMeshObstacle>();if(obstacle)obstacle.enabled=false;foreach(var visual in visuals)visual.enabled=false;dust.Play();fragments.Play();Destroy(gameObject,1.8f);}
 void Update(){if(!artwork||Broken)return;block.Clear();if(Time.time<flashUntil)block.SetColor("_BaseColor",new Color(1,.88f,.6f));artwork.SetPropertyBlock(block);}
}
}
