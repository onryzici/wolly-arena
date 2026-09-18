using TMPro;
using UnityEngine;
namespace WoollyArena {
 public sealed partial class LobbyScreen {
  static readonly Color[] BadgeColors={new Color32(85,91,120,255),new Color32(168,103,48,255),new Color32(52,129,165,255)};
  void RefreshCareer(){
   var data=CareerStore.Load();
   foreach(string id in new[]{"Coins","Gems"}){var t=content.Find(id);if(t){var label=t.Find("Label");if(label)label.GetComponent<TMP_Text>().text=(id=="Coins"?data.Coins:data.ClaimedCount).ToString();}}
   var medals=content.Find("Gems");if(medals){var icon=medals.Find("ItemIcon_Gem_Diamond_Purple");if(icon)icon.GetComponent<UnityEngine.UI.Image>().sprite=SpriteFor("ItemIcon_MemoPad");}
   var profile=content.Find("Profile");if(profile){var image=profile.GetComponent<UnityEngine.UI.Image>();if(image)image.color=BadgeColors[Mathf.Clamp(data.Selected,0,2)];foreach(var t in profile.GetComponentsInChildren<TMP_Text>())if(t.text.Contains("ÇAYLAK")||t.text.Contains("TOZ AVCISI")||t.text.Contains("KANYON ŞERİFİ"))t.text=CareerProgress.BadgeNames[Mathf.Clamp(data.Selected,0,2)];}
   var ranking=content.Find("Ranking");if(ranking)foreach(var label in ranking.GetComponentsInChildren<TMP_Text>())label.text="Rekorlar";
  }
  Transform CareerDialog(string title,string note){
   ShowPopup(title,"ItemIcon_MemoPad","","");var card=popup.transform.Find("Dialog");
   foreach(Transform child in card)child.gameObject.SetActive(false);
   ((RectTransform)card).sizeDelta=new Vector2(940,660);
   Text(card,title,new Vector2(0,275),new Vector2(840,64),38);
   Text(card,note,new Vector2(0,220),new Vector2(840,36),21,color:Cyan);
   var close=MakeButton(card,"CareerClose","KAPAT",Vector2.one*.5f,new Vector2(305,-274),new Vector2(240,68),"Button01_s_Blue",25);
   close.onClick.AddListener(ClosePopup);return card;
  }
  void ShowMissions(int page){
   var data=CareerStore.Load();var card=CareerDialog("KANYON GÖREVLERİ","Kalıcı hedefler · Ödüller lobi mağazasında kullanılır");
   for(int row=0;row<3;row++){
    int id=page*3+row;float y=126-row*130;
    var item=Panel(card,"Mission "+id,Vector2.one*.5f,new Vector2(0,y),new Vector2(858,116),"BorderFrame_Round20_Single_Dark");item.color=new Color32(65,73,94,255);
    Text(item.transform,CareerProgress.Titles[id],new Vector2(-105,32),new Vector2(565,32),25,TextAlignmentOptions.MidlineLeft);
    Text(item.transform,CareerProgress.Descriptions[id],new Vector2(-105,0),new Vector2(565,30),21,TextAlignmentOptions.MidlineLeft);
    int progress=Mathf.Min(data.Value(id),CareerProgress.Targets[id]);
    var track=Panel(item.transform,"Progress",Vector2.one*.5f,new Vector2(-160,-34),new Vector2(450,9),"BorderFrame_Round20_Single_Dark");track.color=Ink;
    var fill=Panel(track.transform,"Fill",new Vector2(0,.5f),Vector2.zero,new Vector2(450f*progress/CareerProgress.Targets[id],9),null);fill.rectTransform.pivot=new Vector2(0,.5f);fill.color=Cyan;
    Text(item.transform,progress+" / "+CareerProgress.Targets[id],new Vector2(133,-33),new Vector2(116,27),18);
    bool claimed=data.IsClaimed(id),ready=data.Ready(id);
    var claim=MakeButton(item.transform,"Claim "+id,claimed?"ALINDI":ready?"AL +"+CareerProgress.Rewards[id]:"+"+CareerProgress.Rewards[id]+" ALTIN",Vector2.one*.5f,new Vector2(304,0),new Vector2(206,68),"Button01_l_Yellow",21,true);
    claim.interactable=ready;claim.onClick.AddListener(()=>{var latest=CareerStore.Load();if(latest.Claim(id)){CareerStore.Save(latest);RefreshCareer();}ShowMissions(page);});
   }
   Text(card,(page+1)+" / 2",new Vector2(-180,-275),new Vector2(110,40),24);
   var swap=MakeButton(card,"MissionPage",page==0?"SONRAKİ >":"< ÖNCEKİ",Vector2.one*.5f,new Vector2(5,-274),new Vector2(235,68),"Button01_s_Blue",22);swap.onClick.AddListener(()=>ShowMissions(1-page));
  }
  void ShowCosmetics(){
   var data=CareerStore.Load();int character=PlayableCharacter.Selected;
   var card=CareerDialog("AKSESUAR MAĞAZASI",PlayableCharacter.Title(character)+" · "+data.Coins+" altın · Aksesuarlar statları etkilemez");
   for(int i=0;i<3;i++){
    int id=i;float x=-285+i*285;
    var tile=Panel(card,"Accessory "+i,Vector2.one*.5f,new Vector2(x,5),new Vector2(267,320),"BorderFrame_Round20_Single_Dark");tile.color=BadgeColors[i];
    // Flat catalog silhouette matches each accessory category.
    var preview=new GameObject("Accessory symbol",typeof(RectTransform));preview.transform.SetParent(tile.transform,false);var rect=(RectTransform)preview.transform;rect.anchoredPosition=new Vector2(0,78);rect.sizeDelta=new Vector2(140,100);var symbol=preview.AddComponent<AccessoryGlyph>();symbol.kind=i;symbol.raycastTarget=false;
    Text(tile.transform,CareerProgress.AccessoryNames[i],new Vector2(0,-5),new Vector2(250,35),21);
    Text(tile.transform,i==0?"Geniş kenarlı şapka":i==1?"Mavi camlı gözlük":"Altın şerif simgesi",new Vector2(0,-43),new Vector2(250,30),18);
    bool owned=data.OwnsAccessory(i),wearing=data.WearingAccessory(i,character);
    var buy=MakeButton(tile.transform,"EquipAccessory "+i,wearing?"ÇIKAR":owned?"TAK":CareerProgress.AccessoryPrices[i]+" ALTIN",Vector2.one*.5f,new Vector2(0,-110),new Vector2(226,68),"Button01_l_Yellow",23,true);
    buy.interactable=owned||data.Coins>=CareerProgress.AccessoryPrices[i];buy.onClick.AddListener(()=>{var latest=CareerStore.Load();if(latest.BuyOrToggleAccessory(id,character)){CareerStore.Save(latest);RefreshCareer();CharacterAccessories.Apply(hero,character);}ShowCosmetics();});
   }
   var badges=MakeButton(card,"Badges","ROZETLER",Vector2.one*.5f,new Vector2(-280,-274),new Vector2(240,68),"Button01_s_Blue",24);badges.onClick.AddListener(ShowBadges);
  }
  void ShowBadges(){
   var data=CareerStore.Load();var card=CareerDialog("ROZET DÜKKÂNI",data.Coins+" altın · Rozetler görünümü değiştirir, savaş statlarını etkilemez");
   for(int i=0;i<3;i++){
    int id=i;float x=-285+i*285;
    var tile=Panel(card,"Badge "+i,Vector2.one*.5f,new Vector2(x,10),new Vector2(267,320),"BorderFrame_Round20_Single_Dark");tile.color=BadgeColors[i];
    Icon(tile.transform,"ItemIcon_Trophy_Gold",new Vector2(0,79),104);
    Text(tile.transform,CareerProgress.BadgeNames[i],new Vector2(0,0),new Vector2(250,40),23);
    Text(tile.transform,i==0?"İlk maceran":i==1?"Çölün iz sürücüsü":"Kanyonun koruyucusu",new Vector2(0,-42),new Vector2(250,32),19);
    var buy=MakeButton(tile.transform,"BadgeAction "+i,data.Selected==i?"SEÇİLİ":data.Owns(i)?"KUŞAN":CareerProgress.Prices[i]+" ALTIN",Vector2.one*.5f,new Vector2(0,-109),new Vector2(226,68),"Button01_l_Yellow",23,true);
    buy.interactable=data.Selected!=i&&(data.Owns(i)||data.Coins>=CareerProgress.Prices[i]);buy.onClick.AddListener(()=>{var latest=CareerStore.Load();if(latest.BuyOrSelect(id)){CareerStore.Save(latest);RefreshCareer();}ShowBadges();});
   }
  }
  void ShowDaily(){
   var data=CareerStore.Load();bool available=CareerStore.Today>data.LastDailyDay;
   var card=CareerDialog("KAMP ERZAĞI","Günde bir kez · Yenilenme 03.00 Türkiye saati (UTC 00.00)");
   Icon(card,"ItemIcon_Gift_Blue",new Vector2(0,70),150);
   Text(card,available?"25 LOBİ ALTINI HAZIR":"BUGÜNKÜ HEDİYEN ALINDI",new Vector2(0,-44),new Vector2(800,60),32);
   var claim=MakeButton(card,"DailyClaim",available?"HEDİYEYİ AL":"YARIN TEKRAR GEL",Vector2.one*.5f,new Vector2(0,-146),new Vector2(370,78),"Button01_l_Yellow",25,true);claim.interactable=available;
   claim.onClick.AddListener(()=>{var latest=CareerStore.Load();if(latest.ClaimDaily(CareerStore.Today)){CareerStore.Save(latest);RefreshCareer();}ShowDaily();});
  }
  void ShowRecords(){
   var data=CareerStore.Load();var card=CareerDialog("KİŞİSEL REKORLAR","Bu cihazdaki kalıcı ilerlemen");
   string[] labels={"TAMAMLANAN EN İYİ DALGA","BİR KOŞUDA EN ÇOK AV","EN YÜKSEK SEVİYE","TAMAMLANAN GÖREV"};int[] values={data.BestWave,data.BestKills,data.BestLevel,data.ClaimedCount};
   for(int i=0;i<4;i++){float x=i%2==0?-214:214,y=i<2?85:-85;var tile=Panel(card,"Record "+i,Vector2.one*.5f,new Vector2(x,y),new Vector2(410,145),"BorderFrame_Round20_Single_Dark");tile.color=new Color32(65,73,94,255);Text(tile.transform,labels[i],new Vector2(0,36),new Vector2(390,32),21);Text(tile.transform,values[i].ToString(),new Vector2(0,-24),new Vector2(380,72),48,color:Cyan);}
  }
 }
}
