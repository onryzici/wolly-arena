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
        GameObject popup;
        bool loading;
        float baseYaw;
        static readonly Color Ink = new Color32(8, 13, 29, 255);
        static readonly Color Blue = new Color32(71, 86, 229, 255);
        static readonly Color Cyan = new Color32(159, 235, 255, 255);

        void Start()
        {
            Application.targetFrameRate = 60;
            baseYaw = hero ? hero.eulerAngles.y : 160;
            if (animator) { animator.SetFloat("Speed", 0); animator.SetFloat("Playback", 1); }
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                string id = button.name;
                button.onClick.AddListener(() => Click(id));
            }
        }

        void LateUpdate()
        {
            if (hero) hero.rotation = Quaternion.Euler(0, baseYaw + Mathf.Sin(Time.unscaledTime * .55f) * 5, 0);
            ApplyLayout();
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true) ClosePopup();
        }

        // Fit the complete composition inside the safe area, including on tablets.
        public void ApplyLayout()
        {
            if (!content || !heroFrame || !hero || !presentationCamera) return;
            var safe = (RectTransform)content.parent;
            float scale = Mathf.Min(safe.rect.width / 1600f, safe.rect.height / 900f);
            content.localScale = Vector3.one * scale;
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
            hero.localScale *= desiredHeight / Mathf.Max(bounds.size.y, .01f);
            bounds = HeroBounds();
            Vector3 center = presentationCamera.ScreenToWorldPoint(new Vector3((top.x + bottom.x) * .5f, (top.y + bottom.y) * .5f, 7));
            hero.position += center - bounds.center;
            if (contactShadow)
            {
                contactShadow.position = presentationCamera.ScreenToWorldPoint(new Vector3((top.x + bottom.x) * .5f, bottom.y + 5 * scale, 7.7f));
                contactShadow.localScale = new Vector3(desiredHeight * .65f, desiredHeight * .10f, 1);
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
                loading = true;
                var button = content.Find("Play").GetComponent<Button>();
                button.interactable = false;
                button.GetComponentInChildren<TMP_Text>().text = "YÜKLENİYOR";
                SceneManager.LoadSceneAsync("TrainingArena");
                return;
            }
            if (id == "Close" || id == "Battle") { ClosePopup(); return; }
            if (id == "Settings") { ShowPopup("AYARLAR", "ItemIcon_Gear", "SES VE GÖRÜNTÜ", "Yatay ekran · 60 FPS\n\nMüzik ve efektleri aşağıdaki düğmeyle\naçabilir veya kapatabilirsin.", true); return; }
            if (id == "Hero" || id == "Profile") { ShowPopup("WOOLLY", "ItemIcon_Battle", "REVOLVER USTASI", "8 mermi · Hızlı yeniden doldurma\n\nWASD: hareket     Shift: koşu\nFare: nişan ve ateş     R: doldurma"); return; }
            if (id == "Mode" || id == "Map") { ShowPopup("KIZIL KANYON", "ItemIcon_Map", "SERBEST ANTRENMAN", "Dört rakibe karşı arenaya çık.\nSiperleri kullan, hareket et ve nişan al.\n\nYenilen rakipler yeniden doğar."); return; }
            string title = id == "Shop" || id == "Coins" || id == "Gems" ? "MAĞAZA" : id == "Friends" ? "ARKADAŞLAR" : id == "Quests" ? "GÖREVLER" : id == "Ranking" ? "SIRALAMA" : "GÜNLÜK ÖDÜL";
            string icon = id == "Friends" ? "ItemIcon_Friend" : id == "Quests" ? "ItemIcon_MemoPad" : id == "Ranking" ? "ItemIcon_Medalstand" : id == "Gift" ? "ItemIcon_Gift_Blue" : "ItemIcon_Shop";
            ShowPopup(title, icon, "YAKINDA", "Bu bölüm henüz açılmadı.\n\nŞimdilik Kızıl Kanyon'da\nWoolly ile antrenman yapabilirsin.");
        }

        public void ClosePopup()
        {
            if (!popup) return;
            popup.SetActive(false);
            Destroy(popup);
            popup = null;
        }

        void ShowPopup(string title, string icon, string subtitle, string body, bool sound = false)
        {
            ClosePopup();
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
            var close = MakeButton(card.transform, "Close", sound ? "KAPAT" : "TAMAM", Vector2.one * .5f, new Vector2(sound ? 155 : 0, -191), new Vector2(sound ? 240 : 290, 76), "Button_Tapered_Yellow", 29, true);
            close.onClick.AddListener(ClosePopup);
            if (sound)
            {
                var mute = MakeButton(card.transform, "Sound", AudioListener.volume > 0 ? "SES: AÇIK" : "SES: KAPALI", Vector2.one * .5f, new Vector2(-145, -191), new Vector2(260, 76), "Button01_s_Blue", 25);
                mute.onClick.AddListener(() => { AudioListener.volume = AudioListener.volume > 0 ? 0 : 1; mute.GetComponentInChildren<TMP_Text>().text = AudioListener.volume > 0 ? "SES: AÇIK" : "SES: KAPALI"; });
            }
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
            MakeButton(content, "Play", "OYNA", Vector2.one * .5f, new Vector2(548, -213), new Vector2(421, 124), "Button_Tapered_Yellow", 53, true);
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
