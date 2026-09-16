using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
namespace WoollyArena {
public sealed class LobbyScreen:MonoBehaviour {
 public TMP_FontAsset font;public Sprite[] art;public Transform hero;public Animator animator;
 GameObject popup;bool loading;float baseYaw;
 void Start(){Application.targetFrameRate=60;baseYaw=hero?hero.eulerAngles.y:160;if(animator){animator.SetFloat("Speed",0);animator.SetFloat("Playback",1);}foreach(var b in GetComponentsInChildren<UnityEngine.UI.Button>(true)){var n=b.name;b.onClick.AddListener(()=>Click(n));}}
 void Update(){if(hero)hero.rotation=Quaternion.Euler(0,baseYaw+Mathf.Sin(Time.unscaledTime*.6f)*4,0);}
 public void Click(string id){if(loading)return;if(id=="Play"){loading=true;SceneManager.LoadSceneAsync("TrainingArena");return;}if(id=="Close"){if(popup)Destroy(popup);return;}
  string title=id=="Hero"?"WOOLLY":id=="Shop"?"MAĞAZA":id=="Friends"?"ARKADAŞLAR":id=="Settings"?"AYARLAR":id=="Quests"?"GÖREVLER":"TEST ARENASI";
  string body=id=="Hero"?"REVOLVER USTASI\n\n8 mermi • Hızlı yeniden doldurma\nSol kontrol: hareket ve koşu\nSağ düğme: baktığın yöne ateş":id=="Mode"?"TEK OYUNCULU ANTRENMAN\n\nDört düşman, parçalanabilir siperler.\nDüşmanlar yenildikten sonra yeniden doğar.":id=="Settings"?"SES\n\nOyun sesi düğmesi aşağıda.\nYatay ekran • 60 FPS hedefi":"Bu bölüm henüz test sürümünde açık değil.\nŞimdilik OYNA ile arenayı deneyebilirsin.";
  if(popup)Destroy(popup);var safe=transform.Find("SafeArea");var shade=Panel(safe,"Modal",new Vector2(.5f,.5f),Vector2.zero,new Vector2(1800,1000),-1);shade.color=new Color(0,0,0,.65f);shade.raycastTarget=true;popup=shade.gameObject;
  var card=Panel(shade.transform,"Card",new Vector2(.5f,.5f),Vector2.zero,new Vector2(650,370),1);Text(card.transform,title,new Vector2(0,130),new Vector2(580,65),38);Text(card.transform,body,new Vector2(0,12),new Vector2(570,190),24);
  var close=Button(card.transform,"Close","KAPAT",new Vector2(.5f,.5f),new Vector2(160,-132),new Vector2(220,66),0);close.onClick.AddListener(()=>Click("Close"));
  if(id=="Settings"){var mute=Button(card.transform,"Sound",AudioListener.volume>0?"SES: AÇIK":"SES: KAPALI",new Vector2(.5f,.5f),new Vector2(-150,-132),new Vector2(260,66),2);mute.onClick.AddListener(()=>{AudioListener.volume=AudioListener.volume>0?0:1;mute.GetComponentInChildren<TMP_Text>().text=AudioListener.volume>0?"SES: AÇIK":"SES: KAPALI";});}
 }
 RectTransform Rect(Transform parent,string name,Vector2 anchor,Vector2 position,Vector2 size){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=size;return r;}
 UnityEngine.UI.Image Panel(Transform p,string n,Vector2 a,Vector2 pos,Vector2 size,int sprite){var r=Rect(p,n,a,pos,size);var im=r.gameObject.AddComponent<UnityEngine.UI.Image>();if(sprite>=0){im.sprite=art[sprite];im.type=UnityEngine.UI.Image.Type.Sliced;}im.raycastTarget=false;return im;}
 void Icon(Transform p,int i,Vector2 pos,float size){var im=Panel(p,"Icon",Vector2.one*.5f,pos,Vector2.one*size,i);im.type=UnityEngine.UI.Image.Type.Simple;im.preserveAspect=true;}
 TMP_Text Text(Transform p,string value,Vector2 pos,Vector2 size,float fs){var r=Rect(p,"Label",Vector2.one*.5f,pos,size);var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=font;t.text=value;t.fontSize=fs;t.alignment=TextAlignmentOptions.Center;t.color=Color.white;t.raycastTarget=false;t.outlineColor=new Color(.07f,.06f,.12f);t.outlineWidth=.18f;return t;}
 UnityEngine.UI.Button Button(Transform p,string id,string label,Vector2 anchor,Vector2 pos,Vector2 size,int sprite){var im=Panel(p,id,anchor,pos,size,sprite);im.raycastTarget=true;var b=im.gameObject.AddComponent<UnityEngine.UI.Button>();b.targetGraphic=im;var c=b.colors;c.pressedColor=new Color(.72f,.78f,.9f);b.colors=c;if(label.Length>0)Text(im.transform,label,Vector2.zero,size-new Vector2(18,8),size.y>100?44:26);return b;}
 public void Construct(){
  var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=1;
  var safe=Rect(transform,"SafeArea",Vector2.zero,Vector2.zero,Vector2.zero);safe.anchorMax=Vector2.one;safe.offsetMin=safe.offsetMax=Vector2.zero;safe.gameObject.AddComponent<SafeAreaPanel>();
  var profile=Panel(safe,"Profile",new Vector2(0,1),new Vector2(213,-66),new Vector2(390,100),1);Icon(profile.transform,6,new Vector2(-142,0),75);Text(profile.transform,"WOOLLY",new Vector2(32,19),new Vector2(255,44),34);Text(profile.transform,"ÇAYLAK  •  SEVİYE 1",new Vector2(32,-22),new Vector2(255,32),20);
  for(int i=0;i<2;i++){var currency=Panel(safe,"Currency",new Vector2(1,1),new Vector2(-370+i*205,-62),new Vector2(190,70),1);Icon(currency.transform,4+i,new Vector2(-56,0),57);Text(currency.transform,"0",new Vector2(27,0),new Vector2(110,54),33);}
  Button(safe,"Settings","≡",new Vector2(1,1),new Vector2(-45,-62),new Vector2(74,74),1);
  string[] ids={"Shop","Hero","Quests","Friends"};string[] labels={"MAĞAZA","KARAKTER","GÖREVLER","ARKADAŞLAR"};int[] icons={7,10,12,8};for(int i=0;i<4;i++){var b=Button(safe,ids[i],"",new Vector2(0,.5f),new Vector2(82,194-i*125),new Vector2(134,110),3);Icon(b.transform,icons[i],new Vector2(0,15),68);Text(b.transform,labels[i],new Vector2(0,-35),new Vector2(130,35),20);}
  var badge=Panel(safe,"HeroBadge",new Vector2(.5f,1),new Vector2(0,-155),new Vector2(240,77),1);Text(badge.transform,"WOOLLY",new Vector2(0,8),new Vector2(230,43),34);Text(badge.transform,"REVOLVER USTASI",new Vector2(0,-23),new Vector2(230,24),17);
  var tip=Panel(safe,"Training",new Vector2(1,.5f),new Vector2(-167,40),new Vector2(295,164),1);Text(tip.transform,"ANTRENMAN",new Vector2(0,46),new Vector2(270,44),28);Text(tip.transform,"Hareket et. Siper al.\nBaktığın yöne ateş et.",new Vector2(0,-13),new Vector2(260,80),21);
  var mode=Button(safe,"Mode","",new Vector2(.5f,0),new Vector2(-80,87),new Vector2(530,118),1);Icon(mode.transform,10,new Vector2(-200,0),82);Text(mode.transform,"TEST ARENASI",new Vector2(37,20),new Vector2(350,50),32);Text(mode.transform,"TEK OYUNCULU  •  SERBEST ANTRENMAN",new Vector2(38,-26),new Vector2(355,35),16);
  Button(safe,"Play","OYNA",new Vector2(1,0),new Vector2(-182,87),new Vector2(325,118),0);
  Text(safe,"WOOLLY ARENA  /  TEST SÜRÜMÜ",new Vector2(0,-210),new Vector2(480,36),18);
 }
}
}
