using UnityEngine;
namespace WoollyArena {
 public enum CombatCue { Revolver,Repeater,Shotgun,Hit,Critical,Death,Hurt,Laser,Charge,Power,Coin,Chest,Wave,Level,Whoosh,Step,Click }
 // Fixed voices, per-event throttling and no repeated adjacent samples.
 public sealed class CombatAudio:MonoBehaviour {
  sealed class Bank {public AudioClip[] clips;public float gain,cooldown,next;public int priority,last=-1;}
  readonly Bank[] banks=new Bank[17];readonly AudioSource[] voices=new AudioSource[14];SurvivalRun run;bool paused;
  Vector3 lastPosition;float walked;public float LastHeavySound {get;private set;}=-100;
  public void Initialize(SurvivalRun owner){run=owner;lastPosition=run.Player.transform.position;
   Add(CombatCue.Revolver,.44f,.055f,60,"Revolver0","Revolver1");Add(CombatCue.Repeater,.30f,.06f,70,"Repeater0","Repeater1");Add(CombatCue.Shotgun,.53f,.1f,45,"Shotgun0","Shotgun1");
   Add(CombatCue.Hit,.32f,.085f,100,"Hit0","Hit1","Hit2");Add(CombatCue.Critical,.48f,.12f,50,"Critical");Add(CombatCue.Death,.28f,.18f,85,"Explosion");Add(CombatCue.Hurt,.62f,.25f,10,"Critical","Hit2");
   Add(CombatCue.Laser,.36f,.09f,55,"Laser0","Laser1");Add(CombatCue.Charge,.23f,.22f,80,"Charge");Add(CombatCue.Power,.38f,.13f,50,"Power0","Power1");
   Add(CombatCue.Coin,.17f,.11f,170,"Coin0","Coin1");Add(CombatCue.Chest,.45f,.25f,75,"Chest");Add(CombatCue.Wave,.30f,.5f,30,"Power1");Add(CombatCue.Level,.38f,.5f,25,"Coin1");
   Add(CombatCue.Whoosh,.34f,.12f,120,"Whoosh");Add(CombatCue.Step,.10f,.19f,220,"Step0","Step1");Add(CombatCue.Click,.28f,.07f,140,"Click");
   for(int i=0;i<voices.Length;i++){var go=new GameObject("SFX voice "+i);go.transform.SetParent(transform,false);var a=go.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=0;a.dopplerLevel=0;voices[i]=a;}
  }
  void Add(CombatCue cue,float gain,float cooldown,int priority,params string[] names){var b=new Bank{clips=new AudioClip[names.Length],gain=gain,cooldown=cooldown,priority=priority};for(int i=0;i<names.Length;i++)b.clips[i]=Resources.Load<AudioClip>("Audio/SFX/"+names[i]);banks[(int)cue]=b;}
  public void Play(CombatCue cue,Vector3 at=default){
   if(!run||run.IsPaused)return;var b=banks[(int)cue];if(b==null||Time.time<b.next)return;
   int selected=Random.Range(0,b.clips.Length);if(b.clips.Length>1&&selected==b.last)selected=(selected+1)%b.clips.Length;var clip=b.clips[selected];if(!clip)return;
   AudioSource voice=null;foreach(var a in voices)if(!a.isPlaying){voice=a;break;}
   if(!voice){foreach(var a in voices)if(a.priority>b.priority&&(!voice||a.priority>voice.priority))voice=a;}
   if(!voice)return;b.last=selected;b.next=Time.time+b.cooldown;voice.Stop();voice.clip=clip;voice.priority=b.priority;voice.pitch=Random.Range(.96f,1.04f);voice.volume=b.gain;
   float distance=at==default?0:Vector3.Distance(at,run.Player.transform.position);voice.volume*=Mathf.Lerp(1,.4f,Mathf.Clamp01(distance/15));voice.panStereo=at==default?0:Mathf.Clamp((at.x-run.Player.transform.position.x)/16,-.35f,.35f);voice.Play();
   if(cue==CombatCue.Hurt||cue==CombatCue.Shotgun||cue==CombatCue.Power)LastHeavySound=Time.unscaledTime;
  }
  void Update(){
   if(!run)return;if(paused!=run.IsPaused){paused=run.IsPaused;foreach(var a in voices){if(paused)a.Pause();else a.UnPause();}}
   var position=run.Player.transform.position;float distance=Vector3.Distance(position,lastPosition);lastPosition=position;
   if(paused||run.Phase!=SurvivalRun.RunPhase.Wave){walked=0;return;}
   walked+=Mathf.Min(distance,.25f);if(walked>1.15f){walked=0;Play(CombatCue.Step,position);}
  }
 }
}
