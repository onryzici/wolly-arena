using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WoollyArena {
 public sealed class StartupLoadingScreen : MonoBehaviour {
  public TMP_FontAsset font;
  CanvasGroup group;
  UnityEngine.UI.RawImage wallpaper;
  UnityEngine.UI.Image progress;
  TMP_Text status;
  RectTransform safe;
  Texture2D artwork;
  Rect lastSafe;
  int lastWidth,lastHeight;

  IEnumerator Start() {
   DontDestroyOnLoad(gameObject);
   BuildView();
   // Present the artwork for a frame before beginning any expensive scene work.
   yield return null;
   float shownAt=Time.realtimeSinceStartup;
   var load=SceneManager.LoadSceneAsync("Lobby",LoadSceneMode.Single);
   if(load==null){status.text="Yükleme başlatılamadı. Oyunu yeniden aç.";yield break;}
   load.allowSceneActivation=false;
   while(load.progress<.9f || Time.realtimeSinceStartup-shownAt<1.8f){
    progress.fillAmount=Mathf.Clamp01(load.progress/.9f)*.94f;
    yield return null;
   }
   status.text="Lobi hazırlanıyor…";
   load.allowSceneActivation=true;
   while(!load.isDone)yield return null;
   LobbyScreen lobby;
   do {yield return null;lobby=FindFirstObjectByType<LobbyScreen>();}while(!lobby || !lobby.IsPresentationReady);
   progress.fillAmount=1;status.text="Hazır!";
   yield return new WaitForSecondsRealtime(.15f);
   for(float t=0;t<.35f;t+=Time.unscaledDeltaTime){group.alpha=1-Mathf.SmoothStep(0,1,t/.35f);yield return null;}
   Destroy(gameObject);
  }

  public void BuildView() {
   if(group)return;
   var ui=new GameObject("Startup Canvas",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler),typeof(UnityEngine.UI.GraphicRaycaster),typeof(CanvasGroup));
   ui.transform.SetParent(transform,false);
   var canvas=ui.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
   var scaler=ui.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
   group=ui.GetComponent<CanvasGroup>();
   var back=Rect("Wallpaper",ui.transform,Vector2.zero,Vector2.one);
   wallpaper=back.gameObject.AddComponent<UnityEngine.UI.RawImage>();
   artwork=Resources.Load<Texture2D>("MenuSkin/LoadingTrio4K");wallpaper.texture=artwork;wallpaper.color=artwork?Color.white:new Color(.04f,.07f,.09f);wallpaper.raycastTarget=true;
   // Modest dark strips protect UI contrast without obscuring the hero artwork.
   Panel("Title Shade",ui.transform,new Vector2(0,.87f),Vector2.one,new Color(0,.025f,.04f,.40f));
   Panel("Footer Shade",ui.transform,Vector2.zero,new Vector2(1,.13f),new Color(0,.025f,.04f,.68f));
   safe=Rect("Safe Area",ui.transform,Vector2.zero,Vector2.one);
   Label("Title","WOOLLY ARENA",safe,new Vector2(.15f,.90f),new Vector2(.85f,.99f),52,new Color(1,.88f,.56f));
   status=Label("Status","Yükleniyor…",safe,new Vector2(.25f,.061f),new Vector2(.75f,.108f),23,Color.white);
   var track=Panel("Progress Track",safe,new Vector2(.31f,.039f),new Vector2(.69f,.052f),new Color(.025f,.05f,.07f,1));
   var fill=Rect("Progress",track.transform,Vector2.zero,Vector2.one);fill.offsetMin=new Vector2(3,3);fill.offsetMax=new Vector2(-3,-3);
   progress=fill.gameObject.AddComponent<UnityEngine.UI.Image>();progress.color=new Color(1,.74f,.22f);progress.type=UnityEngine.UI.Image.Type.Filled;progress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;progress.fillAmount=0;progress.raycastTarget=false;
   // Image.Filled needs a sprite even for a flat-color progress bar.
   var pixel=Texture2D.whiteTexture;progress.sprite=Sprite.Create(pixel,new Rect(0,0,pixel.width,pixel.height),Vector2.one*.5f);
   RefreshLayout();
  }
  void Update(){if(safe && (Screen.width!=lastWidth||Screen.height!=lastHeight||Screen.safeArea!=lastSafe))RefreshLayout();}
  void RefreshLayout(){
   ApplyLayout(Screen.width,Screen.height,Screen.safeArea);
  }
  public void ApplyLayout(int width,int height,Rect safeArea){
   lastWidth=Mathf.Max(1,width);lastHeight=Mathf.Max(1,height);lastSafe=safeArea;
   safe.anchorMin=new Vector2(lastSafe.xMin/lastWidth,lastSafe.yMin/lastHeight);safe.anchorMax=new Vector2(lastSafe.xMax/lastWidth,lastSafe.yMax/lastHeight);
   // Aspect-fill, preserving proportions. The artwork has generous crop margins.
   if(artwork){float source=(float)artwork.width/artwork.height,screen=(float)lastWidth/lastHeight;
    wallpaper.uvRect=screen>source?new Rect(0,(1-source/screen)*.5f,1,source/screen):new Rect((1-screen/source)*.5f,0,screen/source,1);
   }
  }
  static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max){var t=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();t.SetParent(parent,false);t.anchorMin=min;t.anchorMax=max;t.offsetMin=t.offsetMax=Vector2.zero;return t;}
  static UnityEngine.UI.Image Panel(string name,Transform parent,Vector2 min,Vector2 max,Color color){var image=Rect(name,parent,min,max).gameObject.AddComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;return image;}
  TMP_Text Label(string name,string text,Transform parent,Vector2 min,Vector2 max,float size,Color color){var label=Rect(name,parent,min,max).gameObject.AddComponent<TextMeshProUGUI>();if(font)label.font=font;label.text=text;label.fontSize=size;label.fontStyle=FontStyles.Bold;label.alignment=TextAlignmentOptions.Center;label.color=color;label.raycastTarget=false;return label;}
  void OnDestroy(){if(progress&&progress.sprite){if(Application.isPlaying)Destroy(progress.sprite);else DestroyImmediate(progress.sprite);}if(artwork&&Application.isPlaying)Resources.UnloadAsset(artwork);}
 }
}
