using System;
namespace WoollyArena.Editor {
 public static class MeleeStrikeChecks {
  public static void Run(Action<bool,string> check){
   var s=new MeleeStrike();check(s.Progress(0)<0&&!s.Resolve(0,true),"Idle enemies cannot resolve phantom melee hits");
   s.Begin(10);check(!s.Resolve(10.2f,true)&&s.Progress(10.2f)>0,"Melee anticipation is visible before damage");
   check(s.Resolve(10.3f,true)&&!s.Resolve(10.31f,true),"A melee swing damages only once");
   s.Begin(20);check(!s.Resolve(20.3f,false)&&!s.Resolve(20.32f,true),"Dodging out of reach consumes the attack instead of deferring damage");
   s.Begin(30);s.Cancel();check(!s.Resolve(30.3f,true)&&s.Progress(30.3f)<0,"Stagger cancels both the pending strike and its pose");
   s.Begin(40);check(!s.Resolve(42,true),"A long hitch cannot deliver a stale invisible strike");
   s.Begin(float.NaN);check(s.Progress(42)<0,"Invalid timestamps cannot create an attack");
  }
 }
}
