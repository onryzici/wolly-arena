using System;
using System.Linq;
namespace WoollyArena.Editor {
 public static class BalanceChecks {
  public static void Run(Action<bool,string> check) {
   var build=new SurvivalBuild(1); build.AddExperience(15);
   check(build.Level==1,"Fifteen kills cannot grant the first upgrade");
   build.AddExperience(1);check(build.Level==2&&build.PendingLevels==1,"Sixteenth kill grants exactly one upgrade");
   build.AddExperience(24);check(build.Level==3&&build.PendingLevels==2,"Forty kills grant two upgrades, not four or more");
   check(!WaveDifficulty.ShouldClear(20,90,false,false),"Final cannot win before the boss spawns");
   check(!WaveDifficulty.ShouldClear(20,1,true,true),"Living final boss with time left keeps combat active");
   check(WaveDifficulty.ShouldClear(20,80,true,false),"Killing final boss ends the run early");
   check(WaveDifficulty.ShouldClear(20,0,true,true),"Surviving the final timer wins even with boss alive");
   check(!WaveDifficulty.ShouldClear(10,0,true,true),"Mid-run boss remains a required milestone");
   check(!WaveDifficulty.ShouldClear(4,1,false,false)&&WaveDifficulty.ShouldClear(4,0,false,false),"Normal waves finish only on timer expiry");
   double expected=40*RunBalance.GoldChance(1)+RunBalance.ChestGold(1)*(1-Math.Pow(1-RunBalance.ChestChance,40))+5;
   check(expected>40&&expected<50,"Forty-kill starter wave grants 40–50 expected gold including harvesting");
   bool bounded=true;for(int wave=1;wave<=20;wave++)bounded &= RunBalance.GoldChance(wave)>=.75&&RunBalance.GoldChance(wave)<=1;
   check(bounded,"Gold drop chance stays within the documented range");
   foreach(int kills in new[]{20,40,60}) {
    int min=99,max=0;double average=0;
    for(int seed=1;seed<=1000;seed++) {
     var b=new SurvivalBuild(seed);var rng=new Random(seed);int chests=0;
     for(int k=0;k<kills;k++){if(rng.NextDouble()<RunBalance.GoldChance(1))b.AddMaterials(1);if(chests<RunBalance.ChestLimit&&rng.NextDouble()<RunBalance.ChestChance){chests++;b.AddMaterials(RunBalance.ChestGold(1));}}
     b.AddMaterials(b.Stat(RunStat.Harvesting));b.OpenShop(1);int bought=0;
     // Greedy cheapest-first spending gives an upper-pressure budget test, not a combat bot.
     for(int round=0;round<20;round++) {
      foreach(int i in Enumerable.Range(0,4).OrderBy(i=>b.Offers[i].Price))if(b.Buy(i))bought++;
      if(!b.Reroll())break;
     }
     min=Math.Min(min,bought);max=Math.Max(max,bought);average+=bought;
    }
    Console.WriteLine($"BUDGET first wave, {kills} assumed kills, 1000 seeds: purchases {min}–{max}, average {average/1000:F2}");
    check(max<=(kills==20?2:kills==40?3:4),"Starter shopping stays bounded at "+kills+" assumed kills");
   }
   // Repeatable full-run sensitivity analysis. Kill counts are inputs, not measured gameplay.
   foreach(int baseKills in new[]{15,30,45}) {
    double levels=0,items=0,weapons=0,gold=0,dps=0;
    for(int seed=1;seed<=100;seed++) {
     var b=new SurvivalBuild(seed);var rng=new Random(seed);int earned=0;
     for(int wave=1;wave<=19;wave++) {
      int chests=0,kills=baseKills+wave*2;b.AddExperience(kills);
      while(b.PendingLevels>0){b.RollLevelChoices();b.ChooseLevel(0);}
      for(int k=0;k<kills;k++){if(rng.NextDouble()<RunBalance.GoldChance(wave)){b.AddMaterials(1);earned++;}if(chests<RunBalance.ChestLimit&&rng.NextDouble()<RunBalance.ChestChance){chests++;int amount=RunBalance.ChestGold(wave);b.AddMaterials(amount);earned+=amount;}}
      if(WaveDifficulty.BossWave(wave)){b.AddMaterials(RunBalance.BossGold(wave));earned+=RunBalance.BossGold(wave);}
      b.AddMaterials(b.Stat(RunStat.Harvesting));b.OpenShop(wave);
      // Prefer one offered weapon, then cheapest affordable passive. At most one reroll.
      for(int round=0;round<2;round++){
       foreach(int i in Enumerable.Range(0,4).OrderBy(i=>b.Offers[i].Item.IsWeapon?0:1).ThenBy(i=>b.Offers[i].Price))b.Buy(i);
       if(round==0&&!b.Reroll())break;
      }
     }
     double raw=0;
     foreach(var weapon in b.Weapons){int kind=(int)weapon.Item.Weapon.Value;if(kind<3)raw+=(kind==0?25/.4:kind==1?10/.14:39/.65)*RunItem.TierScale(weapon.Tier);}
     for(int kind=0;kind<3;kind++){int level=b.PowerLevel(kind);if(level>0)raw+=25*(kind==0?1.8:kind==1?1.1:3)*(1+.3*(level-1))/Math.Max(.8,(kind==0?3.2:kind==1?2:2.8)-(level-1)*.25);}
     dps+=raw*b.DamageMultiplier*b.AttackMultiplier*(1+b.Stat(RunStat.Critical)/100.0);
     levels+=b.Level;items+=b.Items.Count;weapons+=b.Weapons.Count;gold+=earned;
    }
    Console.WriteLine($"SENSITIVITY kills/wave={baseKills}+2w, 100 seeds before final: level {levels/100:F1}, passive items {items/100:F1}, weapon slots {weapons/100:F1}, dropped gold {gold/100:F1}, ideal single-target DPS {dps/100:F1}, final boss nominal seconds {WaveDifficulty.BossHealth(20)/(dps/100):F1}");
   }
  }
 }
}
