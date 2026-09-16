using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace WoollyArena {
// Explicitly attached only by the Editor validation command; never part of the shipped scene.
public sealed class ArenaSmokeProbe:MonoBehaviour {
 public List<string> checks=new List<string>();ArenaPlayer player;Vector3 start;bool done;
 void Check(bool pass,string name){checks.Add((pass?"PASS ":"FAIL ")+name);}
 IEnumerator Start(){
  player=GetComponent<ArenaPlayer>();start=transform.position;player.SimulatedInput=true;player.TestAim=Vector2.up;
  yield return new WaitForSeconds(.2f);
  player.TestMove=Vector2.up;yield return new WaitForSeconds(.55f);Check(Mathf.Abs(player.Speed-player.walkSpeed)<.15f,"walk speed / locomotion");
  player.TestRun=true;yield return new WaitForSeconds(.5f);Check(Mathf.Abs(player.Speed-player.runSpeed)<.15f,"run speed / locomotion");
  player.TestFire=true;yield return new WaitForSeconds(.14f);Check((player.animator.GetCurrentAnimatorStateInfo(1).IsName("Shoot")||player.animator.GetNextAnimatorStateInfo(1).IsName("Shoot"))&&player.Speed>player.runSpeed-.15f,"fire while running preserves locomotion");player.TestFire=false;
  player.TestMove=Vector2.zero;yield return new WaitForSeconds(.4f);Check(player.Speed<.05f,"deceleration to idle");
  player.TestFire=true;yield return new WaitForSeconds(.14f);Check((player.animator.GetCurrentAnimatorStateInfo(1).IsName("Shoot")||player.animator.GetNextAnimatorStateInfo(1).IsName("Shoot")),"upper body shoot layer");
  yield return new WaitForSeconds(2.1f);player.TestFire=false;Check(player.weapon.Shots>=8,"eight shots and cooldown");Check(player.Hits>=4,"raycast damages target");
  player.weapon.Reload(Time.time);yield return new WaitForSeconds(1.3f);Check(player.weapon.Ammo==player.weapon.Capacity&&!player.weapon.Reloading,"reload restores magazine");
  var cc=GetComponent<CharacterController>();cc.enabled=false;transform.position=new Vector3(-5,.05f,-2);cc.enabled=true;player.TestMove=Vector2.up;yield return new WaitForSeconds(.8f);player.TestMove=Vector2.zero;Check(transform.position.z<-.70f,"cover blocks movement");
  yield return new WaitForSeconds(.3f);player.TestFire=true;int hits=player.Hits;yield return new WaitForSeconds(.35f);player.TestFire=false;Check(player.Hits==hits,"cover blocks shots");
  cc.enabled=false;transform.position=start;cc.enabled=true;player.SimulatedInput=false;done=true;
  string result=string.Join("\n",checks);Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Logs"));File.WriteAllText(Path.Combine(Application.dataPath,"../Logs/arena-smoke-test.txt"),result);Debug.Log("ARENA SMOKE TEST\n"+result);Destroy(this);
 }
 void OnDisable(){if(player)player.SimulatedInput=false;if(!done)Debug.LogWarning("Arena smoke test interrupted");}
}
}
