using UnityEngine;
namespace WoollyArena {
[DefaultExecutionOrder(200)]public sealed class FootstepDust:MonoBehaviour {
 public FootprintTrail footprints;
 public ArenaPlayer player;public ParticleSystem dust;public Transform leftFoot,rightFoot;
 public int Bursts {get;private set;} CharacterController motor;float[] previous=new float[2],last=new float[2];Vector3 oldPosition;Vector3[] lastStamp=new Vector3[2];bool[] stamped=new bool[2];
 void Awake(){motor=GetComponent<CharacterController>();oldPosition=transform.position;previous[0]=previous[1]=0;}
 void LateUpdate(){var movement=transform.position-oldPosition;oldPosition=transform.position;bool moving=motor.isGrounded&&player.Speed>.3f;for(int i=0;i<2;i++){var foot=i==0?leftFoot:rightFoot;float h=foot.position.y-transform.position.y;float threshold=player.Speed>player.walkSpeed+.2f?.205f:.18f;if(moving&&previous[i]>threshold&&h<=threshold&&Time.time-last[i]>.17f){last[i]=Time.time;Emit(foot.position,movement);if(footprints&&(!stamped[i]||Vector3.Distance(lastStamp[i],foot.position)>.14f)){var toe=foot.Find(i==0?"mixamorig:LeftToeBase":"mixamorig:RightToeBase");var forward=toe?toe.position-foot.position:player.visual.forward;forward.y=0;footprints.Stamp(new Vector3(foot.position.x,transform.position.y+.015f,foot.position.z),forward);lastStamp[i]=foot.position;stamped[i]=true;}}previous[i]=h;}}
 void Emit(Vector3 foot,Vector3 movement){Bursts++;bool run=player.Speed>player.walkSpeed+.2f;int count=run?6:3;for(int i=0;i<count;i++){var e=new ParticleSystem.EmitParams();e.position=new Vector3(foot.x,transform.position.y+.04f,foot.z)+new Vector3(Random.Range(-.06f,.06f),0,Random.Range(-.06f,.06f));e.velocity=-movement.normalized*Random.Range(.1f,.24f)+new Vector3(Random.Range(-.14f,.14f),Random.Range(.10f,.24f),Random.Range(-.14f,.14f));e.startLifetime=Random.Range(.32f,.55f);e.startSize=Random.Range(.10f,run?.27f:.19f);e.startColor=new Color(1,.78f,.49f,run?.42f:.29f);dust.Emit(e,1);}}
}
}
