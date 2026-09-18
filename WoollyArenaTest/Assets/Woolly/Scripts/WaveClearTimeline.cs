using System;
namespace WoollyArena {
// Presentation time is separate from wave time; it never changes global Time.timeScale.
public sealed class WaveClearTimeline {
 public const float Duration=2.8f;
 public float Elapsed {get;private set;}
 public bool Ready=>Elapsed>=Duration;
 public float Progress=>Elapsed/Duration;
 public void Reset(){Elapsed=0;}
 public void Advance(float seconds,bool paused){if(!paused&&seconds>0&&!float.IsNaN(seconds)&&!float.IsInfinity(seconds))Elapsed=Math.Min(Duration,Elapsed+seconds);}
 public int SweepCount(int count)=>Math.Min(count,Math.Max(0,(int)(count*Math.Min(1,Math.Max(0,(Elapsed-.15f)/.65f)))));
}
}
