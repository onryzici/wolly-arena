using System;
namespace WoollyArena {
 public static class WaveDifficulty {
  static int Late(int wave)=>Math.Max(0,Math.Min(20,wave)-5);
  public static float HealthScale(int wave)=>1.2f+Late(wave)*.14f;
  public static float SpeedBonus(int wave)=>Math.Min(.55f,Late(wave)*.045f);
  public static int DamageBonus(int wave)=>Late(wave)/2;
  public static int EnemyLimit(int wave)=>Math.Min(60,14+wave*4+Late(wave));
  public static float SpawnInterval(int wave)=>Math.Max(.20f,.7f-(wave-1)*.025f-Late(wave)*.009f);
  public static bool BossWave(int wave)=>wave>=5&&wave<=20&&wave%5==0;
  public static int BossHealth(int wave)=>1800+Late(wave)*600+Late(wave)*Late(wave)*60;
  public static bool ShouldClear(int wave,float remaining,bool spawned,bool alive)=>wave==20 ? spawned&&(!alive||remaining<=0) : remaining<=0&&CanClear(wave,spawned,alive);
  public static bool CanClear(int wave,bool bossSpawned,bool bossAlive)=>!BossWave(wave)||(bossSpawned&&!bossAlive);
 }
}
