using System;
namespace WoollyArena {
 // Economy is tuned for this arena's kill density, not Brotato's raw enemy counts.
 public static class RunBalance {
  public static float AttackMultiplier(int percent) => 1 + (percent<=60?percent:60+(percent-60)*.35f)/100f;
  public static float StaggerCooldown(int wave)=>wave<6?.6f:1.05f;
  public static int ArmoredDamage(int damage,int wave,bool boss,bool elite){int block=boss?3+wave/5:elite?2+wave/4:0;return Math.Max(1,damage-block);}
  public static int ExperienceForLevel(int level) { int n=Math.Max(0,level-1); return 16+6*n+n*n/2; }
  public static double GoldChance(int wave)=>Math.Max(.75,1-Math.Max(0,wave-3)*.015);
  public const double ChestChance=.01;
  public const int ChestLimit=1;
  public static int ChestGold(int wave)=>6+Math.Max(1,wave)/2;
  public static int BossGold(int wave)=>10+Math.Max(1,wave);
  public static int Price(int basePrice,int tier,int wave)=>(int)Math.Ceiling(basePrice*tier*1.35)+2*wave;
  public static int RerollPrice(int wave,int rerolls)=>4+wave+rerolls*Math.Max(3,wave/3);
 }
}
