using UnityEngine;
namespace WoollyArena {
public sealed class BreakableProp:MonoBehaviour {
 public int maxHealth=75;public Renderer artwork;public ParticleSystem dust,fragments;public int Health{get;private set;}public bool Broken=>Health<=0;
 Renderer[] visuals;Collider body;MaterialPropertyBlock block;float flashUntil;
 void Awake(){visuals=GetComponentsInChildren<MeshRenderer>();Health=maxHealth;body=GetComponent<Collider>();block=new MaterialPropertyBlock();if(dust){var main=dust.main;main.scalingMode=ParticleSystemScalingMode.Shape;main.maxParticles=5;main.startLifetime=.22f;main.startSize=.12f;main.startColor=new Color(.75f,.55f,.32f,.16f);var renderer=dust.GetComponent<ParticleSystemRenderer>();renderer.minParticleSize=0;renderer.maxParticleSize=.018f;}}
 public void ReplaceArtwork(Transform model){foreach(var r in visuals)r.enabled=false;visuals=model.GetComponentsInChildren<Renderer>();artwork=visuals.Length>0?visuals[0]:null;}
 public void Damage(int amount){if(Broken||amount<=0)return;Health=Mathf.Max(0,Health-amount);flashUntil=Time.time+.10f;if(!Broken)return;body.enabled=false;var obstacle=GetComponent<UnityEngine.AI.NavMeshObstacle>();if(obstacle)obstacle.enabled=false;foreach(var visual in visuals)visual.enabled=false;dust.Play();fragments.Play();Destroy(gameObject,1.8f);}
 void Update(){if(!artwork||Broken)return;block.Clear();if(Time.time<flashUntil)block.SetColor("_BaseColor",new Color(1,.88f,.6f));artwork.SetPropertyBlock(block);}
}
}
