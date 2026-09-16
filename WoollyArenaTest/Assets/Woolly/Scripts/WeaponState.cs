using System;
namespace WoollyArena {
// Pure state makes cooldown/reload deterministic and independently testable.
[Serializable] public sealed class WeaponState {
 public int Capacity=8; public float Interval=.28f, ReloadDuration=1.15f;
 public int Ammo {get;private set;}=8; public int Shots {get;private set;}
 public bool Reloading {get;private set;} float nextShot, reloadEnd;
 public void Tick(float now){if(Reloading && now>=reloadEnd){Ammo=Capacity;Reloading=false;}}
 public bool Reload(float now){Tick(now);if(Reloading||Ammo==Capacity)return false;Reloading=true;reloadEnd=now+ReloadDuration;return true;}
 public bool TryFire(float now){Tick(now);if(Reloading||now<nextShot)return false;if(Ammo==0){Reload(now);return false;}Ammo--;Shots++;nextShot=now+Interval;return true;}
 public float ReloadProgress(float now)=>Reloading?1-Math.Clamp((reloadEnd-now)/ReloadDuration,0,1):1;
}
}
