using System;
namespace WoollyArena.Editor {
 public static class CareerProgressChecks {
  public static void Run(Action<bool,string> check){
   var p=new CareerProgress();
   check(!p.Claim(0)&&p.Coins==0,"Incomplete mission cannot pay");
   check(!p.Claim(-1)&&!p.Claim(6),"Invalid mission ids cannot pay");
   p.Record(3,49,7,3,false);check(p.Ready(0)&&!p.Ready(1)&&!p.Ready(3)&&!p.Ready(4),"Mission thresholds are exact");
   check(p.Claim(0)&&p.Coins==40&&!p.Claim(0)&&p.Coins==40,"Mission reward is granted only once");
   p.Record(1,5,1,1,false);check(p.BestWave==3&&p.BestKills==49,"Smaller or resumed run does not reduce records");
   p.Record(10,50,8,4,false);check(p.ReadyCount==4,"Combat progress unlocks four further missions");
   p.Record(20,80,9,6,true);p.Record(20,80,9,6,true);check(p.Victories==1&&p.Ready(5),"Victory milestone remains idempotent");
   check(!p.BuyOrSelect(1)&&p.Coins==40,"Unaffordable cosmetic cannot spend currency");
   check(p.ClaimDaily(100)&&!p.ClaimDaily(100)&&!p.ClaimDaily(99)&&p.Coins==65,"Daily reward rejects duplicates and clock rollback");
   check(p.ClaimDaily(101)&&p.Coins==90,"Next UTC day unlocks daily reward");
   p.Claim(1);check(p.Coins==150&&p.BuyOrSelect(1)&&p.Coins==50&&p.Owns(1),"Cosmetic purchase deducts exact price and unlocks badge");
   check(p.BuyOrSelect(0)&&p.BuyOrSelect(1)&&p.Coins==50,"Owned cosmetics equip without repeated charge");
   check(!p.BuyOrSelect(-1)&&!p.BuyOrSelect(3),"Unknown cosmetics cannot be selected");
   var accessories=new CareerProgress{Coins=200};
   check(accessories.BuyOrToggleAccessory(0,1)&&accessories.Coins==80&&accessories.WearingAccessory(0,1)&&!accessories.WearingAccessory(0,0),"Accessory unlock equips only the chosen character");
   check(accessories.BuyOrToggleAccessory(0,0)&&accessories.Coins==80&&accessories.WearingAccessory(0,0),"Owned accessory equips second character without a second purchase");
   check(accessories.BuyOrToggleAccessory(0,1)&&!accessories.WearingAccessory(0,1)&&accessories.Coins==80,"Accessory removal preserves ownership and coins");
   check(!accessories.BuyOrToggleAccessory(1,1)&&!accessories.BuyOrToggleAccessory(9,0),"Unaffordable and unknown accessories cannot be equipped");
   check(p.ClaimedCount==2,"Claimed reward count matches mission flags");
  }
 }
}
