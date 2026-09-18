using System;
namespace WoollyArena {
 // A strike commits after its visible anticipation; each attempt resolves at most once.
 public sealed class MeleeStrike {
  public const float Windup=.28f,Duration=.5f;
  float started=float.NegativeInfinity;bool pending;
  public void Begin(float now){if(float.IsNaN(now)||float.IsInfinity(now))return;started=now;pending=true;}
  public void Cancel(){pending=false;started=float.NegativeInfinity;}
  public float Progress(float now){float age=now-started;return age>=0&&age<Duration?age/Duration:-1;}
  public bool Resolve(float now,bool canHit){
   float age=now-started;if(!pending||age<Windup)return false;
   pending=false;return age<=Duration&&canHit;
  }
 }
}
