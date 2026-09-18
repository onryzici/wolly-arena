using System;
using System.Linq;
using System.Security.Cryptography;
namespace WoollyArena.Editor {
 public static class UpgradeSystemChecks {
  public static void Run(Action<bool,string> check){
   foreach(int level in new[]{5,10,15,25}){
    var b=new SurvivalBuild(16);while(b.Level<level)b.AddExperience(b.NextLevelXP-b.Experience);b.RollLevelChoices();while(b.PendingLevels>1)b.ChooseLevel(0);
    int minimum=level>=25?4:level>=10?3:2;
    check(b.RewardLevel==level&&b.LevelChoiceTiers.All(t=>t>=minimum),"Queued level "+level+" keeps its own guaranteed rarity");
   }
   var build=new SurvivalBuild(67);build.AddExperience(40);build.RollLevelChoices();build.OpenShop(2);
   var before=build.LevelChoices.ToArray();check(!build.RerollLevels()&&before.SequenceEqual(build.LevelChoices),"Unaffordable stat reroll does not change choices");
   build.AddMaterials(100);int cost=build.RerollCost;check(build.RerollLevels()&&build.Materials==100-cost&&build.RerollCost>cost,"Stat reroll charges gold and increases its next price");
   var checkpoint=new RunCheckpoint{Build=build,Wave=2,Phase=4,Health=100};
   var loaded=RunCheckpointStore.Decode(RunCheckpointStore.Encode(checkpoint)).Build;
   check(loaded.LevelChoiceTiers.SequenceEqual(build.LevelChoiceTiers)&&loaded.LevelChoices.SequenceEqual(build.LevelChoices),"Save/load preserves exact stat options and rarities");
   build.RerollLevels();loaded.RerollLevels();check(build.LevelChoices.SequenceEqual(loaded.LevelChoices)&&build.LevelChoiceTiers.SequenceEqual(loaded.LevelChoiceTiers),"Resuming cannot change the next stat reroll RNG outcome");
   var plain=new SurvivalBuild(4);plain.AddMaterials(100);plain.Offers[0]=new ShopOffer{Item=RunCatalog.Find("capacitor"),Tier=2,Price=20};plain.Buy(0);
   check(Math.Abs(plain.WeaponDamageMultiplier(RunWeapon.Energy)-1.36f)<.001f&&plain.WeaponDamageMultiplier(RunWeapon.Revolver)==1&&plain.Stat(RunStat.MaxHealth)==84,"Prism core boosts only powers and applies the HP tradeoff");
   plain.SellItem(0);check(plain.WeaponDamageMultiplier(RunWeapon.Energy)==1&&plain.Stat(RunStat.MaxHealth)==100,"Recycling a special item removes both its effect and drawback");
   bool starters=true;for(int seed=1;seed<=40;seed++){var b=new SurvivalBuild(seed);b.OpenShop(1);starters &= b.Offers.Take(2).All(o=>o.Item.IsWeapon)&&b.Offers.All(o=>!o.Item.IsSpecial);}
   check(starters,"Starter shops guarantee two weapons and exclude advanced special items");
   var v4=new RunCheckpoint{Build=new SurvivalBuild(9,1),Character=1,Wave=2,Phase=0,Health=85};
   var bytes=RunCheckpointStore.Encode(v4);var old=new byte[bytes.Length-16];Array.Copy(bytes,old,bytes.Length-48);old[4]=4;
   using(var sha=SHA256.Create()){var hash=sha.ComputeHash(old,0,old.Length-32);Array.Copy(hash,0,old,old.Length-32,32);}
   var legacy=RunCheckpointStore.Decode(old);check(legacy.Build.CharacterId==1&&legacy.Build.LevelChoiceTiers.All(t=>t==1),"Version-four saves retain character traits and default old options to tier I");
  }
 }
}
