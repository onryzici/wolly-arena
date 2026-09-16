using UnityEngine;
namespace WoollyArena {
public sealed class ArenaCamera:MonoBehaviour {
 public bool confineToArena=true;public Vector2 followLimits=new Vector2(1.5f,2);
 public Transform target;public Vector3 offset=new Vector3(0,10,-9);public float smoothTime=.15f;Vector3 velocity;
 void LateUpdate(){if(!target)return;var center=target.position;if(confineToArena){center.x=Mathf.Clamp(center.x,-followLimits.x,followLimits.x);center.z=Mathf.Clamp(center.z,-followLimits.y,followLimits.y);}transform.position=Vector3.SmoothDamp(transform.position,center+offset,ref velocity,smoothTime);}
}
}
