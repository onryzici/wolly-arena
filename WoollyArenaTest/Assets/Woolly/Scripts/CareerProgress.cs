using System;
namespace WoollyArena {
 [Serializable] public sealed class CareerProgress {
  public int Coins, BestWave, BestKills, BestLevel, BestWeapons, Victories, Claimed, Owned=1, Selected;
  public int AccessoriesOwned,WoollyAccessories,VeraAccessories,PatchworkAccessories;
  public static readonly int[] AccessoryPrices={120,90,70};
  public static readonly string[] AccessoryNames={"KANYON ŞAPKASI","NEON GÖZLÜK","ŞERİF KOLYESİ"};
  public bool OwnsAccessory(int id)=>id>=0&&id<3&&(AccessoriesOwned&(1<<id))!=0;
  public int AccessoriesFor(int character)=>character==2?PatchworkAccessories:character==1?VeraAccessories:character==0?WoollyAccessories:0;
  public bool WearingAccessory(int id,int character)=>id>=0&&id<3&&((AccessoriesFor(character)&(1<<id))!=0);
  public bool BuyOrToggleAccessory(int id,int character){
   if(id<0||id>=3||character<0||character>=CharacterDefinition.Count)return false;
   if(!OwnsAccessory(id)){if(Coins<AccessoryPrices[id])return false;Coins-=AccessoryPrices[id];AccessoriesOwned|=1<<id;}
   if(character==2)PatchworkAccessories^=1<<id;else if(character==1)VeraAccessories^=1<<id;else WoollyAccessories^=1<<id;return true;
  }
  public long LastDailyDay=-1;
  public static readonly string[] Titles={"İLK İZLER","TOZ AVCISI","KANYON NÖBETİ","SİLAH KOLEKSİYONU","USTALAŞIYORSUN","KANYON KAHRAMANI"};
  public static readonly string[] Descriptions={"3. dalgayı tamamla","Bir koşuda 50 düşman yen","10. dalgayı tamamla","Aynı anda 4 silah taşı","Bir koşuda 8. seviyeye ulaş","20 dalgalık bir koşuyu kazan"};
  public static readonly int[] Targets={3,50,10,4,8,1}, Rewards={40,60,100,50,80,200}, Prices={0,100,200};
  public static readonly string[] BadgeNames={"ÇAYLAK","TOZ AVCISI","KANYON ŞERİFİ"};
  public int Value(int id)=>id==0||id==2?BestWave:id==1?BestKills:id==3?BestWeapons:id==4?BestLevel:Victories;
  public bool IsClaimed(int id)=>(Claimed&(1<<id))!=0;
  public bool Ready(int id)=>id>=0&&id<Targets.Length&&!IsClaimed(id)&&Value(id)>=Targets[id];
  public int ReadyCount {get{int n=0;for(int i=0;i<Targets.Length;i++)if(Ready(i))n++;return n;}}
  public int ClaimedCount {get{int n=0;for(int i=0;i<Targets.Length;i++)if(IsClaimed(i))n++;return n;}}
  public void Record(int completedWave,int kills,int level,int weapons,bool victory){BestWave=Math.Max(BestWave,Math.Clamp(completedWave,0,20));BestKills=Math.Max(BestKills,Math.Max(0,kills));BestLevel=Math.Max(BestLevel,Math.Max(0,level));BestWeapons=Math.Max(BestWeapons,Math.Clamp(weapons,0,6));if(victory)Victories=Math.Max(1,Victories);}
  public bool Claim(int id){if(!Ready(id))return false;Claimed|=1<<id;Coins+=Rewards[id];return true;}
  public bool ClaimDaily(long day){if(day<0||day<=LastDailyDay)return false;LastDailyDay=day;Coins+=25;return true;}
  public bool Owns(int id)=>id>=0&&id<Prices.Length&&(Owned&(1<<id))!=0;
  public bool BuyOrSelect(int id){if(id<0||id>=Prices.Length)return false;if(!Owns(id)){if(Coins<Prices[id])return false;Coins-=Prices[id];Owned|=1<<id;}Selected=id;return true;}
 }
}
