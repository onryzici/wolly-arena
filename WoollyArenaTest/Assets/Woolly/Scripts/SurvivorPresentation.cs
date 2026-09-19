using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WoollyArena
{
    public sealed partial class SurvivalShopUI
    {
        GameObject levelPanel;
        TMP_Text levelHeading, levelProgress, levelGrowth, levelWallet, levelHint, experienceLabel;
        readonly Button[] levelCards = new Button[4];
        readonly TMP_Text[] levelNames = new TMP_Text[4], levelBonuses = new TMP_Text[4], levelPreviews = new TMP_Text[4], levelEffects = new TMP_Text[4], levelRarities = new TMP_Text[4];
        readonly ArenaIcon[] levelIcons = new ArenaIcon[4];
        readonly Image[] levelBorders = new Image[4], levelActions = new Image[4];
        Button levelReroll;
        Image bossFill;
        GameObject bossHud;
        TMP_Text bossName;

        void CreateCombatHud(Transform safe)
        {
            var root = Box(safe, "Survivor HUD", 0, .858f, 1, 1, Color.clear).transform;
            var hp = Box(root, "Health Card", .023f, .40f, .267f, .90f, Color.clear).transform;
            Icon(hp, "MaxHealth", .018f, .35f, .15f, .95f);
            var track = Box(hp, "Health Track", .04f, .10f, .96f, .18f, Color.clear);
            healthFill = Box(track.transform, "Health Fill", 0, 0, 1, 1, new Color32(215, 68, 72, 255));
            healthLabel = Text(hp, "Health Value", .18f, .34f, .96f, .93f, "", 26, TextAlignmentOptions.MidlineLeft);
            healthLabel.fontStyle = FontStyles.Bold;
            var timer = Box(root, "Wave Clock", .421f, .30f, .579f, .98f, Color.clear).transform;
            waveLabel = Text(timer, "Wave Label", .02f, .65f, .98f, .97f, "", 15);
            waveLabel.color = muted;
            header = Text(timer, "Time", .02f, .12f, .98f, .72f, "", 37);
            var timerTrack = Box(timer, "Timer Track", .1f, .075f, .9f, .10f, Color.clear);
            waveFill = Box(timerTrack.transform, "Time Remaining", 0, 0, 1, 1, gold);
            var wallet = Box(root, "Wallet Card", .767f, .40f, .916f, .90f, Color.clear).transform;
            Icon(wallet, "Harvesting", .015f, .15f, .25f, .88f);
            walletLabel = Text(wallet, "Gold Value", .23f, .18f, .98f, .9f, "", 28);
            walletLabel.color = mint;
            levelLabel = Text(root, "Level", .022f, .04f, .107f, .31f, "", 15, TextAlignmentOptions.MidlineLeft);
            var xp = Box(root, "Experience Track", .109f, .125f, .86f, .175f, Color.clear);
            experienceFill = Box(xp.transform, "Experience Fill", 0, 0, 1, 1, mint);
            experienceLabel = Text(root, "Experience Value", .866f, .03f, .973f, .32f, "", 13, TextAlignmentOptions.MidlineRight);
            var pause = Button(root, "Pause Run", .932f, .40f, .976f, .90f, "II", 24);
            pause.GetComponent<Image>().color = Color.clear;
            pause.GetComponent<CanvasRenderer>().cullTransparentMesh = false;
            pause.GetComponent<Outline>().enabled = false;
            pause.onClick.AddListener(() => run.SetPaused(true));
            bossHud = Box(safe, "Boss Health", .31f, .80f, .69f, .85f, Color.clear).gameObject;
            bossFill = Box(bossHud.transform, "Boss Fill", .012f, .12f, .988f, .24f, new Color32(177, 63, 57, 255));
            bossName = Text(bossHud.transform, "Boss Name", .025f, .3f, .975f, 1, "", 17);
            foreach (var text in new[] { healthLabel, header, waveLabel, walletLabel, levelLabel, experienceLabel, bossName, Label(pause) })
            {
                text.outlineColor = new Color32(35, 27, 21, 240);
                text.outlineWidth = .22f;
            }
            combatHud = new[] { root.gameObject, bossHud };
            var billboard = run.Player.GetComponentInChildren<CharacterBillboard>();
            if (billboard) billboard.gameObject.SetActive(false);
            if (run.Player.mobile && run.Player.mobile.move)
            {
                var stick = run.Player.mobile.move;
                foreach (var old in stick.GetComponentsInChildren<Graphic>()) old.enabled = false;
                var face = CreateStickFace(stick.transform, false); face.raycastTarget = true; face.transform.SetAsFirstSibling();
                CreateStickFace(stick.knob, true);
            }
        }

        static SurvivorStickFace CreateStickFace(Transform parent, bool knob)
        {
            var go = new GameObject(knob ? "Flat stick knob" : "Flat stick base", typeof(RectTransform));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var face = go.AddComponent<SurvivorStickFace>(); face.knob = knob; face.raycastTarget = false; return face;
        }

        void UpdateHudDetails()
        {
            experienceLabel.text = run.Build.Experience + " / " + run.Build.NextLevelXP;
            bossHud.SetActive(run.Phase == SurvivalRun.RunPhase.Wave && run.BossAlive);
            if (!run.BossAlive) return;
            bossFill.rectTransform.anchorMax = new Vector2(.012f + .976f * run.Boss.vitals.Health / run.Boss.vitals.maxHealth, .24f);
            bossName.text = run.Boss.vitals.displayName;
            waveLabel.text = "DALGA " + run.Wave + " / " + run.totalWaves;
        }

        void CreateLevelScreen(Transform safe)
        {
            levelPanel = Box(safe, "Level Choice Screen", 0, 0, 1, 1, new Color32(21, 24, 22, 247)).gameObject;
            levelPanel.GetComponent<Image>().raycastTarget = true;
            var root = Box(levelPanel.transform, "Level Content", .04f, .045f, .96f, .955f, Color.clear).transform;
            levelHeading = Text(root, "Level Heading", .04f, .84f, .78f, .97f, "SEVİYE ATLADIN!", 42, TextAlignmentOptions.MidlineLeft);
            levelHeading.color = mint;
            levelProgress = Text(root, "Level Progress", .04f, .77f, .94f, .835f, "", 20, TextAlignmentOptions.MidlineLeft);
            levelGrowth = Text(root, "Automatic Growth", .04f, .71f, .94f, .77f, "", 16, TextAlignmentOptions.MidlineLeft);
            levelGrowth.color = muted;
            levelWallet = Text(root, "Level Wallet", .78f, .86f, .96f, .96f, "", 22, TextAlignmentOptions.MidlineRight);
            levelWallet.color = mint;
            for (int i = 0; i < 4; i++)
            {
                int slot = i; float left = .04f + i * .233f;
                levelBorders[i] = Box(root, "Choice Frame " + i, left, .22f, left + .216f, .68f, muted);
                levelCards[i] = Button(levelBorders[i].transform, "Level Choice " + i, .012f, .01f, .988f, .99f, "", 20, false);
                var card = levelCards[i].transform;
                levelRarities[i] = Text(card, "Rarity", .07f, .87f, .93f, .98f, "", 14, TextAlignmentOptions.MidlineLeft);
                levelIcons[i] = Icon(card, "MaxHealth", .34f, .65f, .66f, .87f);
                levelNames[i] = Text(card, "Stat Name", .04f, .53f, .96f, .66f, "", 23);
                levelBonuses[i] = Text(card, "Upgrade Value", .05f, .345f, .95f, .54f, "", 45);
                levelPreviews[i] = Text(card, "Current To New", .02f, .265f, .98f, .35f, "", 17);
                levelPreviews[i].color = muted;
                levelEffects[i] = Text(card, "Upgrade Effect", .07f, .135f, .93f, .27f, "", 15);
                levelActions[i] = Box(card, "Choose Action", .045f, .027f, .955f, .13f, mint);
                Text(levelActions[i].transform, "Choose Label", 0, 0, 1, 1, "SEÇ", 18).color = backdrop;
                levelCards[i].onClick.AddListener(() => run.ChooseLevel(slot));
            }
            levelReroll = Button(root, "Reroll Level Choices", .345f, .08f, .655f, .175f, "", 22);
            levelReroll.onClick.AddListener(() => run.Reroll());
            levelHint = Text(root, "Choice Hint", .1f, .008f, .9f, .067f, "", 16);
            levelHint.color = muted;
            levelPanel.SetActive(false);
        }

        void RefreshLevelScreen()
        {
            levelPanel.transform.SetAsLastSibling();
            var build = run.Build;
            levelProgress.text = "SEVİYE " + build.RewardLevel + "  ·  BİR GÜÇLENDİRME SEÇ";
            levelGrowth.text = "Otomatik bonus: " + CharacterDefinition.GrowthDescription(build.CharacterId);
            levelWallet.text = build.Materials + " ALTIN";
            for (int i = 0; i < 4; i++)
            {
                var stat = build.LevelChoices[i]; int tier = build.LevelChoiceTiers[i];
                var color = tier == 1 ? new Color32(197, 207, 188, 255) : tier == 2 ? new Color32(104, 185, 243, 255) : tier == 3 ? new Color32(192, 134, 249, 255) : new Color32(255, 94, 105, 255);
                levelBorders[i].color = color; levelActions[i].color = color; levelRarities[i].color = color;
                levelCards[i].GetComponent<Image>().color = Color.Lerp(backdrop, color, .08f);
                levelRarities[i].text = (tier == 1 ? "YAYGIN" : tier == 2 ? "GELİŞMİŞ" : tier == 3 ? "NADİR" : "EFSANEVİ") + "  " + RunCatalog.TierName(tier);
                levelNames[i].text = RunItem.StatName(stat); levelIcons[i].Set(stat.ToString());
                int amount = SurvivalBuild.LevelBonus(stat, tier);
                bool percent = stat == RunStat.AttackSpeed || stat == RunStat.MoveSpeed || stat == RunStat.Damage || stat == RunStat.Critical;
                levelBonuses[i].text = "+" + amount + (percent ? "%" : ""); levelBonuses[i].color = mint;
                int before = build.Stat(stat), after = build.StatAfterBonus(stat, amount);
                levelPreviews[i].text = CharacterDefinition.DisplayValue(stat, before) + "  >  " + CharacterDefinition.DisplayValue(stat, after);
                levelEffects[i].text = SurvivorChoice.Effect(stat);
            }
            levelReroll.interactable = build.Materials >= build.RerollCost;
            Label(levelReroll).text = "YENİLE  ·  " + build.RerollCost + " ALTIN";
            levelHint.text = run.SaveError ?? (build.PendingLevels > 1 ? "Bu seçimden sonra " + (build.PendingLevels - 1) + " güçlendirme daha." : "Seçimini yap, ardından mağazada build’ini tamamla.");
        }
    }

    public static class SurvivorChoice
    {
        public static string Effect(RunStat stat)
        {
            switch (stat)
            {
                case RunStat.MaxHealth: return "Daha fazla darbeye dayan.";
                case RunStat.Damage: return "Bütün silahların daha sert vurur.";
                case RunStat.AttackSpeed: return "Daha sık saldır. Yüksek hızda getirisi azalır.";
                case RunStat.MoveSpeed: return "Çemberden çık, saldırıları atlat.";
                case RunStat.Armor: return "Aldığın darbe hasarını azalt.";
                case RunStat.Critical: return "İki kat hasar şansını artır.";
                case RunStat.Regeneration: return "Her 5 saniyede can yenile.";
                default: return "Dalga sonunda daha çok altın.";
            }
        }
    }
}
