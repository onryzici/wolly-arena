using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
namespace WoollyArena.Editor {
public static class ArenaValidation {
 public static string Run(){var checks=new List<string>();Action<bool,string> check=(ok,n)=>{if(!ok)throw new Exception("FAIL "+n);checks.Add("PASS "+n);};
 var w=new WeaponState();check(w.TryFire(0),"first shot");check(!w.TryFire(.1f),"cooldown rejects early shot");for(int i=1;i<w.Capacity;i++)check(w.TryFire(i*.3f),"shot "+(i+1));check(!w.TryFire(2.5f)&&w.Reloading,"empty magazine starts reload");check(!w.TryFire(3f),"cannot shoot while reloading");w.Tick(3.7f);check(w.Ammo==w.Capacity&&!w.Reloading,"reload completion");check(!w.Reload(3.8f),"full magazine cannot reload");
 check(Mathf.Abs(ArenaPlayer.ClampInput(Vector2.one).magnitude-1)<.0001f,"no diagonal speed boost");
 var p=UnityEngine.Object.FindFirstObjectByType<ArenaPlayer>();check(p&&p.muzzle&&p.view&&p.animator&&p.effects,"scene references");var clips=p.animator.runtimeAnimatorController.animationClips;check(new[]{"Idle","Walk","Run","Shoot"}.All(n=>clips.Any(c=>c.name==n)),"four animation clips");check(clips.Where(c=>c.name!="Shoot").All(c=>c.isLooping),"locomotion loops");
 var ac=(AnimatorController)p.animator.runtimeAnimatorController;var mask=ac.layers[1].avatarMask;check(Enumerable.Range(0,mask.transformCount).All(i=>!mask.GetTransformPath(i).StartsWith("Visual")),"mask paths match imported animation");check(Enumerable.Range(0,mask.transformCount).Where(i=>mask.GetTransformPath(i).Contains("UpLeg")).All(i=>!mask.GetTransformActive(i)),"shoot mask excludes legs");
 check(p.visual.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Grip_R_"))==8,"eight finger bones imported");check(UnityEngine.Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsSortMode.None).All(t=>t.font!=null),"HUD fonts assigned");
 var result=string.Join("\n",checks);System.IO.File.WriteAllText("Logs/arena-edit-tests.txt",result);return result;}
}
}
