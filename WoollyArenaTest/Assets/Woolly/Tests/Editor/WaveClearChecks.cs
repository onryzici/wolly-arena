using System;
namespace WoollyArena.Editor {
public static class WaveClearChecks {
 public static void Run(Action<bool,string> check){
  var t=new WaveClearTimeline();
  check(!t.Ready&&t.SweepCount(48)==0,"Wave completion begins with a visible hold, not an immediate shop");
  t.Advance(.1f,false);check(!t.Ready&&t.SweepCount(48)==0,"Survivors remain still during the initial impact beat");
  t.Advance(.4f,false);int swept=t.SweepCount(48);check(swept>0&&swept<48,"End-of-wave sweep removes enemies progressively");
  float before=t.Elapsed;t.Advance(10,true);check(t.Elapsed==before&&!t.Ready,"Pause freezes celebration and prevents premature shop entry");
  t.Advance(float.NaN,false);t.Advance(float.PositiveInfinity,false);t.Advance(-1,false);check(t.Elapsed==before,"Invalid time inputs cannot corrupt the transition");
  t.Advance(.5f,false);check(t.SweepCount(48)==48&&!t.Ready,"Enemy sweep finishes before the reward presentation closes");
  t.Advance(10,false);check(t.Ready&&t.Elapsed==WaveClearTimeline.Duration&&t.SweepCount(0)==0,"Long frames finish cleanly and empty waves are supported");
  t.Advance(1,false);check(t.Elapsed==WaveClearTimeline.Duration,"Completed presentation does not accumulate more time");
  t.Reset();check(!t.Ready&&t.Elapsed==0&&t.SweepCount(48)==0,"Next wave resets all presentation progress");
 }
}
}
