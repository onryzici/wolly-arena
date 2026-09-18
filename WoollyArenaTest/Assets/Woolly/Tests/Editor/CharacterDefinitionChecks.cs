using System;
using System.IO;
using System.Security.Cryptography;
namespace WoollyArena.Editor {
 public static class CharacterDefinitionChecks {
  public static void Run(Action<bool,string> check){
   var woolly=new SurvivalBuild(12,0);var vera=new SurvivalBuild(12,1);var patchwork=new SurvivalBuild(12,2);
   check(patchwork.Stat(RunStat.MaxHealth)==100&&patchwork.Stat(RunStat.Armor)==2,"Patchwork uses the durable Woolly profile");
   check(woolly.Stat(RunStat.MaxHealth)==100&&woolly.Stat(RunStat.Armor)==2,"Woolly starts with 100 HP and thick-wool armor");
   check(vera.Stat(RunStat.MaxHealth)==85&&vera.Stat(RunStat.Armor)==0,"Vera trades durability for agility");
   check(Math.Abs(vera.AttackMultiplier-1.12f)<.001f&&Math.Abs(vera.MoveMultiplier-1.08f)<.001f&&Math.Abs(vera.DamageMultiplier-.95f)<.001f,"Vera's trait affects actual combat multipliers");
   bool consistent=true;for(int id=0;id<CharacterDefinition.Count;id++){var b=new SurvivalBuild(1,id);for(int i=0;i<8;i++)consistent &= b.Stat((RunStat)i)==CharacterDefinition.BaseStat(id,(RunStat)i);}
   check(consistent,"All eight displayed base stats come from the same values as gameplay");
   check(CharacterDefinition.DisplayStat(1,RunStat.Damage)=="95%"&&CharacterDefinition.DisplayStat(1,RunStat.AttackSpeed)=="112%","UI shows total damage/speed multipliers, not ambiguous bonus values");
   foreach(var b in new[]{woolly,vera,patchwork}){
    b.AddExperience(40);b.RollLevelChoices();b.ChooseLevel(0);b.AddMaterials(100);
    b.Offers[0]=new ShopOffer{Item=RunCatalog.Find("boots"),Tier=1,Price=10};b.Buy(0);
    var saved=new RunCheckpoint{Build=b,Character=b.CharacterId,Wave=2,Phase=0,Health=70};
    var loaded=RunCheckpointStore.Decode(RunCheckpointStore.Encode(saved));
    bool same=loaded.Build.CharacterId==b.CharacterId;for(int i=0;i<8;i++)same &= loaded.Build.Stat((RunStat)i)==b.Stat((RunStat)i);
    check(same,"Character "+b.CharacterId+": restore keeps trait, earned bonuses and gear without doubling bonuses");
   }
   var accessories=new CareerProgress{Coins=500};
   check(accessories.BuyOrToggleAccessory(0,2)&&accessories.WearingAccessory(0,2)&&!accessories.WearingAccessory(0,0)&&!accessories.WearingAccessory(0,1),"Patchwork accessories are independent from Woolly and Vera");
   var growthP=new SurvivalBuild(1,2);while(growthP.Level<2)growthP.AddExperience(1);
   check(growthP.Stat(RunStat.MaxHealth)==102,"Patchwork level growth matches the selection description");
   var growthW=new SurvivalBuild(1,0);var growthV=new SurvivalBuild(1,1);
   while(growthW.Level<2)growthW.AddExperience(1);while(growthV.Level<2)growthV.AddExperience(1);
   check(growthW.Stat(RunStat.MaxHealth)==102&&growthV.Stat(RunStat.AttackSpeed)==13,"Character level growth changes actual health / attack speed before choosing a bonus");
   growthW.RollLevelChoices();growthW.LevelChoices[0]=RunStat.Damage;growthW.LevelChoiceTiers[0]=1;int before=growthW.Stat(RunStat.Damage);growthW.ChooseLevel(0);
   check(growthW.Stat(RunStat.Damage)==before+5&&CharacterDefinition.DisplayValue(RunStat.Damage,growthW.Stat(RunStat.Damage))=="105%","Chosen level bonus raises actual stat and displayed total");
   check(CharacterDefinition.LevelGrowth(-1,RunStat.MaxHealth,9)==0,"Legacy stat profile retains saved balance");
   var checkpoint=new RunCheckpoint{Build=new SurvivalBuild(4),Character=1,Wave=2,Phase=0,Health=100};
   var bytes=RunCheckpointStore.Encode(checkpoint);var old=new byte[bytes.Length-20];Array.Copy(bytes,old,bytes.Length-52);old[4]=3;
   using(var sha=SHA256.Create()){var hash=sha.ComputeHash(old,0,old.Length-32);Array.Copy(hash,0,old,old.Length-32,32);}
   var legacy=RunCheckpointStore.Decode(old);
   check(legacy.Character==1&&legacy.Build.CharacterId==-1&&legacy.Build.Stat(RunStat.MaxHealth)==100,"Version-three Vera saves retain their old stats and health");
   checkpoint.Build=new SurvivalBuild(4,0);bool rejected=false;
   try{RunCheckpointStore.Encode(checkpoint);}catch(InvalidDataException){rejected=true;}
   check(rejected,"Mismatched model and character stat profiles cannot be saved");
  }
 }
}
