using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WoollyArena
{
    public sealed class LobbyScreen : MonoBehaviour
    {
        public TMP_FontAsset font;
        public Material outlinedFontMaterial;
        public Sprite[] art;
        public Transform hero;
        public Animator animator;
        public Camera presentationCamera;
        public Transform contactShadow;
        public RectTransform content;
        public RectTransform heroFrame;
        public Bounds heroLocalBounds;
        public Renderer backdropRenderer;

        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        readonly Vector3[] corners = new Vector3[4];
        Renderer[] heroRenderers;
        RectTransform bottomNavigation, bottomBackground;
        GameObject popup;
        Sprite frostPreview;readonly Sprite[] characterPortraits=new Sprite[2];
        Transform woollyHero,veraHero;Animator woollyAnimator,veraAnimator;
        bool loading;
        RectTransform characterScreen, lobbyHeroFrame;
        TMP_Text characterName, characterRole, characterDescription, selectionNote;
        UnityEngine.UI.Button selectCharacterButton;
        readonly UnityEngine.UI.Image[] characterCardBorders = new UnityEngine.UI.Image[2];
        readonly Dictionary<Transform, Bounds> presentationBounds = new Dictionary<Transform, Bounds>();
        int previewCharacter;
        Material presentationBackdrop;
        bool CharacterScreenOpen => characterScreen && characterScreen.gameObject.activeSelf;
        int PresentationId => CharacterScreenOpen ? previewCharacter : PlayableCharacter.Selected;
        float baseYaw;
        public bool CanRotateHero => !loading && !popup;
        public void RotateHero(float degrees){if(CanRotateHero)baseYaw=Mathf.Repeat(baseYaw+degrees,360);}
        static readonly Color Ink = new Color32(8, 13, 29, 255);
        static readonly Color Blue = new Color32(71, 86, 229, 255);
        static readonly Color Cyan = new Color32(159, 235, 255, 255);

        void Start()
        {
            Application.targetFrameRate = 60; RunSession.ApplySound();
            AnchorBottomNavigation();
            woollyHero=hero;woollyAnimator=animator;
            baseYaw = hero ? hero.eulerAngles.y : 160;
            lobbyHeroFrame=heroFrame;
            ShowSelectedCharacter();
            if (animator) { animator.SetFloat("Speed", 0); animator.SetFloat("Playback", 1); }
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                ApplyPackageStyle(button);
                string id = button.name;
                button.onClick.AddListener(() => Click(id));
            }
            AttachHeroDrag(heroFrame);
            RefreshBiomeCard();
            bool saved = RunSession.HasCheckpoint;
            var play = content.Find("Play");
            if (play) play.GetComponentInChildren<TMP_Text>().text = saved ? "DEVAM ET" : "OYNA";
            if (saved && !content.Find("NewRun"))
            {
                if (play) ((RectTransform)play).anchoredPosition = new Vector2(548, -186);
                var fresh = MakeButton(content, "NewRun", "YENİ KOŞU", Vector2.one * .5f, new Vector2(548, -282), new Vector2(421, 52), "Button01_s_Blue", 25);
                fresh.onClick.AddListener(ConfirmNewRun);
            }
        }

        void LateUpdate()
        {
            if (hero) hero.rotation = Quaternion.Euler(0, baseYaw, 0);
            ApplyLayout();
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true) ClosePopup();
        }

        // Keep the central composition fitted, but dock navigation to the safe bottom edge.
        public void ApplyLayout()
        {
            if (!content) return;
            var safe = (RectTransform)content.parent;
            float scale = Mathf.Min(safe.rect.width / 1600f, safe.rect.height / 900f);
            if (scale <= 0) return;
            content.localScale = Vector3.one * scale;
            if(characterScreen)characterScreen.localScale=Vector3.one*scale;
            if (bottomNavigation)
            {
                bottomNavigation.localScale = Vector3.one * scale;
                if (bottomBackground)
                {
                    // Fill below the safe area without moving buttons into the home indicator.
                    var canvasRect = (RectTransform)transform;
                    safe.GetWorldCorners(corners);
                    float inset = Mathf.Max(0, canvasRect.InverseTransformPoint(corners[0]).y - canvasRect.rect.yMin) / scale;
                    bottomBackground.sizeDelta = new Vector2(canvasRect.rect.width / scale, 116 + inset);
                    bottomBackground.anchoredPosition = new Vector2(0, -394 - inset * .5f);
                }
            }
            if (!heroFrame || !hero || !presentationCamera) return;
            var canvas = GetComponent<Canvas>();
            var uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            heroFrame.GetWorldCorners(corners);
            Vector2 bottom = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            Vector2 top = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);
            float height = presentationCamera.pixelHeight;
            if (height <= 0 || top.y <= bottom.y) return;
            if (heroRenderers == null) heroRenderers = hero.GetComponentsInChildren<Renderer>();
            if (heroRenderers.Length == 0) return;
            Bounds bounds = HeroBounds();
            float desiredHeight = (top.y - bottom.y) / height * presentationCamera.orthographicSize * 2;
            // The mohawk must not shrink Vera's body relative to Woolly.
            desiredHeight *= PresentationId == 1 ? 1.10f : 1f;
            hero.localScale *= desiredHeight / Mathf.Max(bounds.size.y, .01f);
            bounds = HeroBounds();
            Vector3 floor = presentationCamera.ScreenToWorldPoint(new Vector3((top.x + bottom.x) * .5f, bottom.y, 7));
            hero.position += new Vector3(floor.x-bounds.center.x, floor.y-bounds.min.y, floor.z-bounds.center.z);
            bounds=HeroBounds();
            if (contactShadow)
            {
                // Anchor to the baked sole height, not imported animation bounds or UI padding.
                var soleScreen=presentationCamera.WorldToScreenPoint(new Vector3(bounds.center.x,bounds.min.y,bounds.center.z));
                contactShadow.position=presentationCamera.ScreenToWorldPoint(new Vector3(soleScreen.x,soleScreen.y+2*scale,soleScreen.z+.45f));
                contactShadow.localScale=new Vector3(desiredHeight*.47f,desiredHeight*.065f,1);
            }
        }

        void AttachHeroDrag(RectTransform frame)
        {
            if(!frame||frame.Find("Rotate Character"))return;
            var dragObject=new GameObject("Rotate Character",typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(LobbyHeroDrag));
            var rect=(RectTransform)dragObject.transform;rect.SetParent(frame,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=dragObject.GetComponent<UnityEngine.UI.Image>();image.color=Color.clear;image.raycastTarget=true;
            dragObject.GetComponent<LobbyHeroDrag>().lobby=this;
        }

        void MeasurePresentation()
        {
            if(presentationBounds.TryGetValue(hero,out heroLocalBounds))return;
            animator.Update(0);
            var mesh=new Mesh();bool first=true;Bounds measured=default;
            foreach(var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if(renderer.name.EndsWith("_Outline"))continue;
                renderer.BakeMesh(mesh);
                foreach(var vertex in mesh.vertices)
                {
                    var point=hero.InverseTransformPoint(renderer.transform.TransformPoint(vertex));
                    if(first){measured=new Bounds(point,Vector3.zero);first=false;}else measured.Encapsulate(point);
                }
            }
            Destroy(mesh);
            if(!first){heroLocalBounds=measured;presentationBounds[hero]=measured;}
        }

        void AnchorBottomNavigation()
        {
            if (!content || bottomNavigation) return;
            // Existing scenes keep their buttons and listeners; only their layout parent changes.
            var elements = new List<RectTransform>();
            foreach (Transform child in content)
            {
                var rect = child as RectTransform;
                if (rect && (child.name == "BottomBar" || child.name == "BottomBorder" ||
                    (rect.anchoredPosition.y < -330 && child.GetComponent<UnityEngine.UI.Button>())))
                    elements.Add(rect);
            }
            if (elements.Count == 0) return;
            bottomNavigation = Rect(content.parent, "BottomNavigation", new Vector2(.5f, 0), Vector2.zero, new Vector2(1600, 900));
            bottomNavigation.pivot = new Vector2(.5f, 0);
            foreach (var rect in elements)
            {
                Vector2 position = rect.anchoredPosition;
                rect.SetParent(bottomNavigation, false);
                rect.anchoredPosition = position;
                if (rect.name == "BottomBar") bottomBackground = rect;
            }
        }

        Bounds HeroBounds()
        {
            if (heroLocalBounds.size.y > .01f)
                return new Bounds(hero.TransformPoint(heroLocalBounds.center), Vector3.Scale(heroLocalBounds.size, hero.lossyScale));
            Bounds bounds = heroRenderers[0].bounds;
            foreach (var renderer in heroRenderers) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        public void Click(string id)
        {
            if (loading) return;
            if (id == "Play")
            {
                RunSession.ResumeRequested = RunSession.HasCheckpoint;
                loading = true;
                var button = content.Find("Play").GetComponent<Button>();
                button.interactable = false;
                button.GetComponentInChildren<TMP_Text>().text = "YÜKLENİYOR";
                SceneManager.LoadSceneAsync("TrainingArena");
                return;
            }
            if (id == "Close" || id == "Battle") { ClosePopup(); if(CharacterScreenOpen)CloseCharacterScreen(); return; }
            if (id == "Settings") { ShowPopup("AYARLAR", "ItemIcon_Gear", "SES VE GÖRÜNTÜ", "Yatay ekran · 60 FPS\n\nMüzik ve efektleri aşağıdaki düğmeyle\naçabilir veya kapatabilirsin.", true); return; }
            if (id == "Hero" || id == "Profile") { ShowCharacterPicker(); return; }
            if (id == "Mode" || id == "Map") { ShowBiomePicker(); return; }
            string title = id == "Shop" || id == "Coins" || id == "Gems" ? "MAĞAZA" : id == "Friends" ? "ARKADAŞLAR" : id == "Quests" ? "GÖREVLER" : id == "Ranking" ? "SIRALAMA" : "GÜNLÜK ÖDÜL";
            string icon = id == "Friends" ? "ItemIcon_Friend" : id == "Quests" ? "ItemIcon_MemoPad" : id == "Ranking" ? "ItemIcon_Medalstand" : id == "Gift" ? "ItemIcon_Gift_Blue" : "ItemIcon_Shop";
            ShowPopup(title, icon, "YAKINDA", "Bu bölüm henüz açılmadı.\n\nŞimdilik Kızıl Kanyon'da\nWoolly ile antrenman yapabilirsin.");
        }

        void ShowSelectedCharacter(int id=-1){
            if(id<0)id=PlayableCharacter.Selected;
            if(id==1){
                if(!veraHero){veraAnimator=PlayableCharacter.Create(woollyHero.parent,woollyAnimator.runtimeAnimatorController,true);if(!veraAnimator)return;veraHero=veraAnimator.transform;veraHero.SetPositionAndRotation(woollyHero.position,woollyHero.rotation);veraHero.localScale=woollyHero.localScale;}
                woollyHero.gameObject.SetActive(false);veraHero.gameObject.SetActive(true);hero=veraHero;animator=veraAnimator;
            }else{if(veraHero)veraHero.gameObject.SetActive(false);woollyHero.gameObject.SetActive(true);hero=woollyHero;animator=woollyAnimator;}
            heroRenderers=null;heroLocalBounds=new Bounds();animator.SetFloat("Speed",0);animator.SetFloat("Playback",1);
            MeasurePresentation();
            foreach(var label in content.GetComponentsInChildren<TMP_Text>(true)){
                if(label.text=="WOOLLY"||label.text=="PUNK VERA"){
                    label.text=PlayableCharacter.Title(id);
                    if(label.transform.parent==content){label.rectTransform.anchoredPosition=new Vector2(-36,326);label.fontSize=48;}
                }
                if(label.text=="REVOLVER USTASI"||label.text=="MUŞTA DÖVÜŞÇÜSÜ"){
                    label.text=PlayableCharacter.Role(id);label.rectTransform.anchoredPosition=new Vector2(-36,285);
                }
            }
        }
        Sprite CharacterPortrait(int id){
            if(!characterPortraits[id]){var texture=Resources.Load<Texture2D>("Characters/"+(id==1?"PunkVeraLobbyPortrait":"WoollyPortrait"));if(texture)characterPortraits[id]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f);}
            return characterPortraits[id];
        }
        void ShowCharacterPicker()
        {
            if(CharacterScreenOpen)return;
            ClosePopup();
            previewCharacter=PlayableCharacter.Selected;
            if(!characterScreen)BuildCharacterScreen();
            characterScreen.gameObject.SetActive(true);
            content.gameObject.SetActive(false);
            heroFrame=(RectTransform)characterScreen.Find("CharacterPreview");
            SetSelectionBackdrop(true);
            PreviewCharacter(previewCharacter);
            UpdateNavigation(true);
        }
        void BuildCharacterScreen()
        {
            characterScreen=Rect(content.parent,"CharacterSelectionScreen",Vector2.one*.5f,Vector2.zero,new Vector2(1600,900));
            var top=Panel(characterScreen,"Header",Vector2.one*.5f,new Vector2(0,405),new Vector2(4000,90),null);top.color=new Color32(35,33,73,255);
            var back=MakeButton(characterScreen,"BackToLobby","GERİ",Vector2.one*.5f,new Vector2(-676,405),new Vector2(190,65),"Button01_s_Blue",27);
            back.onClick.AddListener(CloseCharacterScreen);
            Text(characterScreen,"KARAKTERLER",new Vector2(-370,405),new Vector2(360,60),37,TextAlignmentOptions.Left);
            Text(characterScreen,"2 / 2 KARAKTER",new Vector2(604,405),new Vector2(340,45),25);
            var preview=Rect(characterScreen,"CharacterPreview",Vector2.one*.5f,new Vector2(-410,-20),new Vector2(510,440));
            AttachHeroDrag(preview);
            characterName=Text(characterScreen,"",new Vector2(-410,314),new Vector2(560,60),46);
            characterRole=Text(characterScreen,"",new Vector2(-410,267),new Vector2(540,36),23,color:Cyan);
            Text(characterScreen,"ÇEVİRMEK İÇİN SÜRÜKLE",new Vector2(-410,-282),new Vector2(510,35),19,color:new Color(.84f,.83f,1));
            var collection=Panel(characterScreen,"Collection",Vector2.one*.5f,new Vector2(305,155),new Vector2(790,337),"Button_Round03_Dark");
            Text(collection.transform,"KOLEKSİYONUN",new Vector2(0,133),new Vector2(700,40),27,TextAlignmentOptions.Left);
            for(int i=0;i<2;i++){
                int choice=i;
                var button=MakeButton(collection.transform,"PreviewCharacter"+i,"",Vector2.one*.5f,new Vector2(i==0?-185:185,-16),new Vector2(345,254),"BorderFrame_Round20_Single_Dark");
                characterCardBorders[i]=(UnityEngine.UI.Image)button.targetGraphic;
                var portrait=Panel(button.transform,"Portrait",Vector2.one*.5f,new Vector2(-75,8),new Vector2(148,224),null);portrait.sprite=CharacterPortrait(i);portrait.type=UnityEngine.UI.Image.Type.Simple;portrait.preserveAspect=true;
                Text(button.transform,PlayableCharacter.Title(i),new Vector2(78,57),new Vector2(167,40),i==0?26:23);
                Text(button.transform,i==0?"REVOLVER":"MUŞTA",new Vector2(78,14),new Vector2(166,32),20,color:Cyan);
                Text(button.transform,"SEVİYE 1",new Vector2(78,-55),new Vector2(166,32),19);
                button.onClick.AddListener(()=>PreviewCharacter(choice));
            }
            var detail=Panel(characterScreen,"CharacterDetails",Vector2.one*.5f,new Vector2(305,-112),new Vector2(790,173),"Button_Round03_Dark");
            characterDescription=Text(detail.transform,"",new Vector2(0,22),new Vector2(720,80),26);
            characterDescription.textWrappingMode=TextWrappingModes.Normal;
            selectionNote=Text(detail.transform,"",new Vector2(0,-51),new Vector2(730,35),19,color:Cyan);
            selectCharacterButton=MakeButton(characterScreen,"SelectCharacter","SEÇ VE LOBİYE DÖN",Vector2.one*.5f,new Vector2(305,-262),new Vector2(580,84),"Button01_l_Yellow",32,true);
            selectCharacterButton.onClick.AddListener(()=>{PlayableCharacter.Selected=previewCharacter;CloseCharacterScreen();});
        }
        void PreviewCharacter(int id)
        {
            previewCharacter=id;baseYaw=160;
            ShowSelectedCharacter(id);
            characterName.text=PlayableCharacter.Title(id);characterRole.text=PlayableCharacter.Role(id);
            characterDescription.text=id==1?"Punk ruhu, sıkı gard.\nVera muştalarıyla dövüşe hazır.":"Sakin nişan, hızlı çekiş.\nWoolly revolveriyle arenaya hazır.";
            selectionNote.text=RunSession.HasCheckpoint?"Seçim yeni koşuda geçerli; kayıtlı koşun korunur.":"İki karakter de aynı başlangıç gücüyle oyuna girer.";
            for(int i=0;i<2;i++)characterCardBorders[i].color=i==id?new Color(.72f,.86f,1):new Color(.63f,.63f,.76f);
            selectCharacterButton.GetComponentInChildren<TMP_Text>().text=id==PlayableCharacter.Selected?"SEÇİLİ · LOBİYE DÖN":"SEÇ VE LOBİYE DÖN";
            Canvas.ForceUpdateCanvases();ApplyLayout();
        }
        void CloseCharacterScreen()
        {
            if(!CharacterScreenOpen)return;
            characterScreen.gameObject.SetActive(false);content.gameObject.SetActive(true);heroFrame=lobbyHeroFrame;
            SetSelectionBackdrop(false);ShowSelectedCharacter();UpdateNavigation(false);Canvas.ForceUpdateCanvases();ApplyLayout();
        }
        void UpdateNavigation(bool selection)
        {
            if(!bottomNavigation)return;
            foreach(var button in bottomNavigation.GetComponentsInChildren<UnityEngine.UI.Button>()){
                bool selected=selection?button.name=="Hero":button.name=="Battle";
                var image=button.targetGraphic as UnityEngine.UI.Image;
                if(image){image.sprite=SpriteFor(selected?"Button01_s_Blue":"Button_Round03_Dark");image.color=Color.white;}
            }
        }
        void SetSelectionBackdrop(bool selected)
        {
            if(!backdropRenderer)return;
            if(!presentationBackdrop)presentationBackdrop=backdropRenderer.material;
            presentationBackdrop.SetFloat("_SelectionMode",selected?1:0);
            if(selected)presentationBackdrop.SetTexture("_SelectionPattern",Resources.Load<Texture2D>("Characters/SelectionPattern"));
        }
        void RefreshBiomeCard(){
            var mode=content.Find("Mode");if(!mode)return;
            var preview=mode.Find("MapFrame/ArenaPreview");if(preview)preview.GetComponent<Image>().sprite=BiomePreview(ArenaBiome.Selected);
            foreach(var label in mode.GetComponentsInChildren<TMP_Text>()){
                if(label.text.Contains("KANYON")||label.text.Contains("KORUSU"))label.text=ArenaBiome.Title(ArenaBiome.Selected);
                else if(label.text.Contains("ANTRENMAN")||label.text.Contains("HAYATTA"))label.text="20 DALGA · HAYATTA KAL";
                else if(label.text.Contains("RAKİP")||label.text.Contains("BOSS"))label.text="5 / 10 / 15 / 20 · BOSS";
            }
        }
        Sprite BiomePreview(int biome){
            if(biome==0)return SpriteFor("ArenaPreview");
            if(!frostPreview){var texture=Resources.Load<Texture2D>("Biomes/FrostPreview")??Resources.Load<Texture2D>("Biomes/FrostGround");if(texture)frostPreview=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f);}
            return frostPreview;
        }
        void ShowBiomePicker(){
            ShowPopup("HARİTA SEÇ", "ItemIcon_Map", "", "");
            var card=popup.transform.Find("Dialog");
            foreach(Transform child in card)if(child.name=="ItemIcon_Map"||child.name=="Label")child.gameObject.SetActive(false);
            Text(card,RunSession.HasCheckpoint?"Yeni koşu haritası · Kayıtlı koşun korunur":"20 DALGA · 4 BOSS",new Vector2(0,110),new Vector2(590,38),21,color:Cyan);
            for(int i=0;i<2;i++){
                int choice=i;
                var button=MakeButton(card,"Biome"+i,ArenaBiome.Title(i),Vector2.one*.5f,new Vector2(i==0?-151:151,-30),new Vector2(280,220),i==0?"Button01_l_Yellow":"Button01_s_Blue",23,true);
                var label=button.GetComponentInChildren<TMP_Text>();label.rectTransform.anchoredPosition=new Vector2(0,-73);label.rectTransform.sizeDelta=new Vector2(270,42);
                var preview=Panel(button.transform,"BiomePreview",Vector2.one*.5f,new Vector2(0,26),new Vector2(254,138),null);preview.sprite=BiomePreview(i);preview.type=Image.Type.Simple;preview.preserveAspect=true;
                button.onClick.AddListener(()=>{ArenaBiome.Selected=choice;RefreshBiomeCard();ClosePopup();});
            }
        }
        void OnDestroy(){if(presentationBackdrop)Destroy(presentationBackdrop);if(frostPreview)Destroy(frostPreview);foreach(var portrait in characterPortraits)if(portrait)Destroy(portrait);}

        void ConfirmNewRun()
        {
            ShowPopup("YENİ KOŞU", "ItemIcon_Battle", "MEVCUT KOŞU BIRAKILACAK", "Yeni bir koşuya başlamak kayıtlı\nkoşunun yerini alır. Devam etmek istiyor musun?");
            var card = popup.transform.Find("Dialog");
            var close = card.Find("Close");
            if (close) { ((RectTransform)close).anchoredPosition = new Vector2(-155, -191); close.GetComponentInChildren<TMP_Text>().text = "VAZGEÇ"; }
            var start = MakeButton(card, "StartFresh", "BAŞLAT", Vector2.one * .5f, new Vector2(155, -191), new Vector2(240, 76), "Button01_l_Yellow", 27, true);
            start.onClick.AddListener(() =>
            {
                if (loading) return;
                string error = RunSession.ClearCheckpoint();
                if (error != null) { ShowPopup("KAYIT", "ItemIcon_Gear", "YENİ KOŞU BAŞLATILAMADI", "Cihazın depolama alanını kontrol et."); return; }
                RunSession.ResumeRequested = false; loading = true; SceneManager.LoadSceneAsync("TrainingArena");
            });
        }
        public void ClosePopup()
        {
            if (!popup) { if(CharacterScreenOpen)CloseCharacterScreen(); return; }
            popup.SetActive(false);
            Destroy(popup);
            popup = null;
        }

        void ShowPopup(string title, string icon, string subtitle, string body, bool sound = false)
        {
            if(popup){popup.SetActive(false);Destroy(popup);popup=null;}
            var shade = Panel(transform, "Modal", Vector2.one * .5f, Vector2.zero, Vector2.zero, null);
            shade.rectTransform.anchorMin = Vector2.zero;
            shade.rectTransform.anchorMax = Vector2.one;
            shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
            shade.color = new Color(.02f, .04f, .12f, .78f);
            shade.raycastTarget = true;
            shade.gameObject.AddComponent<Button>().onClick.AddListener(ClosePopup);
            popup = shade.gameObject;
            var card = Panel(shade.transform, "Dialog", Vector2.one * .5f, Vector2.zero, new Vector2(650, 490), "Button_Round03_Dark");
            card.rectTransform.localScale = content.localScale;
            card.raycastTarget = true;
            card.gameObject.AddComponent<Button>().transition = Selectable.Transition.None;
            var header = Panel(card.transform, "TitleBar", Vector2.one * .5f, new Vector2(0, 190), new Vector2(620, 85), "BorderFrame_Round20_Single_Dark");
            header.color = new Color32(134, 158, 255, 255);
            Text(header.transform, title, Vector2.zero, new Vector2(510, 60), 36);
            Icon(card.transform, icon, new Vector2(0, 82), 94);
            Text(card.transform, subtitle, new Vector2(0, 8), new Vector2(570, 42), 29, color: Cyan);
            Text(card.transform, body, new Vector2(0, -73), new Vector2(575, 132), 24);
            var close = MakeButton(card.transform, "Close", sound ? "KAPAT" : "TAMAM", Vector2.one * .5f, new Vector2(sound ? 155 : 0, -191), new Vector2(sound ? 240 : 290, 76), "Button01_l_Yellow", 29, true);
            close.onClick.AddListener(ClosePopup);
            if (sound)
            {
                var mute = MakeButton(card.transform, "Sound", AudioListener.volume > 0 ? "SES: AÇIK" : "SES: KAPALI", Vector2.one * .5f, new Vector2(-145, -191), new Vector2(260, 76), "Button01_s_Blue", 25);
                mute.onClick.AddListener(() => { RunSession.ToggleSound(); mute.GetComponentInChildren<TMP_Text>().text = AudioListener.volume > 0 ? "SES: AÇIK" : "SES: KAPALI"; });
            }
        }

        void ApplyPackageStyle(UnityEngine.UI.Button button){
            var image=button.targetGraphic as UnityEngine.UI.Image;if(!image)return;
            string id=button.name;
            string asset=id=="Play"||id=="StartFresh"||id=="Close"||id=="Biome0" ? "Button01_l_Yellow"
                : id=="Profile"||id=="Coins"||id=="Gems"||id=="Hero" ? "BorderFrame_Round20_Single_Dark"
                : id=="Mode"||id.StartsWith("Character") ? "Button_Round03_Dark"
                : id=="Battle" ? "Button01_s_Blue"
                : id=="Shop"||id=="Quests"||id=="Map" ? "Button_Round03_Dark"
                : "Button01_s_Blue";
            var sprite=SpriteFor(asset);
            if(!sprite)return;
            image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Color.white;
            // Keep the pack's original highlight, rim and depth instead of covering it with a tint.
            var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=Color.white;
            colors.pressedColor=new Color(.87f,.92f,1);colors.disabledColor=new Color(.65f,.68f,.73f);button.colors=colors;
        }
        Sprite SpriteFor(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            if (sprites.Count == 0) foreach (var sprite in art) if (sprite) sprites[sprite.name] = sprite;
            return sprites.TryGetValue(name, out var result) ? result : null;
        }

        RectTransform Rect(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        Image Panel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, string sprite)
        {
            var rect = Rect(parent, name, anchor, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = SpriteFor(sprite);
            image.type = image.sprite && image.sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.raycastTarget = false;
            return image;
        }

        Image Icon(Transform parent, string sprite, Vector2 position, float size)
        {
            var icon = Panel(parent, sprite, Vector2.one * .5f, position, Vector2.one * size, sprite);
            icon.type = Image.Type.Simple;
            icon.preserveAspect = true;
            return icon;
        }

        TMP_Text Text(Transform parent, string value, Vector2 position, Vector2 size, float fontSize, TextAlignmentOptions alignment = TextAlignmentOptions.Center, Color? color = null, bool outline = true)
        {
            var rect = Rect(parent, "Label", Vector2.one * .5f, position, size);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSharedMaterial = outline && outlinedFontMaterial ? outlinedFontMaterial : font.material;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color ?? Color.white;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.extraPadding = true;
            return text;
        }

        Button MakeButton(Transform parent, string id, string label, Vector2 anchor, Vector2 position, Vector2 size, string sprite, float fontSize = 28, bool darkText = false)
        {
            var image = Panel(parent, id, anchor, position, size, sprite);
            image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(.78f, .84f, .96f);
            colors.disabledColor = new Color(.65f, .65f, .65f);
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.gameObject.AddComponent<LobbyButtonFeedback>();
            if (!string.IsNullOrEmpty(label)) Text(image.transform, label, Vector2.zero, size - new Vector2(30, 10), fontSize, color: darkText ? Ink : Color.white, outline: !darkText);
            ApplyPackageStyle(button);
            return button;
        }

        void SideButton(string id, string label, string icon, Vector2 position)
        {
            var button = MakeButton(content, id, "", Vector2.one * .5f, position, new Vector2(112, 112), "Button_Round03_Dark");
            Icon(button.transform, icon, new Vector2(0, 15), 82);
            Text(button.transform, label, new Vector2(0, -35), new Vector2(140, 30), 20);
        }

        void Currency(string id, string icon, string value, float x, string plusSprite)
        {
            var bar = MakeButton(content, id, "", Vector2.one * .5f, new Vector2(x, 402), new Vector2(204, 48), "BorderFrame_Round20_Single_Dark");
            bar.targetGraphic.color = new Color32(85, 91, 120, 255);
            Icon(bar.transform, icon, new Vector2(-91, 0), 67);
            Text(bar.transform, value, new Vector2(-7, 0), new Vector2(98, 44), 29);
            var plus = Panel(bar.transform, "Add", Vector2.one * .5f, new Vector2(73, 0), new Vector2(45, 47), plusSprite);
            Text(plus.transform, "+", new Vector2(0, 2), new Vector2(40, 42), 37);
        }

        public void Construct()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            gameObject.AddComponent<GraphicRaycaster>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 1;
            var safe = Rect(transform, "SafeArea", Vector2.zero, Vector2.zero, Vector2.zero);
            safe.anchorMax = Vector2.one;
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            safe.gameObject.AddComponent<SafeAreaPanel>();
            content = Rect(safe, "Content", Vector2.one * .5f, Vector2.zero, new Vector2(1600, 900));

            var top = Panel(content, "TopBar", Vector2.one * .5f, new Vector2(0, 405), new Vector2(4000, 90), null);
            top.color = new Color32(39, 46, 145, 155);
            Icon(content, "ItemIcon_Battle", new Vector2(-733, 402), 57);
            Text(content, "WOOLLY ARENA", new Vector2(-553, 402), new Vector2(285, 53), 30, TextAlignmentOptions.Left);
            Currency("Coins", "ItemIcon_Money_Coin", "0", 346, "Button01_s_Yellow");
            Currency("Gems", "ItemIcon_Gem_Diamond_Purple", "0", 600, "Button01_s_Purple");
            var menu = MakeButton(content, "Settings", "", Vector2.one * .5f, new Vector2(744, 402), new Vector2(64, 65), "Button_Round03_Dark");
            Icon(menu.transform, "Icon_Menu_Hamburger", Vector2.zero, 38);

            var profile = MakeButton(content, "Profile", "", Vector2.one * .5f, new Vector2(-560, 306), new Vector2(424, 80), "BorderFrame_Round20_Single_Dark");
            Text(profile.transform, "WOOLLY", new Vector2(-75, 9), new Vector2(226, 42), 30, TextAlignmentOptions.Left);
            Text(profile.transform, "ÇAYLAK · SEVİYE 1", new Vector2(-75, -22), new Vector2(226, 26), 16, TextAlignmentOptions.Left, Cyan, false);
            Icon(profile.transform, "ItemIcon_Trophy_Gold", new Vector2(111, 1), 63);
            Text(profile.transform, "0", new Vector2(169, 1), new Vector2(48, 40), 31);

            var gift = MakeButton(content, "Gift", "", Vector2.one * .5f, new Vector2(-593, 192), new Vector2(358, 96), "BorderFrame_Round20_Single_Dark");
            Icon(gift.transform, "ItemIcon_Chest_Gold", new Vector2(-126, 7), 115);
            Text(gift.transform, "GÜNLÜK ÖDÜL", new Vector2(42, 18), new Vector2(222, 38), 25);
            Text(gift.transform, "YAKINDA", new Vector2(42, -20), new Vector2(215, 30), 18, color: Cyan, outline: false);
            SideButton("Friends", "Arkadaşlar", "ItemIcon_Friend", new Vector2(-708, 26));
            SideButton("Quests", "Görevler", "ItemIcon_MemoPad", new Vector2(-708, -112));
            SideButton("Ranking", "Sıralama", "ItemIcon_Medalstand", new Vector2(-562, 26));
            SideButton("Gift", "Hediyeler", "ItemIcon_Gift_Blue", new Vector2(-562, -112));

            heroFrame = Rect(content, "HeroFrame", Vector2.one * .5f, new Vector2(-36, 15), new Vector2(540, 430));
            Text(content, "WOOLLY", new Vector2(-36, 300), new Vector2(430, 66), 53);
            Text(content, "REVOLVER USTASI", new Vector2(-36, 258), new Vector2(350, 35), 20, color: Cyan);
            var heroInfo = MakeButton(content, "Hero", "", Vector2.one * .5f, new Vector2(-36, -248), new Vector2(284, 53), "BorderFrame_Round20_Single_Dark");
            Panel(heroInfo.transform, "LevelFill", Vector2.one * .5f, new Vector2(-34, 0), new Vector2(204, 38), "Slider_Basic02_Fill_Blue");
            Icon(heroInfo.transform, "ItemIcon_Star_Blue", new Vector2(-136, 3), 74);
            Text(heroInfo.transform, "1", new Vector2(-136, 4), new Vector2(55, 44), 30);
            Text(heroInfo.transform, "SEVİYE 1", new Vector2(22, 0), new Vector2(213, 38), 24);

            var mode = MakeButton(content, "Mode", "", Vector2.one * .5f, new Vector2(548, 74), new Vector2(426, 360), "Button_Round03_Dark");
            Text(mode.transform, "KIZIL KANYON", new Vector2(0, 137), new Vector2(380, 44), 33);
            var previewFrame = Panel(mode.transform, "MapFrame", Vector2.one * .5f, new Vector2(0, 15), new Vector2(390, 184), "BorderFrame_Round20_Single_Dark");
            previewFrame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            Panel(previewFrame.transform, "ArenaPreview", Vector2.one * .5f, Vector2.zero, new Vector2(378, 173), "ArenaPreview").type = Image.Type.Simple;
            Text(mode.transform, "SERBEST ANTRENMAN", new Vector2(0, -103), new Vector2(375, 34), 23, color: Cyan);
            Text(mode.transform, "4 RAKİP  ·  TEK OYUNCULU", new Vector2(0, -143), new Vector2(376, 30), 19);
            MakeButton(content, "Play", "OYNA", Vector2.one * .5f, new Vector2(548, -213), new Vector2(421, 124), "Button01_l_Yellow", 53, true);
            Text(content, "ARENA SENİ BEKLİYOR!", new Vector2(548, -290), new Vector2(420, 32), 19);

            var nav = Panel(content, "BottomBar", Vector2.one * .5f, new Vector2(0, -394), new Vector2(4000, 116), null);
            nav.color = new Color32(28, 33, 57, 255);
            var line = Panel(content, "BottomBorder", Vector2.one * .5f, new Vector2(0, -334), new Vector2(4000, 5), null);
            line.color = Ink;
            string[] ids = { "Shop", "Hero", "Battle", "Quests", "Map" };
            string[] labels = { "MAĞAZA", "KARAKTER", "SAVAŞ", "GÖREVLER", "HARİTA" };
            string[] icons = { "ItemIcon_Shop", "ItemIcon_Book_1_Purple", "ItemIcon_Battle", "ItemIcon_MemoPad", "ItemIcon_Map" };
            for (int i = 0; i < ids.Length; i++)
            {
                bool selected = i == 2;
                var tab = MakeButton(content, ids[i], "", Vector2.one * .5f, new Vector2((i - 2) * 286, selected ? -383 : -393), new Vector2(266, selected ? 139 : 111), selected ? "Menu_BottomBtn_TabFocus" : null);
                tab.targetGraphic.color = selected ? Blue : new Color(1, 1, 1, 0);
                Icon(tab.transform, icons[i], new Vector2(0, selected ? 16 : 14), selected ? 83 : 66);
                Text(tab.transform, labels[i], new Vector2(0, selected ? -41 : -34), new Vector2(244, 32), selected ? 25 : 21);
            }
        }
    }
}
