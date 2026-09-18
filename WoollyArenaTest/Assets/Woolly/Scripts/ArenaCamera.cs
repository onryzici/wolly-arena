using UnityEngine;
namespace WoollyArena {
public sealed class ArenaCamera:MonoBehaviour {
 public bool confineToArena=true;public Vector2 followLimits=new Vector2(1.5f,2);
 public Transform target;public Vector3 offset=new Vector3(0,10,-9);public float smoothTime=.15f;Vector3 velocity;
 [Range(0,1)] public float shakeIntensity=.55f;
 float impact,shakeClock;Vector3 lastShake;
 // Strongest impact wins: crowds cannot accumulate a violent camera displacement.
 public void Punch(float strength){if(float.IsNaN(strength)||float.IsInfinity(strength))return;impact=Mathf.Clamp(Mathf.Max(impact,strength),0,.12f);}
 void LateUpdate(){
  transform.position-=lastShake;lastShake=Vector3.zero;
  if(!target){impact=0;return;}
  var center=target.position;
  if(confineToArena){center.x=Mathf.Clamp(center.x,-followLimits.x,followLimits.x);center.z=Mathf.Clamp(center.z,-followLimits.y,followLimits.y);}
  transform.position=Vector3.SmoothDamp(transform.position,center+offset,ref velocity,smoothTime);
  if(Time.deltaTime<=0)return;
  shakeClock+=Time.deltaTime;
  impact=Mathf.MoveTowards(impact,0,Time.deltaTime*.8f);
  lastShake=(transform.right*Mathf.Sin(shakeClock*93)+transform.up*Mathf.Sin(shakeClock*71))*(impact*.5f*shakeIntensity);
  transform.position+=lastShake;
 }
 void OnDisable(){transform.position-=lastShake;lastShake=Vector3.zero;impact=0;}
}
}
