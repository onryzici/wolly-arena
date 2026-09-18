using System;
namespace WoollyArena {
 // One source for selection-screen values and new-run stats. -1 preserves legacy saves.
 public static class CharacterDefinition {
  public const int Count=3;
  public static string ModelName(int id)=>id==2?"Patchwork":"PunkVera";
  public static string PortraitName(int id)=>id==2?"PatchworkPortrait":id==1?"PunkVeraLobbyPortrait":"WoollyPortrait";
  public static bool IsTitle(string text){for(int i=0;i<Count;i++)if(text==Title(i))return true;return false;}
  public static bool IsRole(string text){for(int i=0;i<Count;i++)if(text==Role(i))return true;return false;}
  public static string Title(int id)=>id==2?"PATCHWORK":id==1?"PUNK VERA":"WOOLLY";
  public static string Role(int id)=>id==2?"YAMALI KUZU":id==1?"HIZLI TETİK":"REVOLVER USTASI";
  public static string Trait(int id)=>id==2?"SAĞLAM DİKİŞ":id==1?"ÇEVİK TETİKÇİ":"KALIN YÜN";
  public static string Description(int id)=>id==1?"%12 hızlı saldırı · %8 hızlı hareket\n15 daha az can · %5 daha az hasar":"+2 başlangıç zırhı\nDaha dayanıklı, dengeli başlangıç";
  public static string Power(int id)=>id==1?"NEON ATAK":"YÜN KALKANI";
  public static string PowerDescription(int id)=>id==1?"Dash: 2 sn +%25 saldırı hızı · 8 sn bekleme":"Dash: kısa kalkan ve yakın darbe · 8 sn bekleme";
  public static int LevelGrowth(int id,RunStat stat,int level)=>(id==0||id==2)&&stat==RunStat.MaxHealth?Math.Max(0,level-1)*2:id==1&&stat==RunStat.AttackSpeed?Math.Max(0,level-1):0;
  public static string GrowthDescription(int id)=>id==1?"Her seviye +%1 saldırı hızı":"Her seviye +2 azami can";
  public static string DisplayValue(RunStat stat,int value)=>stat==RunStat.Damage||stat==RunStat.AttackSpeed||stat==RunStat.MoveSpeed?(100+value)+"%":stat==RunStat.Critical?value+"%":value.ToString();
  public static int BaseStat(int id,RunStat stat){
   int value=stat==RunStat.MaxHealth?100:stat==RunStat.Critical||stat==RunStat.Harvesting?5:0;
   if((id==0||id==2)&&stat==RunStat.Armor)value+=2;
   if(id==1){switch(stat){case RunStat.MaxHealth:value-=15;break;case RunStat.Damage:value-=5;break;case RunStat.AttackSpeed:value+=12;break;case RunStat.MoveSpeed:value+=8;break;}}
   return value;
  }
  public static string DisplayStat(int id,RunStat stat){
   int value=BaseStat(id,stat);
   if(stat==RunStat.Damage||stat==RunStat.AttackSpeed||stat==RunStat.MoveSpeed)return (100+value)+"%";
   if(stat==RunStat.Critical)return value+"%";
   if(stat==RunStat.Regeneration)return value+" / 5 sn";
   return value.ToString();
  }
 }
}
