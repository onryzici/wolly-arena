using System;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WoollyArena.Editor
{
    [InitializeOnLoad]
    public static class SurvivalReview
    {
        const string Active = "Woolly.SurvivalReview.Active", Result = "Logs/survival-review.txt";
        static int step, shots, kills;
        static double started, next;
        static Vector3 position;
        static SurvivalReview() { if (SessionState.GetBool(Active, false)) Attach(); }
        [MenuItem("Woolly/Review Survival")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play before reviewing.");
            Directory.CreateDirectory("Logs"); File.WriteAllText(Result, "Equipment, stats and survival review\n");
            ModelChecks();
            EditorSceneManager.OpenScene("Assets/Woolly/Scenes/TrainingArena.unity");
            SessionState.SetBool(Active, true); Attach(); EditorApplication.EnterPlaymode();
        }
        static void Attach()
        {
            SurvivalRun.CheckpointsEnabled = false;
            step = 0; started = EditorApplication.timeSinceStartup; next = started + 2;
            EditorApplication.update -= Tick; EditorApplication.update += Tick;
            Application.logMessageReceived -= Log; Application.logMessageReceived += Log;
        }
        static void Log(string message, string trace, LogType type)
        {
            if (Application.isBatchMode && message.StartsWith("FMOD failed to switch back")) { File.AppendAllText(Result, "ENVIRONMENT NOTE " + message + "\n"); return; }
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) File.AppendAllText(Result, "RUNTIME ERROR " + message + "\n");
        }
        static void Check(bool condition, string text)
        {
            if (!condition) throw new InvalidOperationException(text);
            File.AppendAllText(Result, "PASS " + text + "\n");
        }
        static void Offer(SurvivalBuild build, string id, int index = 0, int tier = 1, int price = 10)
        { build.Offers[index] = new ShopOffer { Item = RunCatalog.Find(id), Tier = tier, Price = price }; }
        public static void ModelChecks() { SurvivalBuildChecks.Run(Check); RunCheckpointChecks.Run(Check); WaveClearChecks.Run(Check); ExpansionChecks.Run(Check); }
        static void Expire(SurvivalRun run) {
            if(run.BossAlive){typeof(EnemyAgent).GetField("born",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(run.Boss,Time.time-1);run.Boss.Damage(1000000,Vector3.forward);}
            typeof(SurvivalRun).GetField("<Remaining>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(run, .001f);
        }
        static Button Button(string name) => Object.FindAnyObjectByType<ArenaHUD>().GetComponentsInChildren<Button>().First(x => x.name == name);
        static void LevelChoices(SurvivalRun run)
        {
            int count = 0;
            while (run.Phase == SurvivalRun.RunPhase.LevelUp && count++ < 100) run.ChooseLevel(0);
        }
        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup - started > 130) throw new TimeoutException("Review timed out at " + step);
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
                var run = Object.FindAnyObjectByType<SurvivalRun>(); var player = Object.FindAnyObjectByType<ArenaPlayer>();
                var director = Object.FindAnyObjectByType<EnemySpawnDirector>(); var v = player ? player.GetComponent<CharacterVitals>() : null;
                switch (step)
                {
                    case 0:
                        if (!run || !player) return;
                        Check(run.Build.Weapons.Count == 1 && run.Phase == SurvivalRun.RunPhase.Wave && run.totalWaves == 20, "Arena starts a twenty-wave equipment run");
                        Check(player.Stats && player.Loadout && player.autoCombat && !player.respawnOnDeath, "Stats and loadout controllers are connected to the player");
                        Check(Object.FindObjectsByType<EventSystem>().Length == 1, "Scene has exactly one UI event system");
                        v.ProtectFor(90); next = EditorApplication.timeSinceStartup + 12; step++; break;
                    case 1:
                        Check(player.Loadout.Shots > 0 && run.Kills > 0, "Equipped revolver automatically targets and defeats real enemies");
                        Check(run.Materials == run.Rewards.GoldCollected && run.Rewards.GoldDropped >= run.Kills * 3 && run.Build.Level >= 2, "Kills drop collectible gold and award XP levels");
                        Capture("equipment-wave", 1600, 900);
                        player.mobile.move.OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = 17, position = RectTransformUtility.WorldToScreenPoint(null, player.mobile.move.transform.position) });
                        Expire(run); next = EditorApplication.timeSinceStartup + WaveClearTimeline.Duration + .3; step++; break;
                    case 2:
                        Check(run.Phase == SurvivalRun.RunPhase.LevelUp && player.CombatPaused && !player.mobile.move.Held && !player.RequestDodge(), "Wave end queues stat choices and releases combat inputs");
                        Check(!run.Reroll() && !run.SellWeapon(0), "Shop mutations cannot bypass the level choice phase");
                        run.BeginNextWave(); Check(run.Wave == 1, "Pending levels block next wave");
                        Capture("level-up", 1600, 900); Capture("level-up", 1440, 1080);
                        LevelChoices(run); Check(run.Phase == SurvivalRun.RunPhase.Upgrade && run.Build.PendingLevels == 0, "All level choices resolve before the shop");
                        Check(run.LastHarvest == 5 && run.Rewards.GoldCollected == run.Rewards.GoldDropped && run.Materials == run.Rewards.GoldCollected + 5, "Wave end banks every gold drop and pays harvest once");
                        run.Build.AddMaterials(250);
                        Offer(run.Build, "vest", 0); Offer(run.Build, "revolver", 1); Offer(run.Build, "repeater", 2); Offer(run.Build, "galaxy", 3);
                        run.ShopUI.Refresh();
                        Capture("equipment-shop", 1600, 900); Capture("equipment-shop", 2400, 1080); Capture("equipment-shop", 1440, 1080);
                        position = player.transform.position; shots = player.Loadout.Shots;
                        next = EditorApplication.timeSinceStartup + .5; step++; break;
                    case 3:
                        Check(player.Loadout.Shots == shots && Vector3.Distance(position, player.transform.position) < .01f, "Shop freezes combat and movement");
                        int hp = v.maxHealth; float speed = player.runSpeed;
                        Button("Upgrade 0").onClick.Invoke();
                        Check(v.maxHealth == hp + 20 && player.runSpeed < speed && v.Armor == run.Build.Stat(RunStat.Armor), "Buying armor equipment updates live health, speed and mitigation; paused=" + run.IsPaused + " hp=" + v.maxHealth + " previous=" + hp + " speed=" + player.runSpeed.ToString("R") + " previous=" + speed.ToString("R") + " items=" + run.Build.Items.Count);
                        Button("Upgrade 1").onClick.Invoke(); Button("Weapon Slot 0").onClick.Invoke(); Button("Combine").onClick.Invoke();
                        Check(run.Build.Weapons.Count == 1 && run.Build.Weapons[0].Tier == 2, "Inventory combine button upgrades the equipped weapon");
                        Button("Upgrade 2").onClick.Invoke(); Button("Upgrade 3").onClick.Invoke();
                        Offer(run.Build, "energy", 0); Offer(run.Build, "star", 1); Offer(run.Build, "shotgun", 2); Offer(run.Build, "canteen", 3); run.ShopUI.Refresh();
                        for (int i = 0; i < 4; i++) Button("Upgrade " + i).onClick.Invoke();
                        Check(run.Build.Weapons.Count == 6 && run.Powers.Level(0) > 0 && run.Powers.Level(1) > 0 && run.Powers.Level(2) > 0, "Six-slot loadout includes guns and all automatic powers");
                        v.ProtectFor(0); int health = v.Health; int expected = Mathf.Max(1, Mathf.RoundToInt(20 * SurvivalBuild.ArmorMultiplier(v.Armor))); v.Damage(20);
                        Check(v.Health == health - expected, "Armor reduces actual incoming health damage");
                        v.ProtectFor(90); kills = run.Kills; shots = player.Loadout.Shots;
                        Button("Next Wave").onClick.Invoke();
                        Check(run.Wave == 2 && run.Phase == SurvivalRun.RunPhase.Wave && !player.CombatPaused, "Next wave retains purchased equipment and resumes combat");
                        v.ProtectFor(90); next = EditorApplication.timeSinceStartup + 7; step++; break;
                    case 4:
                        Check(player.Loadout.Shots > shots + 6 && run.Powers.Casts >= 3 && run.Kills > kills, "Multiple automatic weapons and powers attack in the live arena");
                        Capture("full-loadout", 1600, 900);
                        int regenStart = v.Health; v.ProtectFor(0); v.Damage(20); v.ProtectFor(90);
                        healthAfterHit = v.Health; next = EditorApplication.timeSinceStartup + 5.3; step++; break;
                    case 5:
                        Check(v.Health > healthAfterHit, "Regeneration restores real health during a wave");
                        Expire(run); next = EditorApplication.timeSinceStartup + WaveClearTimeline.Duration + .3; step++; break;
                    case 6:
                        LevelChoices(run); Check(run.Phase == SurvivalRun.RunPhase.Upgrade, "Later waves return to shop after level choices");
                        Check(run.Powers.Effects.ActiveCount == 0, "Wave end cancels active power effects");
                        // Actual UI selection/recycling, including removal of armor penalties.
                        Button("Owned Item 0").onClick.Invoke(); int count = run.Build.Items.Count; Button("Recycle").onClick.Invoke();
                        Check(run.Build.Items.Count == count - 1, "Item recycle removes one owned item: " + count + " -> " + run.Build.Items.Count);
                        Check(Mathf.Approximately(player.runSpeed, 4.8f * run.Build.MoveMultiplier), "Item recycle updates live speed: " + player.runSpeed.ToString("R") + " expected " + (4.8f * run.Build.MoveMultiplier).ToString("R"));
                        run.BeginNextWave(); v.ProtectFor(0); v.Damage(100000);
                        next = EditorApplication.timeSinceStartup + 2.5; step++; break;
                    case 7:
                        Check(run.Phase == SurvivalRun.RunPhase.Defeat && v.Health == 0 && player.CombatPaused, "Defeat blocks automatic respawn");
                        Capture("equipment-defeat", 1600, 900); Button("Next Wave").onClick.Invoke(); next = EditorApplication.timeSinceStartup + 1; step++; break;
                    case 8:
                        Check(run.Build.Items.Count == 0 && run.Build.Weapons.Count == 1 && run.Build.Level == 1 && run.Wave == 1, "Replay resets inventory, stats, XP and waves");
                        v.ProtectFor(100); Expire(run); next = EditorApplication.timeSinceStartup + WaveClearTimeline.Duration + .3; step++; break;
                    case 9:
                        if (run.Wave < 20)
                        {
                            LevelChoices(run); Check(run.Phase == SurvivalRun.RunPhase.Upgrade, "Wave " + run.Wave + " completes to a usable shop");
                            run.BeginNextWave(); v.ProtectFor(100); Expire(run); next = EditorApplication.timeSinceStartup + WaveClearTimeline.Duration + .3;
                        }
                        else
                        {
                            Check(run.Phase == SurvivalRun.RunPhase.Victory && player.CombatPaused, "Wave twenty ends in victory");
                            Capture("equipment-victory", 1600, 900); Button("Next Wave").onClick.Invoke(); next = EditorApplication.timeSinceStartup + 1; step++;
                        }
                        break;
                    case 10:
                        Check(run && run.Wave == 1 && run.Build.Weapons.Count == 1, "Victory replay creates a clean build");
                        SceneManager.LoadScene("Assets/Woolly/Scenes/Lobby.unity"); next = EditorApplication.timeSinceStartup + 1; step++; break;
                    case 11:
                        Check(Object.FindAnyObjectByType<LobbyScreen>() && !run && Time.timeScale == 1, "Returning to lobby leaves no paused run behind");
                        Check(!File.ReadAllText(Result).Contains("RUNTIME ERROR"), "No runtime errors during integration review");
                        File.AppendAllText(Result, "ALL PASSED\n"); Finish(); break;
                }
            }
            catch (Exception e) { File.AppendAllText(Result, "FAIL step " + step + ": " + e + "\n"); Finish(); }
        }
        static int healthAfterHit;
        static void Finish()
        {
            SessionState.SetBool(Active, false); EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
            SurvivalRun.CheckpointsEnabled = true;
            EditorApplication.ExitPlaymode();
        }
        static void Capture(string phase, int width, int height)
        {
            var hud = Object.FindAnyObjectByType<ArenaHUD>(); var canvas = hud.GetComponent<Canvas>(); var scaler = hud.GetComponent<CanvasScaler>(); var camera = Camera.main;
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active; var mode = canvas.renderMode; var oldCamera = canvas.worldCamera;
            float plane = canvas.planeDistance, scale = canvas.scaleFactor; bool scaling = scaler.enabled;
            var target = new RenderTexture(width, height, 24); var image = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                scaler.enabled = false; camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.scaleFactor = height / 900f;
                Canvas.ForceUpdateCanvases(); foreach (var text in hud.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(); camera.Render();
                RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, width, height), 0, 0); image.Apply(); File.WriteAllBytes($"Logs/survival-{phase}-{width}x{height}.png", image.EncodeToPNG());
                var panel = hud.transform.Find("SafeArea/Wave Upgrade");
                if (panel && panel.gameObject.activeInHierarchy)
                {
                    foreach (var button in panel.GetComponentsInChildren<Button>())
                    {
                        var corners = new Vector3[4]; ((RectTransform)button.transform).GetWorldCorners(corners);
                        if (!corners.All(c => { var p = camera.WorldToScreenPoint(c); return p.x >= -1 && p.y >= -1 && p.x <= width + 1 && p.y <= height + 1; })) throw new Exception("Out of bounds " + button.name);
                    }
                    Check(true, phase + " " + width + "x" + height + ": all controls fit viewport");
                }
            }
            finally
            {
                camera.targetTexture = oldTarget; RenderTexture.active = oldActive; canvas.renderMode = mode; canvas.worldCamera = oldCamera; canvas.planeDistance = plane; canvas.scaleFactor = scale; scaler.enabled = scaling;
                Object.DestroyImmediate(image); Object.DestroyImmediate(target);
            }
        }
    }
}
