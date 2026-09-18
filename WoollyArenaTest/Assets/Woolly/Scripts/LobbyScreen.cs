using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WoollyArena
{
    public sealed partial class LobbyScreen : MonoBehaviour
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
        Sprite frostPreview;
        Transform woollyHero;Animator woollyAnimator;
        readonly Animator[] characterAnimators=new Animator[CharacterDefinition.Count];
        bool loading;
        public bool IsPresentationReady { get; private set; }
        RectTransform characterScreen, characterPage, lobbyHeroFrame;
        TMP_Text characterPageCount; readonly TMP_Text[] characterStats=new TMP_Text[8];
        float pageSlide; bool pageDragging;
        TMP_Text characterName, characterRole, characterDescription, selectionNote;
        UnityEngine.UI.Button selectCharacterButton;
        readonly UnityEngine.UI.Image[] characterCardBorders = new UnityEngine.UI.Image[CharacterDefinition.Count];
        readonly Dictionary<Transform, Bounds> presentationBounds = new Dictionary<Transform, Bounds>();
        int previewCharacter;
        Material presentationBackdrop;
        Texture2D selectionBackdrop;
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
            if(backdropRenderer)presentationBackdrop=backdropRenderer.material;
            AnchorBottomNavigation();
            woollyHero=hero;woollyAnimator=animator;
            baseYaw = hero ? hero.eulerAngles.y : 160;
            lobbyHeroFrame=heroFrame;
            if(lobbyHeroFrame){lobbyHeroFrame.sizeDelta=new Vector2(540,500);lobbyHeroFrame.anchoredPosition=new Vector2(-36,50);}
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
            RefreshCareer();
            ArrangeLobbyActions();
            StartCoroutine(RefreshInitialPresentation());
            bool saved = RunCheckpointStore.TryLoad(RunSession.SavePath,out var checkpoint);
            var play = content.Find("Play");
            if (play) play.GetComponentInChildren<TMP_Text>().text = saved ? $"DEVAM ET\n<size=55%>{PlayableCharacter.Title(checkpoint.Character)} · DALGA {checkpoint.Wave:00}</size>" : "OYNA";
            if (saved && !content.Find("NewRun"))
            {
                if (play) ((RectTransform)play).anchoredPosition = new Vector2(548, -160);
                var fresh = MakeButton(content, "NewRun", "YENİ KOŞU", Vector2.one * .5f, new Vector2(548, -268), new Vector2(421, 84), "Button01_s_Blue", 25);
                fresh.onClick.AddListener(ConfirmNewRun);
            }
        }

        System.Collections.IEnumerator RefreshInitialPresentation(){
            // Animator skinning is not ready during the first Start. Never retain those initial bounds.
            yield return null;
            yield return null;
            presentationBounds.Clear();heroLocalBounds=default;
            if(animator){animator.Update(0);MeasurePresentation();}
            Canvas.ForceUpdateCanvases();ApplyLayout();
            IsPresentationReady=true;
        }
        void ArrangeLobbyActions(){
            var fresh=content.Find("NewRun") as RectTransform;if(fresh){fresh.sizeDelta=new Vector2(421,84);fresh.anchoredPosition=new Vector2(548,-268);}
            var mode=content.Find("Mode");if(!mode)return;
            ((RectTransform)mode).sizeDelta=new Vector2(426,318);((RectTransform)mode).anchoredPosition=new Vector2(548,103);
            var frame=mode.Find("MapFrame") as RectTransform;if(frame){frame.sizeDelta=new Vector2(382,152);frame.anchoredPosition=new Vector2(0,12);}
            var preview=mode.Find("MapFrame/ArenaPreview");if(preview){var image=preview.GetComponent<UnityEngine.UI.Image>();image.preserveAspect=true;image.rectTransform.sizeDelta=new Vector2(370,142);}
            foreach(var label in mode.GetComponentsInChildren<TMP_Text>()){
                if(label.text.Contains("KANYON")||label.text.Contains("KORUSU")){label.rectTransform.anchoredPosition=new Vector2(0,119);label.fontSize=30;}
                else if(label.text.Contains("HAYATTA")){label.rectTransform.anchoredPosition=new Vector2(0,-89);label.fontSize=22;}
                else if(label.text.Contains("BOSS")){label.rectTransform.anchoredPosition=new Vector2(0,-124);label.fontSize=18;}
            }
            foreach(var label in content.GetComponentsInChildren<TMP_Text>())if(label.text=="ARENA SENİ BEKLİYOR!")label.gameObject.SetActive(false);
        }
        void LateUpdate()
        {
            if(presentationBackdrop)presentationBackdrop.SetFloat("_AmbientTime",Time.unscaledTime);
            if (hero) hero.rotation = Quaternion.Euler(0, baseYaw, 0);
            if(CharacterScreenOpen&&characterPage){
                if(!pageDragging)pageSlide=Mathf.Lerp(pageSlide,0,1-Mathf.Exp(-16*Time.unscaledDeltaTime));
                characterPage.anchoredPosition=new Vector2(pageSlide,0);
                var keyboard=UnityEngine.InputSystem.Keyboard.current;
                if(keyboard?.leftArrowKey.wasPressedThisFrame==true)StepCharacter(-1);
                if(keyboard?.rightArrowKey.wasPressedThisFrame==true)StepCharacter(1);
            }
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
            // Compensate for the mohawk in BOTH previews; switching screens must not shrink Vera.
            desiredHeight *= PresentationId == 1 ? 1.18f : 1f;
            // Always solve from the cached unit-scale mesh, never multiply the animated current scale.
            float parentScale=hero.parent?Mathf.Abs(hero.parent.lossyScale.y):1;
            if(heroLocalBounds.size.y<=.01f)return;
            hero.localScale=Vector3.one*(desiredHeight/Mathf.Max(heroLocalBounds.size.y*parentScale,.01f));
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
            // Rebind may restore imported root transforms. Measure only in neutral unit space,
            // then restore the presentation transform so a later refresh cannot compound its scale.
            Vector3 savedScale=hero.localScale,savedPosition=hero.localPosition;
            Quaternion savedRotation=hero.localRotation;
            var mesh=new Mesh();bool first=true;Bounds measured=default;
            try{
                animator.Rebind();animator.SetFloat("Speed",0);animator.SetFloat("Playback",1);animator.Update(0);
                hero.localScale=Vector3.one;hero.localRotation=Quaternion.identity;hero.localPosition=Vector3.zero;
                foreach(var renderer in hero.GetComponentsInChildren<SkinnedMeshRenderer>()){
                    if(renderer.name.EndsWith("_Outline"))continue;
                    renderer.BakeMesh(mesh,false);
                    foreach(var vertex in mesh.vertices){
                        var point=hero.InverseTransformPoint(renderer.transform.TransformPoint(vertex));
                        if(first){measured=new Bounds(point,Vector3.zero);first=false;}else measured.Encapsulate(point);
                    }
                }
            }finally{
                hero.localScale=savedScale;hero.localRotation=savedRotation;hero.localPosition=savedPosition;Destroy(mesh);
            }
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
            if (id == "NewRun") { ConfirmNewRun(); return; }
            if (id == "Settings") { ShowPopup("AYARLAR", "ItemIcon_Gear", "SES VE GÖRÜNTÜ", "Müzik: Robo-Western — Kevin MacLeod\nincompetech.com · CC BY 4.0\ncreativecommons.org/licenses/by/4.0/", true); return; }
            if (id == "Hero" || id == "Profile") { ShowCharacterPicker(); return; }
            if (id == "Mode" || id == "Map") { ShowBiomePicker(); return; }
            if(id=="Quests"||id=="Gems"){ShowMissions(0);return;}
            if(id=="Gift"){ShowDaily();return;}
            if(id=="Ranking"){ShowRecords();return;}
            if(id=="Shop"||id=="Coins"){ShowCosmetics();return;}
            if(id=="Friends"){ShowPopup("ARKADAŞLAR","ItemIcon_Friend","TEK OYUNCULU MACERA","Bu sürümde çevrimiçi arkadaş sistemi yok.\nKendi rekorlarını Rekorlar menüsünden takip et.");return;}
            string title = id == "Shop" || id == "Coins" || id == "Gems" ? "MAĞAZA" : id == "Friends" ? "ARKADAŞLAR" : id == "Quests" ? "GÖREVLER" : id == "Ranking" ? "SIRALAMA" : "GÜNLÜK ÖDÜL";
            string icon = id == "Friends" ? "ItemIcon_Friend" : id == "Quests" ? "ItemIcon_MemoPad" : id == "Ranking" ? "ItemIcon_Medalstand" : id == "Gift" ? "ItemIcon_Gift_Blue" : "ItemIcon_Shop";
            ShowPopup(title, icon, "YAKINDA", "Bu bölüm henüz açılmadı.\n\nŞimdilik Kızıl Kanyon'da\nWoolly ile antrenman yapabilirsin.");
        }

        void ShowSelectedCharacter(int id=-1){
            if(id<0)id=PlayableCharacter.Selected;
            id=Mathf.Clamp(id,0,CharacterDefinition.Count-1);
            if(id>0&&!characterAnimators[id]){
                var created=PlayableCharacter.Create(woollyHero.parent,woollyAnimator.runtimeAnimatorController,true,woollyHero,id);
                if(!created)return;
                characterAnimators[id]=created;created.transform.SetPositionAndRotation(woollyHero.position,woollyHero.rotation);created.transform.localScale=woollyHero.localScale;
            }
            woollyHero.gameObject.SetActive(id==0);
            for(int i=1;i<characterAnimators.Length;i++)if(characterAnimators[i])characterAnimators[i].gameObject.SetActive(i==id);
            animator=id==0?woollyAnimator:characterAnimators[id];hero=animator.transform;
            heroRenderers=null;heroLocalBounds=new Bounds();animator.SetFloat("Speed",0);animator.SetFloat("Playback",1);
            MeasurePresentation();CharacterAccessories.Apply(hero,id);
            var profile=content.Find("Profile");
            if(profile){
                var portrait=profile.Find("CharacterPortrait");
                var image=portrait?portrait.GetComponent<UnityEngine.UI.Image>():Panel(profile,"CharacterPortrait",Vector2.one*.5f,new Vector2(-173,0),new Vector2(60,76),null);
                image.sprite=CharacterPortrait(id);image.type=UnityEngine.UI.Image.Type.Simple;image.preserveAspect=true;image.color=Color.white;image.enabled=image.sprite!=null;
                foreach(var label in profile.GetComponentsInChildren<TMP_Text>())if(CharacterDefinition.IsTitle(label.text)||label.text.Contains("ÇAYLAK")){label.rectTransform.anchoredPosition=new Vector2(-24,label.rectTransform.anchoredPosition.y);label.rectTransform.sizeDelta=new Vector2(198,label.rectTransform.sizeDelta.y);}
            }
            foreach(var label in content.GetComponentsInChildren<TMP_Text>(true)){
                if(CharacterDefinition.IsTitle(label.text)){
                    label.text=PlayableCharacter.Title(id);
                    if(label.transform.parent==content){label.rectTransform.anchoredPosition=new Vector2(-36,326);label.fontSize=48;}
                }
                if(CharacterDefinition.IsRole(label.text)||label.text=="MUŞTA DÖVÜŞÇÜSÜ"){
                    label.text=PlayableCharacter.Role(id);label.rectTransform.anchoredPosition=new Vector2(-36,285);
                }
            }
        }
        Sprite CharacterPortrait(int id){
            // Imported assets survive texture reimports; do not retain runtime sprites pointing at replaced textures.
            return Resources.Load<Sprite>("Characters/"+CharacterDefinition.PortraitName(id));
        }
        void RefreshCharacterPortraits(){
            if(!characterScreen)return;
            for(int i=0;i<CharacterDefinition.Count;i++){
                var slot=characterScreen.Find("CharacterThumbnail"+i+"/Portrait");
                if(!slot)continue;
                var image=slot.GetComponent<UnityEngine.UI.Image>();
                image.sprite=CharacterPortrait(i);image.enabled=image.sprite!=null;
            }
        }
        void ShowCharacterPicker()
        {
            if(CharacterScreenOpen)return;
            ClosePopup();
            previewCharacter=PlayableCharacter.Selected;
            if(!characterScreen)BuildCharacterScreen();
            characterScreen.gameObject.SetActive(true);
            content.gameObject.SetActive(false);
            if(bottomNavigation)bottomNavigation.gameObject.SetActive(false);
            RefreshCharacterPortraits();
            heroFrame=(RectTransform)characterPage.Find("CharacterPreview");
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
            Text(characterScreen,"KARAKTER SEÇ",new Vector2(-355,405),new Vector2(410,60),35,TextAlignmentOptions.Left);
            characterPageCount=Text(characterScreen,"",new Vector2(650,405),new Vector2(170,45),26);
            var swipe=Panel(characterScreen,"SwipeSurface",Vector2.one*.5f,new Vector2(0,5),new Vector2(1560,680),null);
            swipe.color=Color.clear;swipe.raycastTarget=true;swipe.gameObject.AddComponent<CharacterSwipe>().lobby=this;
            characterPage=Rect(characterScreen,"CharacterPage",Vector2.one*.5f,Vector2.zero,new Vector2(1600,900));
            Rect(characterPage,"CharacterPreview",Vector2.one*.5f,new Vector2(-315,-85),new Vector2(540,500));
            characterName=Text(characterPage,"",new Vector2(-315,325),new Vector2(650,60),49);
            characterRole=Text(characterPage,"",new Vector2(-315,280),new Vector2(560,35),23,color:Cyan);
            Text(characterScreen,"KARAKTER DEĞİŞTİRMEK İÇİN KAYDIR",new Vector2(-315,-395),new Vector2(690,28),16,color:new Color(.84f,.83f,1));
            var previous=MakeButton(characterScreen,"PreviousCharacter","‹",Vector2.one*.5f,new Vector2(-690,10),new Vector2(86,116),"Button01_s_Blue",58);
            var next=MakeButton(characterScreen,"NextCharacter","›",Vector2.one*.5f,new Vector2(63,10),new Vector2(86,116),"Button01_s_Blue",58);
            previous.onClick.AddListener(()=>StepCharacter(-1));next.onClick.AddListener(()=>StepCharacter(1));
            var detail=Panel(characterPage,"BaseStats",Vector2.one*.5f,new Vector2(420,126),new Vector2(570,380),"Button_Round03_Dark");
            Text(detail.transform,"TEMEL STATLAR",new Vector2(0,151),new Vector2(500,44),28,TextAlignmentOptions.Left);
            Text(detail.transform,"BAŞLANGIÇ SİLAHI · ALTIPATLAR I",new Vector2(0,113),new Vector2(500,30),18,TextAlignmentOptions.Left,color:Cyan);
            for(int i=0;i<8;i++){
                float x=i%2==0?-133:133;float y=56-(i/2)*62;
                Text(detail.transform,RunItem.StatName((RunStat)i).ToUpper(System.Globalization.CultureInfo.GetCultureInfo("tr-TR")),new Vector2(x,y+12),new Vector2(228,25),16,TextAlignmentOptions.Left,color:new Color(.74f,.78f,.9f));
                characterStats[i]=Text(detail.transform,"",new Vector2(x,y-15),new Vector2(228,36),28,TextAlignmentOptions.Left);
            }
            var trait=Panel(characterPage,"CharacterTrait",Vector2.one*.5f,new Vector2(420,-145),new Vector2(570,146),"Button_Round03_Dark");
            characterDescription=Text(trait.transform,"",Vector2.zero,new Vector2(512,136),18,TextAlignmentOptions.Left);
            characterDescription.textWrappingMode=TextWrappingModes.Normal;
            selectionNote=Text(characterScreen,"",new Vector2(420,-280),new Vector2(590,36),17,color:Cyan);
            selectCharacterButton=MakeButton(characterScreen,"SelectCharacter","SEÇ",Vector2.one*.5f,new Vector2(420,-350),new Vector2(510,78),"Button01_l_Yellow",27,true);
            selectCharacterButton.onClick.AddListener(()=>{PlayableCharacter.Selected=previewCharacter;CloseCharacterScreen();});

        }
        public void DragCharacterPage(float amount){if(!CharacterScreenOpen)return;pageDragging=true;pageSlide=Mathf.Clamp(amount*.35f,-120,120);}
        public void ReleaseCharacterPage(float amount){pageDragging=false;if(Mathf.Abs(amount)>=65)StepCharacter(amount<0?1:-1);}
        public void CancelCharacterSwipe(){pageDragging=false;pageSlide=0;}
        void StepCharacter(int direction){
            if(!CharacterScreenOpen)return;
            pageDragging=false;pageSlide=direction*110;
            PreviewCharacter((previewCharacter+direction+CharacterDefinition.Count)%CharacterDefinition.Count);
        }
        void PreviewCharacter(int id)
        {
            previewCharacter=id;baseYaw=160;RefreshCharacterPortraits();
            ShowSelectedCharacter(id);
            characterName.text=PlayableCharacter.Title(id);characterRole.text=PlayableCharacter.Role(id);
            characterDescription.text="<b>"+CharacterDefinition.Trait(id)+"</b>\n"+CharacterDefinition.Description(id)+"\n"+CharacterDefinition.GrowthDescription(id)+"\n"+CharacterDefinition.Power(id)+" · "+CharacterDefinition.PowerDescription(id);
            for(int i=0;i<8;i++)characterStats[i].text=CharacterDefinition.DisplayStat(id,(RunStat)i);
            characterPageCount.text=(id+1)+" / "+CharacterDefinition.Count;
            selectionNote.text=RunCheckpointStore.TryLoad(RunSession.SavePath,out var savedRun)?$"Kayıt: {PlayableCharacter.Title(savedRun.Character)} · Dalga {savedRun.Wave} · Seçim yeni koşuda geçerli.":"Başlangıç statları · Ekipmansız değerler";

            selectCharacterButton.GetComponentInChildren<TMP_Text>().text=id==PlayableCharacter.Selected?"LOBİYE DÖN":"KARAKTERİ SEÇ";
            Canvas.ForceUpdateCanvases();ApplyLayout();
        }
        void CloseCharacterScreen()
        {
            if(!CharacterScreenOpen)return;
            CancelCharacterSwipe();characterScreen.gameObject.SetActive(false);content.gameObject.SetActive(true);if(bottomNavigation)bottomNavigation.gameObject.SetActive(true);heroFrame=lobbyHeroFrame;
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
            if(selected && !selectionBackdrop)selectionBackdrop=Resources.Load<Texture2D>("MenuSkin/CharacterCamp");
            if(selectionBackdrop)presentationBackdrop.SetTexture("_SelectionBackdrop",selectionBackdrop);
            presentationBackdrop.SetFloat("_SelectionMode",selected && selectionBackdrop?1:0);

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
        void OnDestroy(){if(presentationBackdrop)Destroy(presentationBackdrop);if(frostPreview)Destroy(frostPreview);}

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
            string asset=id=="Play"||id=="StartFresh"||id=="Close"||id=="Biome0"||id=="SelectCharacter" ? "Button01_l_Yellow"
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

            heroFrame = Rect(content, "HeroFrame", Vector2.one * .5f, new Vector2(-36, 50), new Vector2(540, 500));
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
