using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoollyArena
{
    public sealed partial class SurvivalShopUI : MonoBehaviour
    {
        SurvivalRun run;
        TMP_FontAsset font;
        TMP_Text header, title, subtitle, stats, equipmentTitle, details, itemTitle;
        GameObject panel, inventory, pausePanel, resultPanel;
        TMP_Text resultTitle,resultSubtitle,resultWave,resultKills,resultLevel,resultMissions;
        CanvasGroup clearBanner; TMP_Text clearTitle,clearSummary;
        GameObject[] combatHud;
        TMP_Text pauseMessage;
        UnityEngine.UI.Button soundButton;
        UnityEngine.UI.Button[] cards = new UnityEngine.UI.Button[4], locks = new UnityEngine.UI.Button[4], weapons = new UnityEngine.UI.Button[6], items = new UnityEngine.UI.Button[4];
        TMP_Text[] cardNames = new TMP_Text[4], cardDescriptions = new TMP_Text[4], cardPrices = new TMP_Text[4], tiers = new TMP_Text[4];
        UnityEngine.UI.Image[] accents = new UnityEngine.UI.Image[4], offerActions = new UnityEngine.UI.Image[4];
        UnityEngine.UI.Button next, reroll, combine, sell, previousItems, nextItems;
        int selected = -1, itemPage;
        bool selectedItem;
        OwnedGear selectedGear;
        float nextHeaderAt;
        public TMP_FontAsset FeedbackFont => font;
        int shownGold=-1,shownHealth=-1,shownLevel=-1,shownSecond=-1;
        float goldPulse=-10,healthPulse=-10,levelPulse=-10,timerPulse=-10;
        Color healthPulseColor;
        void LateUpdate(){
            if(!run || run.IsPaused || !walletLabel)return;
            float now=Time.time;
            AnimateReadout(walletLabel,now-goldPulse,gold,.16f);
            AnimateReadout(healthLabel,now-healthPulse,healthPulseColor,.12f);
            AnimateReadout(levelLabel,now-levelPulse,mint,.2f);
            float beat=Mathf.Clamp01((now-timerPulse)/.28f);
            header.transform.localScale=Vector3.one*(1+Mathf.Sin(beat*Mathf.PI)*.12f);
        }
        void AnimateReadout(TMP_Text text,float age,Color flash,float strength){
            float u=Mathf.Clamp01(age/.32f);
            text.transform.localScale=Vector3.one*(1+Mathf.Sin(u*Mathf.PI)*strength);
            text.color=Color.Lerp(flash,ink,u);
        }
        UnityEngine.UI.Image waveFill, healthFill, experienceFill;
        TMP_Text waveLabel, healthLabel, walletLabel, levelLabel;
        ArenaIcon[] offerIcons=new ArenaIcon[4], weaponIcons=new ArenaIcon[6], itemIcons=new ArenaIcon[4];
        TMP_Text[] statValues=new TMP_Text[8];
        TMP_FontAsset runtimeFont;
        Sprite rounded; Texture2D roundedTexture;
        readonly Color backdrop = new Color32(27, 29, 29, 255), surface = new Color32(43, 47, 44, 255);
        readonly Color ink = new Color32(246, 237, 213, 255), gold = new Color32(255, 199, 98, 255), muted = new Color32(173, 190, 198, 255);
        readonly Color mint = new Color32(170, 225, 108, 255), sky = new Color32(180, 221, 244, 255), amber = new Color32(255, 202, 100, 255);
        public void Initialize(SurvivalRun owner)
        {
            run = owner;
            var hud = Object.FindAnyObjectByType<ArenaHUD>(); font = hud.ammo.font;
            var survivorFont=Resources.Load<Font>("SurvivorSen");
            if(survivorFont){runtimeFont=TMP_FontAsset.CreateFontAsset(survivorFont);font=runtimeFont;}
            // Preserve Turkish glyphs even when the scene font is a static Latin atlas.
            if(!runtimeFont && !font.HasCharacters("ÇçĞğİıÖöŞşÜü") && font.sourceFontFile){runtimeFont=TMP_FontAsset.CreateFontAsset(font.sourceFontFile);runtimeFont.TryAddCharacters("ÇçĞğİıÖöŞşÜü");font=runtimeFont;}
            var canvas = hud.GetComponent<Canvas>(); canvas.sortingOrder = 100;
            var safe = canvas.transform.Find("SafeArea"); if (!safe) safe = canvas.transform;
            // Remove the overlapping training readouts before constructing the survival HUD.
            hud.ammo.gameObject.SetActive(false); hud.status.gameObject.SetActive(false); hud.stats.gameObject.SetActive(false);
            foreach(var label in hud.GetComponentsInChildren<TMP_Text>(true))
                if(!string.IsNullOrEmpty(label.text) && (label.text.Contains("WOOLLY /") || label.text.Contains("MOVEMENT +") || label.text.StartsWith("WASD") || label.text.StartsWith("LEFT"))) label.gameObject.SetActive(false);
            foreach(var button in hud.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                if(button.GetComponent<ArenaLobbyReturn>() || button.name.ToLowerInvariant().Contains("lobby")) button.gameObject.SetActive(false);
            CreateCombatHud(safe);
            var celebration=Box(safe,"Wave Clear",.16f,.42f,.84f,.66f,Color.clear);
            clearBanner=celebration.gameObject.AddComponent<CanvasGroup>();clearBanner.blocksRaycasts=false;
            clearTitle=Text(celebration.transform,"Wave Cleared",0,.36f,1,1,"DALGA TEMİZLENDİ",56);clearTitle.color=gold;clearTitle.fontStyle=FontStyles.Bold;
            clearSummary=Text(celebration.transform,"Clear Rewards",.06f,0,.94f,.33f,"",25);
            celebration.gameObject.SetActive(false);
            panel = Box(safe, "Wave Upgrade", .015f, .025f, .985f, .975f, backdrop).gameObject;
            panel.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            title = Text(panel.transform, "Shop Title", .025f, .90f, .96f, .985f, "", 32, TextAlignmentOptions.MidlineLeft);
            subtitle = Text(panel.transform, "Shop Subtitle", .025f, .835f, .96f, .895f, "", 18, TextAlignmentOptions.MidlineLeft);
            var side = Box(panel.transform, "Character Panel", .018f, .14f, .225f, .82f, surface).transform;
            var sideImage=side.GetComponent<UnityEngine.UI.Image>();sideImage.sprite=Rounded();sideImage.color=surface;
            Text(side, "Character Title", .03f, .9f, .97f, 1, "ÖZELLİKLER", 22, TextAlignmentOptions.MidlineLeft).color = gold;
            stats = Text(side, "Stats", .03f, .41f, .97f, .90f, "", 18, TextAlignmentOptions.TopLeft);stats.gameObject.SetActive(false);
            for(int i=0;i<8;i++){
                float x=.04f+(i%2)*.48f,y=.78f-(i/2)*.115f;
                Icon(side,((RunStat)i).ToString(),x,y,x+.18f,y+.095f);
                statValues[i]=Text(side,"Stat "+i,x+.18f,y,x+.45f,y+.095f,"",21);
            }
            itemTitle = Text(side, "Items Title", .03f, .34f, .97f, .41f, "", 17, TextAlignmentOptions.MidlineLeft);
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                items[i] = Button(side, "Owned Item " + i, .03f, .02f + (3-i)*.08f, .97f, .09f + (3-i)*.08f, "", 15);
                itemIcons[i]=Icon(items[i].transform,"empty",.04f,.08f,.24f,.92f);
                Label(items[i]).rectTransform.anchorMin=new Vector2(.25f,0);
                items[i].onClick.AddListener(() => Select(itemPage * 4 + slot, true));
            }
            previousItems = Button(panel.transform, "Previous Items", .018f, .06f, .115f, .13f, "<", 15);
            nextItems = Button(panel.transform, "Next Items", .122f, .06f, .225f, .13f, ">", 15);
            previousItems.onClick.AddListener(() => { itemPage = Mathf.Max(0, itemPage - 1); Refresh(); });
            nextItems.onClick.AddListener(() => { itemPage++; Refresh(); });
            for (int i = 0; i < 4; i++)
            {
                int slot = i; float x = .244f + i * .185f;
                cards[i] = Button(panel.transform, "Upgrade " + i, x, .43f, x + .174f, .82f, "", 20, false);
                var t = cards[i].transform;
                accents[i] = Box(t, "Tier Accent", 0, .97f, 1, 1, gold);
                // Separate vertical bands: title, art, readable effects, then the purchase action.
                cardNames[i] = Text(t, "Name", .04f, .83f, .96f, .96f, "", 23);
                cardNames[i].fontStyle=FontStyles.Bold;
                offerIcons[i]=Icon(t,"empty",.09f,.53f,.86f,.82f);
                tiers[i] = Text(t, "Tier", .79f, .67f, .97f, .81f, "", 18);
                cardDescriptions[i] = Text(t, "Description", .05f, .235f, .95f, .52f, "", 19);
                offerActions[i]=Box(t,"Purchase Area",.05f,.035f,.95f,.205f,gold);
                cardPrices[i] = Text(offerActions[i].transform, "Price", 0, 0, 1, 1, "", 21);
                cardPrices[i].outlineWidth=0;
                cards[i].onClick.AddListener(() => run.ChooseUpgrade(slot));
                locks[i] = Button(panel.transform, "Lock " + i, x, .37f, x + .174f, .42f, "KİLİTLE", 16);
                locks[i].onClick.AddListener(() => run.ToggleLock(slot));
            }
            inventory = new GameObject("Equipment", typeof(RectTransform));
            var ir = (RectTransform)inventory.transform; ir.SetParent(panel.transform, false); ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one; ir.offsetMin = ir.offsetMax = Vector2.zero;
            equipmentTitle = Text(ir, "Equipment Title", .244f, .32f, .98f, .36f, "", 18, TextAlignmentOptions.MidlineLeft);
            for (int i = 0; i < 6; i++)
            {
                int slot = i; float x = .244f + i * .123f;
                weapons[i] = Button(ir, "Weapon Slot " + i, x, .22f, x + .113f, .315f, "", 17);
                weaponIcons[i]=Icon(weapons[i].transform,"empty",.12f,.35f,.88f,.94f);
                Label(weapons[i]).rectTransform.anchorMax=new Vector2(1,.3f);
                weapons[i].onClick.AddListener(() => Select(slot, false));
            }
            details = Text(ir, "Selected Equipment", .244f, .14f, .69f, .21f, "", 16, TextAlignmentOptions.MidlineLeft);
            combine = Button(ir, "Combine", .705f, .145f, .835f, .21f, "BİRLEŞTİR", 17);
            sell = Button(ir, "Recycle", .845f, .145f, .978f, .21f, "SAT", 17);
            combine.onClick.AddListener(() => { if (run.Combine(selected)) { selected = -1; selectedGear = null; Refresh(); } });
            sell.onClick.AddListener(() => { if (selectedItem ? run.SellItem(selected) : run.SellWeapon(selected)) { selected = -1; selectedGear = null; Refresh(); } });
            reroll = Button(panel.transform, "Reroll", .244f, .035f, .47f, .125f, "", 22);
            reroll.GetComponent<UnityEngine.UI.Image>().sprite=Rounded();reroll.GetComponent<UnityEngine.UI.Image>().color=sky;
            reroll.onClick.AddListener(() => run.Reroll());
            next = Button(panel.transform, "Next Wave", .62f, .035f, .978f, .125f, "DEVAM >", 23);
            next.GetComponent<UnityEngine.UI.Image>().sprite=Rounded();next.GetComponent<UnityEngine.UI.Image>().color=gold;
            next.onClick.AddListener(() => { if (run.Phase == SurvivalRun.RunPhase.Defeat || run.Phase == SurvivalRun.RunPhase.Victory) run.Replay(); else run.BeginNextWave(); });
            foreach(var text in panel.GetComponentsInChildren<TMP_Text>(true)) {
                text.overflowMode=TextOverflowModes.Ellipsis;
                text.textWrappingMode=TextWrappingModes.NoWrap;
                text.outlineWidth=0;
            }
            foreach(var description in cardDescriptions)description.textWrappingMode=TextWrappingModes.Normal;
            details.textWrappingMode=TextWrappingModes.Normal;
            pausePanel = Box(safe, "Pause Overlay", 0, 0, 1, 1, new Color(.19f, .38f, .43f, .35f)).gameObject;
            pausePanel.GetComponent<UnityEngine.UI.Image>().raycastTarget = true;
            Box(pausePanel.transform, "Pause Card Shadow", .21f, .125f, .79f, .88f, new Color(.18f, .28f, .31f, .22f));
            Box(pausePanel.transform, "Pause Card", .20f, .15f, .80f, .905f, surface);
            Box(pausePanel.transform, "Pause Accent", .20f, .88f, .80f, .905f, amber);
            Text(pausePanel.transform, "Paused Title", .18f, .72f, .82f, .87f, "DURAKLATILDI", 38).color = gold;
            pauseMessage = Text(pausePanel.transform, "Checkpoint Info", .24f, .49f, .76f, .72f, "", 23);
            var resume = Button(pausePanel.transform, "Resume Run", .30f, .35f, .70f, .46f, "DEVAM ET", 25);
            resume.GetComponent<UnityEngine.UI.Image>().color = mint;
            resume.onClick.AddListener(() => run.SetPaused(false));
            soundButton = Button(pausePanel.transform, "Pause Sound", .30f, .22f, .49f, .32f, "", 20);
            soundButton.GetComponent<UnityEngine.UI.Image>().color = sky;
            soundButton.onClick.AddListener(() => { RunSession.ToggleSound(); ShowPause(true); });
            var lobby = Button(pausePanel.transform, "Save And Lobby", .51f, .22f, .70f, .32f, "LOBİ", 20);
            lobby.GetComponent<UnityEngine.UI.Image>().color = amber;
            lobby.onClick.AddListener(run.LeaveToLobby);
            foreach(var action in new[]{next,reroll,resume,soundButton,lobby}) Label(action).color=new Color32(27,35,43,255);
            pausePanel.SetActive(false);
            CreateResultScreen(safe);
            CreateLevelScreen(safe);
            if (run.Player.mobile)
            {
                run.Player.mobile.aim.gameObject.SetActive(false);
                var reload = run.Player.mobile.transform.Find("Reload"); if (reload) reload.gameObject.SetActive(false);
            }
            foreach (var text in hud.GetComponentsInChildren<TMP_Text>())
            {
                if (string.IsNullOrEmpty(text.text)) continue;
                if (text.text.Contains("WOOLLY / TRAINING")) text.text = "WOOLLY / SURVIVAL";
                else if (text.text.Contains("MOVEMENT + COMBAT LAB")) text.text = "MOVE · AUTO ATTACK · EQUIP";
                else if (text.text.StartsWith("LEFT") || text.text.StartsWith("WASD")) text.text = "MOVE TO SURVIVE · AUTOMATIC WEAPONS · DASH";
            }
        }
        public void ShowPause(bool visible)
        {
            pausePanel.SetActive(visible);
            if (!visible) return;
            pausePanel.transform.SetAsLastSibling();
            pauseMessage.text = run.SaveError ?? ((run.Phase == SurvivalRun.RunPhase.Wave || run.Phase == SurvivalRun.RunPhase.WaveClear) ? "Lobiye dönersen bu dalga baştan başlar." : "İlerlemen kaydedildi.");
            Label(soundButton).text = RunSession.SoundEnabled ? "SES AÇIK" : "SES KAPALI";
        }
        void Select(int index, bool item)
        {
            var collection=item?run.Build.Items:run.Build.Weapons;
            selectedItem=item;selectedGear=index>=0&&index<collection.Count?collection[index]:null;Refresh();
        }
        void MarkSelected(UnityEngine.UI.Button button,bool active)
        {
            var outline=button.GetComponent<UnityEngine.UI.Outline>();
            outline.effectColor=active?gold:new Color(0,0,0,.45f);
            outline.effectDistance=active?new Vector2(3,-3):new Vector2(0,-3);
        }
        public void UpdateHeader()
        {
            if (Time.unscaledTime < nextHeaderAt) return;
            nextHeaderAt = Time.unscaledTime + .1f;
            var v = run.Player.GetComponent<CharacterVitals>();
            int seconds=Mathf.CeilToInt(run.Remaining);
            if(shownGold>=0&&run.Materials>shownGold)goldPulse=Time.time;
            if(shownHealth>=0&&v.Health!=shownHealth){healthPulse=Time.time;healthPulseColor=v.Health<shownHealth?new Color(1,.35f,.3f):mint;}
            if(shownLevel>=0&&run.Build.Level>shownLevel)levelPulse=Time.time;
            if(seconds!=shownSecond&&seconds>0&&seconds<=5&&run.Phase==SurvivalRun.RunPhase.Wave)timerPulse=Time.time;
            shownGold=run.Materials;shownHealth=v.Health;shownLevel=run.Build.Level;shownSecond=seconds;
            waveFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(run.Remaining/run.CurrentWaveDuration),1);
            header.text = seconds==0&&run.BossAlive?"BOSS!":$"{seconds/60:00}:{seconds%60:00}";
            header.color = seconds<=5 ? new Color32(255,133,107,255) : ink;
            waveLabel.text = run.BossAlive?$"BOSS · {Mathf.CeilToInt(100f*run.Boss.vitals.Health/run.Boss.vitals.maxHealth)}%":$"DALGA {run.Wave:00}";
            healthLabel.text = $"{v.Health}<size=65%> / {v.maxHealth}</size>";
            healthFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)v.Health/v.maxHealth),1);
            walletLabel.text = run.Materials.ToString();
            levelLabel.text = $"SEV. {run.Build.Level}";
            UpdateHudDetails();
            experienceFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)run.Build.Experience/run.Build.NextLevelXP),1);

        }
        public void UpdateWaveClear(float elapsed){
            float enter=Mathf.Clamp01(elapsed/.2f),exit=Mathf.Clamp01((WaveClearTimeline.Duration-elapsed)/.4f);
            clearBanner.alpha=enter*exit;
            float pop=1+Mathf.Sin(Mathf.Clamp01(elapsed/.35f)*Mathf.PI)*.12f;
            clearBanner.transform.localScale=Vector3.one*pop;
            clearTitle.text=run.Wave>=run.totalWaves?"KANYON TEMİZLENDİ!":"DALGA TEMİZLENDİ!";
            clearSummary.text=$"{run.WaveKills} av  ·  +{run.WaveGold} altın";
        }
        public void Refresh()
        {
            bool finished=run.Phase==SurvivalRun.RunPhase.Defeat||run.Phase==SurvivalRun.RunPhase.Victory;
            resultPanel.SetActive(finished);
            levelPanel.SetActive(run.Phase==SurvivalRun.RunPhase.LevelUp);
            if(finished){ShowResult();return;}
            bool clearing=run.Phase==SurvivalRun.RunPhase.WaveClear;
            clearBanner.gameObject.SetActive(clearing);
            if(clearing){panel.SetActive(false);foreach(var piece in combatHud)piece.SetActive(false);clearBanner.transform.SetAsLastSibling();return;}
            var build = run.Build; bool level = run.Phase == SurvivalRun.RunPhase.LevelUp, shop = run.Phase == SurvivalRun.RunPhase.Upgrade;
            if(level){panel.SetActive(false);foreach(var piece in combatHud)piece.SetActive(false);RefreshLevelScreen();return;}
            if(!shop)selectedGear=null;
            var selectedCollection=selectedItem?build.Items:build.Weapons;
            selected=selectedGear==null?-1:selectedCollection.IndexOf(selectedGear);
            if(selected<0)selectedGear=null;
            bool ended = run.Phase == SurvivalRun.RunPhase.Defeat || run.Phase == SurvivalRun.RunPhase.Victory;
            foreach(var piece in combatHud)piece.SetActive(run.Phase == SurvivalRun.RunPhase.Wave);
            panel.SetActive(run.Phase != SurvivalRun.RunPhase.Wave); panel.transform.SetAsLastSibling(); nextHeaderAt = 0; UpdateHeader();
            title.text = level ? "SEVİYE ATLADIN" : shop ? $"DALGA {run.Wave} TAMAMLANDI" : run.Phase == SurvivalRun.RunPhase.Victory ? "BAŞARDIN!" : "BİR TUR DAHA?";
            title.color = gold;
            subtitle.text = level ? $"SEVİYE {build.RewardLevel} · {CharacterDefinition.GrowthDescription(build.CharacterId)} · {build.PendingLevels} seçim" : shop ? $"{run.Materials} ALTIN  ·  +{run.LastHarvest} hasat  ·  Güçlenmek için bir kart seç" : $"{run.Wave}. dalga  ·  {run.Kills} yenilen düşman";
            if (run.SaveError != null) subtitle.text = run.SaveError;
            subtitle.color = run.SaveError != null ? new Color(.7f, .18f, .12f) : muted;
            for(int i=0;i<8;i++)statValues[i].text=CharacterDefinition.DisplayValue((RunStat)i,build.Stat((RunStat)i));
            for (int i = 0; i < 4; i++)
            {
                cards[i].gameObject.SetActive(!ended); locks[i].gameObject.SetActive(shop);

                if (level)
                {
                    var stat = build.LevelChoices[i]; tiers[i].text = RunCatalog.TierName(build.LevelChoiceTiers[i]);offerIcons[i].Set(stat.ToString()); cardNames[i].text = RunItem.StatName(stat);
                    cardDescriptions[i].text = CharacterDefinition.DisplayValue(stat,build.Stat(stat))+" → "+CharacterDefinition.DisplayValue(stat,build.Stat(stat)+SurvivalBuild.LevelBonus(stat,build.LevelChoiceTiers[i])); cardPrices[i].text = "SEÇ";
                    cards[i].interactable = true; accents[i].color = RunCatalog.TierColor(build.LevelChoiceTiers[i]);
                }
                else if (shop)
                {
                    var offer = build.Offers[i]; tiers[i].text = RunCatalog.TierName(offer.Tier);
                    offerIcons[i].Set(offer.Item.Id);
                    cardNames[i].text = offer.Item.Name;
                    cardDescriptions[i].text = ShortDescription(offer.Item,offer.Tier);
                    string action = offer.Item.IsWeapon && build.Weapons.Count >= 6 && build.CanBuy(i) ? "BİRLEŞTİR" : "SATIN AL";
                    cardPrices[i].text = offer.Sold ? "ALINDI" : $"{action} · {offer.Price}";
                    if (!offer.Sold && !build.CanBuy(i))
                        cardPrices[i].text = build.Materials < offer.Price ? $"{offer.Price} ALTIN GEREKLİ" : "YUVALAR DOLU";
                    cards[i].interactable = build.CanBuy(i); accents[i].color = RunCatalog.TierColor(offer.Tier);
                    Label(locks[i]).text = offer.Locked ? "KİLİDİ AÇ" : "KİLİTLE";
                    locks[i].GetComponent<UnityEngine.UI.Image>().color=offer.Locked?new Color32(61,115,120,255):surface;
                    locks[i].interactable = !offer.Sold;
                }
                cards[i].GetComponent<UnityEngine.UI.Image>().sprite=Rounded();cards[i].GetComponent<UnityEngine.UI.Image>().color = Color.Lerp(surface, accents[i].color, .12f);
                tiers[i].color = accents[i].color;
                offerActions[i].color=cards[i].interactable?gold:surface;
                cardPrices[i].color=cards[i].interactable?new Color32(27,35,43,255):muted;
            }
            // Level-up is its own compact four-card choice layout; empty inventory rows belong to the shop.
            itemTitle.transform.parent.gameObject.SetActive(shop);
            previousItems.gameObject.SetActive(shop);nextItems.gameObject.SetActive(shop);
            for(int i=0;i<4;i++){
                var rect=(RectTransform)cards[i].transform;float x=level?.06f+i*.224f:.244f+i*.185f;
                rect.anchorMin=new Vector2(x,level?.27f:.43f);rect.anchorMax=new Vector2(x+(level?.207f:.174f),level?.79f:.82f);
            }
            var rerollRect=(RectTransform)reroll.transform;rerollRect.anchorMin=new Vector2(level?.35f:.244f,.035f);rerollRect.anchorMax=new Vector2(level?.65f:.47f,.125f);
            inventory.SetActive(shop || ended);
            equipmentTitle.text = build.FamilySummary();
            for (int i = 0; i < 6; i++)
            {
                var gear = i < build.Weapons.Count ? build.Weapons[i] : null;
                weaponIcons[i].Set(gear==null?"empty":gear.Item.Id);
                Label(weapons[i]).text = gear == null ? "" : RunCatalog.TierName(gear.Tier);
                weapons[i].interactable = shop && gear != null;
                MarkSelected(weapons[i],!selectedItem&&selected==i);
                weapons[i].GetComponent<UnityEngine.UI.Image>().sprite=Rounded();weapons[i].GetComponent<UnityEngine.UI.Image>().color = gear == null ? surface : Color.Lerp(surface, RunCatalog.TierColor(gear.Tier), .22f);
            }
            itemPage = Mathf.Clamp(itemPage, 0, Mathf.Max(0, (build.Items.Count - 1) / 4));
            int itemPages=Mathf.Max(1,(build.Items.Count+3)/4);
            itemTitle.text = $"EŞYALAR {build.Items.Count}"+(itemPages>1?$" · {itemPage+1}/{itemPages}":"");
            previousItems.gameObject.SetActive(shop&&itemPages>1);nextItems.gameObject.SetActive(shop&&itemPages>1);
            for (int i = 0; i < 4; i++)
            {
                int index = itemPage * 4 + i; bool exists = index < build.Items.Count;
                itemIcons[i].Set(exists?build.Items[index].Item.Id:"empty");
                Label(items[i]).text = exists ? build.Items[index].Item.Name + "  " + build.Items[index].Tier : "—";
                items[i].interactable = shop && exists;
                MarkSelected(items[i],selectedItem&&selected==index);
            }
            previousItems.interactable = shop && itemPage > 0;
            nextItems.interactable = shop && (itemPage + 1) * 4 < build.Items.Count;
            var collection = selectedItem ? build.Items : build.Weapons;
            var selection = selected >= 0 && selected < collection.Count ? collection[selected] : null;
            details.text = selection == null ? "2 / 4 aynı aileden silah: build bonusu. Ayrıntılar için silaha dokun." : selection.Item.Name + "  " + RunCatalog.TierName(selection.Tier) + "\n" + (selection.Item.IsWeapon ? build.FamilyDescription(WeaponArsenal.Profile(selection.Item.Weapon.Value).Family) : selection.Item.Description(selection.Tier).Replace("\n", " · "));
            combine.interactable = shop && !selectedItem && build.CanCombine(selected);
            sell.interactable = shop && selection != null && (selectedItem || build.Weapons.Count > 1);
            combine.gameObject.SetActive(shop&&selection!=null&&!selectedItem);
            sell.gameObject.SetActive(shop&&selection!=null);
            Label(combine).text=combine.interactable?"BİRLEŞTİR":selection!=null&&selection.Tier>=4?"SON SEVİYE":"EŞİ YOK";
            Label(sell).text = selection == null ? "SAT" : !selectedItem&&build.Weapons.Count==1?"SON SİLAH":"SAT +" + selection.SellValue;
            reroll.gameObject.SetActive(shop||level); reroll.interactable = build.Materials >= build.RerollCost && (level||System.Array.Exists(build.Offers, x => x!=null&&(!x.Locked || x.Sold)));
            Label(reroll).text = !level&&System.Array.TrueForAll(build.Offers, x => x != null && x.Locked && !x.Sold) ? "HEPSİ KİLİTLİ" : "YENİLE · " + build.RerollCost + " ALTIN";
            next.gameObject.SetActive(!level);
            int nextWave=run.Wave+1;
            Label(next).text = ended ? "TEKRAR OYNA" : WaveDifficulty.BossWave(nextWave)?$"DALGA {nextWave} · BOSS  >":$"DALGA {nextWave} BAŞLAT  >";
        }
        void CreateResultScreen(Transform parent)
        {
            var dark=new Color32(28,38,62,255);var cream=new Color32(255,236,190,255);
            resultPanel=Box(parent,"Run Results",0,0,1,1,new Color(.08f,.12f,.22f,.9f)).gameObject;
            resultPanel.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            Box(resultPanel.transform,"Result Shadow",.18f,.085f,.83f,.88f,dark);
            var card=Box(resultPanel.transform,"Result Card",.17f,.11f,.82f,.905f,cream);
            var border=card.gameObject.AddComponent<UnityEngine.UI.Outline>();border.effectColor=dark;border.effectDistance=new Vector2(5,-5);
            Box(card.transform,"Result Ribbon",.025f,.76f,.975f,.975f,new Color32(39,137,219,255));
            resultTitle=Text(card.transform,"Result Title",.04f,.80f,.96f,.97f,"",44);resultTitle.fontStyle=FontStyles.Bold;
            resultSubtitle=Text(card.transform,"Result Subtitle",.06f,.655f,.94f,.765f,"",22);resultSubtitle.color=dark;resultSubtitle.outlineWidth=0;
            TMP_Text Metric(float x,string label){
                var tile=Box(card.transform,label,x,.345f,x+.27f,.625f,new Color32(255,249,222,255));
                var stroke=tile.gameObject.AddComponent<UnityEngine.UI.Outline>();stroke.effectColor=dark;stroke.effectDistance=new Vector2(3,-3);
                var caption=Text(tile.transform,"Caption",.03f,.66f,.97f,.96f,label,19);caption.color=dark;caption.outlineWidth=0;
                var number=Text(tile.transform,"Value",.02f,.06f,.98f,.69f,"",45);number.color=dark;number.outlineWidth=0;number.fontStyle=FontStyles.Bold;return number;
            }
            resultWave=Metric(.055f,"DALGA");resultKills=Metric(.365f,"YENİLEN DÜŞMAN");resultLevel=Metric(.675f,"SEVİYE");
            resultMissions=Text(card.transform,"Mission Summary",.05f,.225f,.95f,.335f,"",21);resultMissions.color=dark;resultMissions.outlineWidth=0;
            var home=Button(card.transform,"Result Lobby",.055f,.06f,.485f,.195f,"LOBİYE DÖN",26);home.GetComponent<UnityEngine.UI.Image>().color=new Color32(49,135,216,255);home.onClick.AddListener(run.LeaveToLobby);
            var replay=Button(card.transform,"Result Replay",.515f,.06f,.945f,.195f,"TEKRAR OYNA",26);replay.GetComponent<UnityEngine.UI.Image>().color=new Color32(255,192,60,255);Label(replay).color=dark;Label(replay).outlineWidth=0;replay.onClick.AddListener(run.Replay);
            resultPanel.SetActive(false);
        }
        void ShowResult()
        {
            panel.SetActive(false);pausePanel.SetActive(false);clearBanner.gameObject.SetActive(false);
            foreach(var piece in combatHud)piece.SetActive(false);
            resultPanel.transform.SetAsLastSibling();bool won=run.Phase==SurvivalRun.RunPhase.Victory;
            resultTitle.text=won?"KANYONUN KAHRAMANI!":"TOZU SİLK, YENİDEN DENE!";
            resultTitle.color=won?new Color32(255,222,112,255):Color.white;
            resultSubtitle.text=won?"20 dalga geride kaldı. Kanyon artık güvende!":run.Wave<=3?"Hareket et, saldırıları atlat ve silahlarını güçlendir.":"Her koşu yeni bir şans. Bir sonraki dalga seni bekliyor.";
            resultWave.text=(won?run.Wave:Mathf.Max(0,run.Wave-1))+" / "+run.totalWaves;
            resultKills.text=run.Kills.ToString();resultLevel.text=run.Build.Level.ToString();
            int ready=CareerStore.Load().ReadyCount;
            resultMissions.text=run.SaveError??(ready>0?ready+" görev ödülü hazır · Lobide Görevler'den al!":"Rekorların kaydedildi · Lobide görevlerini kontrol et.");
        }
        string ShortDescription(RunItem item,int tier) {
            if(item.IsWeapon)return item.Description(tier);
            // One effect per line makes both benefits and penalties understandable before buying.
            return item.Description(tier);
        }
        ArenaIcon Icon(Transform parent,string key,float x0,float y0,float x1,float y1) {
            var go=new GameObject("Icon "+key,typeof(RectTransform));var icon=go.AddComponent<ArenaIcon>();
            var r=icon.rectTransform;r.SetParent(parent,false);r.anchorMin=new Vector2(x0,y0);r.anchorMax=new Vector2(x1,y1);r.offsetMin=r.offsetMax=Vector2.zero;icon.raycastTarget=false;icon.Set(key);return icon;
        }
        Sprite Rounded()
        {
            if(rounded) return rounded;
            const int size=32; const float radius=8;
            roundedTexture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="HUD rounded corners",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float dx=Mathf.Max(Mathf.Abs(x-15.5f)-(16-radius),0),dy=Mathf.Max(Mathf.Abs(y-15.5f)-(16-radius),0);
                pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy)));
            }
            roundedTexture.SetPixels(pixels);roundedTexture.Apply();
            rounded=Sprite.Create(roundedTexture,new Rect(0,0,size,size),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(9,9,9,9));return rounded;
        }
        void OnDestroy(){if(rounded)Destroy(rounded);if(roundedTexture)Destroy(roundedTexture);if(runtimeFont){foreach(var texture in runtimeFont.atlasTextures)if(texture)Destroy(texture);Destroy(runtimeFont.material);Destroy(runtimeFont);}}
        TMP_Text Label(UnityEngine.UI.Button button) => button.GetComponentInChildren<TMP_Text>(true);
        UnityEngine.UI.Image Box(Transform parent, string name, float x0, float y0, float x1, float y1, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            var r = (RectTransform)go.transform; r.SetParent(parent, false); r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1); r.offsetMin = r.offsetMax = Vector2.zero;
            var image = go.GetComponent<UnityEngine.UI.Image>(); image.color = color; image.sprite=Rounded(); image.type=UnityEngine.UI.Image.Type.Sliced; image.raycastTarget = false; return image;
        }
        TMP_Text Text(Transform parent, string name, float x0, float y0, float x1, float y1, string value, int size, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var go = new GameObject(name, typeof(RectTransform)); var text = go.AddComponent<TextMeshProUGUI>(); var r = text.rectTransform;
            r.SetParent(parent, false); r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1); r.offsetMin = new Vector2(6, 3); r.offsetMax = new Vector2(-6, -3);
            text.font = font; text.text = value; text.fontSize = size; text.enableAutoSizing = true; text.fontSizeMin = size * .72f; text.fontSizeMax = size;
            text.alignment = alignment; text.color = ink; text.outlineColor=new Color32(44,25,24,230);text.outlineWidth=0; text.raycastTarget = false; return text;
        }
        UnityEngine.UI.Button Button(Transform parent, string name, float x0, float y0, float x1, float y1, string value, int size, bool createLabel = true)
        {
            var image = Box(parent, name, x0, y0, x1, y1, Color.white); image.raycastTarget = true;image.sprite=Rounded();image.color=surface;
            var button = image.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1, .96f, .87f); colors.pressedColor = new Color(.84f, .9f, .92f); colors.disabledColor = new Color(.82f, .82f, .79f); button.colors = colors;
            var outline = image.gameObject.AddComponent<UnityEngine.UI.Outline>(); outline.effectColor=new Color(0,0,0,.45f);outline.effectDistance=new Vector2(0,-3);
            if (createLabel) Text(image.transform, "Label", 0, 0, 1, 1, value, size); return button;
        }
    }
}
