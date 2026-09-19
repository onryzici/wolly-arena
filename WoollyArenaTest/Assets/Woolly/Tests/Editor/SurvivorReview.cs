using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WoollyArena.Editor
{
    [InitializeOnLoad]
    public static class SurvivorReview
    {
        const string Active = "Woolly.SurvivorReview", Report = "Logs/survivor-runtime-review.txt";
        static int step, errors, gold, pending, announced;
        static double next, started;
        static SurvivalRun run;
        static EnemyAgent mage;
        static float initialMageDistance;
        static bool previousPersistence;
        static SurvivorReview() { if (SessionState.GetBool(Active, false)) Attach(); EditorApplication.update += Requests; }
        static void Requests()
        {
            if (!File.Exists("Logs/survivor-review.request") || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            File.Delete("Logs/survivor-review.request"); Run();
        }
        [MenuItem("Woolly/Review Survivor UI And Squads")]
        public static void Run()
        {
            if (EditorApplication.isPlaying) { EditorApplication.ExitPlaymode(); EditorApplication.delayCall += Run; return; }
            Directory.CreateDirectory("Logs"); File.WriteAllText(Report, "Survivor UI / tactical squads Play Mode review\n");
            SurvivorChecks.Run(Check);
            EditorSceneManager.OpenScene("Assets/Woolly/Scenes/TrainingArena.unity");
            SessionState.SetBool(Active, true); Attach(); EditorApplication.EnterPlaymode();
        }
        static void Attach()
        {
            previousPersistence = SurvivalRun.CheckpointsEnabled; SurvivalRun.CheckpointsEnabled = false; RunSession.ResumeRequested = false;
            Application.runInBackground = true; step = -1; errors = 0; run = null; started = EditorApplication.timeSinceStartup; next = started + 2;
            EditorApplication.update -= Tick; EditorApplication.update += Tick; Application.logMessageReceived -= Log; Application.logMessageReceived += Log;
        }
        static void Log(string text, string trace, LogType type) { if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) { errors++; File.AppendAllText(Report, "ERROR " + text + "\n" + trace + "\n"); } }
        static void Check(bool pass, string message) { if (!pass) throw new Exception(message); File.AppendAllText(Report, "PASS " + message + "\n"); }
        static void Set(object obj, string field, object value) => obj.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(obj, value);
        static void Clear()
        {
            run.Director.CancelSquads(); foreach (var enemy in run.Director.Enemies) if (enemy) Object.Destroy(enemy.gameObject); run.Director.Enemies.Clear();
        }
        static Button Button(string name) => Object.FindAnyObjectByType<ArenaHUD>().GetComponentsInChildren<Button>(true).First(b => b.name == name);
        static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup - started > 150) throw new TimeoutException("Survivor review step " + step);
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
                if (!run) run = Object.FindAnyObjectByType<SurvivalRun>(); if (!run) return;
                switch (step)
                {
                    case -1:
                        ReviewArtwork();
                        run.enabled = false; run.Player.GetComponent<CharacterVitals>().ProtectFor(999); run.Player.Loadout.enabled = false; Clear();
                        var stick = run.Player.mobile.move;
                        Check(stick.GetComponentsInChildren<SurvivorStickFace>().Length == 2 && stick.GetComponentsInChildren<SurvivorStickFace>().All(f => f.GetComponent<CanvasRenderer>()), "Both flat joystick faces have renderers and are active");
                        var stickCanvas = stick.GetComponentInParent<Canvas>();
                        var pointer = new PointerEventData(EventSystem.current) { pointerId = 71, position = RectTransformUtility.WorldToScreenPoint(stickCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : stickCanvas.worldCamera, stick.transform.TransformPoint(Vector3.right * stick.radius)) };
                        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);
                        Check(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<TouchStick>() == stick, "Styled joystick remains the real pointer hit target; hits=" + string.Join(",", hits.Select(h => h.gameObject.name)) + " position=" + pointer.position + " screen=" + Screen.width + "x" + Screen.height + " mode=" + stick.GetComponentInParent<Canvas>().renderMode + " input=" + stick.GetComponent<Graphic>().raycastTarget);
                        pointer.pointerPressRaycast = pointer.pointerCurrentRaycast = hits[0];
                        ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer, ExecuteEvents.pointerDownHandler);
                        next = EditorApplication.timeSinceStartup + .45; step++; break;
                    case 0:
                        Check(run.Player.transform.position.sqrMagnitude > .15f, "Pointer input on the styled joystick moves the actual player");
                        run.Player.mobile.move.ResetInput(); var motor = run.Player.GetComponent<CharacterController>(); motor.enabled = false; run.Player.transform.position = Vector3.zero; motor.enabled = true;
                        run.enabled = false; run.Player.GetComponent<CharacterVitals>().ProtectFor(999); run.Player.Loadout.enabled = false; Clear();
                        Check(run.Player.gameObject.activeInHierarchy, "Removing duplicate overhead readouts keeps the player active");
                        Capture("hud", 1600, 900); Capture("hud", 2400, 1080); Capture("hud", 1440, 1080);
                        run.Build.AddExperience(100); run.Build.OpenShop(5); run.Build.AddMaterials(100); run.Build.RollLevelChoices();
                        for (int i = 0; i < 4; i++) { run.Build.LevelChoices[i] = (RunStat)i; run.Build.LevelChoiceTiers[i] = i + 1; }
                        Set(run, "<Phase>k__BackingField", SurvivalRun.RunPhase.LevelUp); run.Player.CombatPaused = true; run.ShopUI.Refresh();
                        next = EditorApplication.timeSinceStartup + .7; step++; break;
                    case 1:
                        Capture("level", 1600, 900); Capture("level", 2400, 1080); Capture("level", 1440, 1080);
                        gold = run.Materials; int cost = run.Build.RerollCost; pending = run.Build.PendingLevels;
                        Button("Reroll Level Choices").onClick.Invoke();
                        Check(run.Materials == gold - cost && run.Build.PendingLevels == pending, "Level reroll button spends the displayed cost without consuming the reward");
                        int expected = run.Build.StatAfterBonus(run.Build.LevelChoices[0], SurvivalBuild.LevelBonus(run.Build.LevelChoices[0], run.Build.LevelChoiceTiers[0]));
                        var stat = run.Build.LevelChoices[0]; Button("Level Choice 0").onClick.Invoke();
                        Check(run.Build.PendingLevels == pending - 1 && run.Build.Stat(stat) == expected, "Actual level button applies its previewed stat once");
                        while (run.Build.PendingLevels > 0) Button("Level Choice 0").onClick.Invoke();
                        Check(run.Phase == SurvivalRun.RunPhase.Upgrade && !Button("Level Choice 0").gameObject.activeInHierarchy, "Last reward closes the level screen and opens the shop");
                        Capture("shop", 1600, 900); Capture("shop", 2400, 1080); Capture("shop", 1440, 1080);
                        Set(run, "<Phase>k__BackingField", SurvivalRun.RunPhase.Wave); Set(run, "<Wave>k__BackingField", 12); run.Player.CombatPaused = false; run.ShopUI.Refresh(); Clear();
                        announced = run.Director.QueueSquads(12);
                        Check(announced >= 6 && run.Director.PendingCount == announced && run.Director.Enemies.Count == 0, "Several squads are announced before enemies appear");
                        Capture("spawn-warnings", 1600, 900); run.SetPaused(true); next = EditorApplication.timeSinceStartup + 1.4; step++; break;
                    case 2:
                        Check(run.Director.PendingCount == announced && run.Director.Enemies.Count == 0, "Pause freezes announced squad arrivals");
                        run.SetPaused(false); next = EditorApplication.timeSinceStartup + 1.55; step++; break;
                    case 3:
                        var groups = run.Director.Enemies.Where(e => e && !e.Defeated).ToArray();
                        Check(groups.Length >= 6 && groups.Select(e => e.SpawnSector).Distinct().Count() >= 2, "Live groups arrive from distinct arena regions");
                        Check(groups.All(e => e.SquadId > 0 && e.GetComponent<NavMeshAgent>().isOnNavMesh), "Squad members receive group identity and valid navigation");
                        Check(groups.Select(e => e.Role).Distinct().Count() >= 3, "Live squad composition includes distinct tactical roles");
                        Check(groups.Any(e => e.Role == EnemyRole.Flanker && Vector3.Distance(e.TacticalGoal, run.Player.transform.position) > 1), "Live flankers choose a side approach on the arena NavMesh");
                        Capture("squads", 1600, 900);
                        run.Director.QueueSquads(100);
                        Check(run.Director.PendingCount <= SquadRules.Capacity && run.Director.PendingCount + groups.Length <= run.EnemyLimit, "Large arrival request respects pending and population budgets");
                        Clear(); next = EditorApplication.timeSinceStartup + .3; step++; break;
                    case 4:
                        run.Director.Spawn(); mage = run.Director.Enemies.Last(); mage.Role = EnemyRole.Skirmisher; mage.LaserRanged = true;
                        mage.GetComponent<NavMeshAgent>().enabled = false;
                        Check(NavMesh.SamplePosition(run.Player.transform.position + Vector3.right * 3, out var location, 1, NavMesh.AllAreas), "Ranged retreat test has a valid starting cell");
                        mage.transform.position = location.position; initialMageDistance = Vector3.Distance(location.position, run.Player.transform.position);
                        next = EditorApplication.timeSinceStartup + 1.7; step++; break;
                    case 5:
                        Check(mage && Vector3.Distance(mage.transform.position, run.Player.transform.position) > initialMageDistance + .45f, "Ranged enemy actually retreats on the arena NavMesh");
                        Clear(); run.Director.QueueSquads(8); Set(run, "<Phase>k__BackingField", SurvivalRun.RunPhase.Upgrade);
                        next = EditorApplication.timeSinceStartup + 1.4; step++; break;
                    case 6:
                        Check(run.Director.PendingCount == 0 && run.Director.Enemies.Count == 0, "Wave transition cancels queued spawns and warning markers");
                        Set(run, "<Wave>k__BackingField", 0); run.enabled = true; run.Player.Loadout.enabled = true; run.BeginNextWave();
                        run.Player.GetComponent<CharacterVitals>().ProtectFor(999);
                        next = EditorApplication.timeSinceStartup + 6; step++; break;
                    case 7:
                        Check(run.Director.Enemies.Count > 0 && run.Player.Loadout.Shots > 0, "Normal wave loop spawns squads and keeps automatic combat running");
                        Capture("combat", 1600, 900); next = EditorApplication.timeSinceStartup + .25; step++; break;
                    case 8:
                        if (run.Phase == SurvivalRun.RunPhase.Wave || run.Phase == SurvivalRun.RunPhase.WaveClear) { next = EditorApplication.timeSinceStartup + .25; break; }
                        Check(run.Kills > 0 && (run.Phase == SurvivalRun.RunPhase.LevelUp || run.Phase == SurvivalRun.RunPhase.Upgrade), "A complete timed wave awards kills and reaches the reward/shop flow");
                        Check(run.Director.PendingCount == 0, "Natural wave completion clears all announced arrivals");
                        Clear();run.enabled=false;run.Player.Loadout.enabled=false;
                        Set(run,"<Phase>k__BackingField",SurvivalRun.RunPhase.Wave);Set(run,"<Wave>k__BackingField",16);Set(run,"<Remaining>k__BackingField",35f);
                        run.Build.Weapons.Clear();foreach(var id in new[]{"blade","flame","frost","grenade","axe","railgun"})run.Build.Weapons.Add(new OwnedGear(RunCatalog.Find(id),1,20));
                        Set(run.Build,"<Revision>k__BackingField",run.Build.Revision+1);run.Player.Stats.Apply();run.Player.CombatPaused=false;run.ShopUI.Refresh();
                        run.Director.QueueSquads(24);next=EditorApplication.timeSinceStartup+1.6;step++;break;
                    case 9:
                        run.Director.QueueSquads(24);next=EditorApplication.timeSinceStartup+1.6;step++;break;
                    case 10:
                        run.Director.QueueSquads(12);next=EditorApplication.timeSinceStartup+1.6;step++;break;
                    case 11:
                        Check(run.Director.Enemies.Count>=24,"Crowded visual fixture contains at least twenty-four live enemies");
                        run.Player.Loadout.enabled=true;next=EditorApplication.timeSinceStartup+1.25;step++;break;
                    case 12:
                        Capture("crowded-combat",1600,900);
                        foreach(var enemy in run.Director.Enemies.Where(e=>e&&!e.Defeated).Take(8).ToArray())enemy.Damage(99999,(enemy.transform.position-run.Player.transform.position).normalized,true);
                        next=EditorApplication.timeSinceStartup+.13;step++;break;
                    case 13:
                        Capture("death-effects",1600,900);
                        Check(run.DeathFX.ActiveCount>0&&run.DeathFX.ActiveCount<=EnemyDeathVFX.MaximumFragments,"Simultaneous defeats use the bounded fragment pool");
                        var clouds=Object.FindObjectsByType<ParticleSystem>().Where(p=>p.name=="Pooled defeat dust"||p.name=="Pooled elemental billows").ToArray();
                        Check(clouds.Length==2&&clouds.All(p=>p.particleCount<=p.main.maxParticles),"Both VFX particle systems respect their fixed budgets");
                        Check(errors == 0, "No runtime exceptions or error logs");
                        File.AppendAllText(Report, "COMPLETE: real UI buttons and joystick input, three aspect ratios, squad warnings, pause, population caps, region variety, flanking, ranged retreat and one complete wave with invulnerability for integration testing. Physical device and full human run not tested.\n");
                        Finish(); break;
                }
            }
            catch (Exception e) { File.AppendAllText(Report, "FAIL " + e + "\n"); Finish(); }
        }
        static void Finish() { SessionState.SetBool(Active, false); EditorApplication.update -= Tick; Application.logMessageReceived -= Log; SurvivalRun.CheckpointsEnabled = previousPersistence; EditorApplication.ExitPlaymode(); }
        static void ReviewArtwork()
        {
            var probe=new GameObject("Inventory import probe",typeof(RectTransform));
            try {
                var icon=probe.AddComponent<ArenaIcon>();var regions=new HashSet<Rect>();
                foreach(var item in RunCatalog.All){icon.Set(item.Id);Check(icon.sprite&&icon.sprite.texture.name=="FlatInventory"&&regions.Add(icon.sprite.rect),"Unique flat cartoon artwork imported for "+item.Id);}
                Check(regions.Count==36,"All twenty weapons and sixteen items have separate illustrated icons");
            } finally { Object.DestroyImmediate(probe); }
            string[] names={"barrel_large_decorated","box_stacked","keg_decorated","sword_shield_broken","sword_shield_gold","rubble_large","coin_stack_large","bottle_A_green"};
            foreach(var name in names){var prefab=Resources.Load<GameObject>("KayKit/"+name);Check(prefab&&prefab.GetComponentsInChildren<MeshFilter>().Any(f=>f.sharedMesh&&f.sharedMesh.vertexCount>0),"KayKit model imported: "+name);}
            var attached=Object.FindObjectsByType<KayKitAsset>();
            Check(names.All(name=>attached.Any(a=>a.name==name)),"All eight KayKit additions are placed in the actual arena");
            var billboard=run.Player.GetComponentInChildren<CharacterBillboard>(true);
            Check(!billboard||billboard.ammoPips.All(p=>!p||!p.gameObject.activeInHierarchy),"Overhead ammunition pips are hidden");
            var shader=Shader.Find("Woolly/SelectionRing");Check(shader&&!ShaderUtil.ShaderHasError(shader),"Single-color player marker shader compiles");
            Check(!File.Exists("Assets/Scenes/SampleScene.unity")&&EditorBuildSettings.scenes.All(s=>File.Exists(s.path)),"Unused sample scene removed and all build scenes remain valid");
        }
        static void Capture(string label, int width, int height)
        {
            var camera = Camera.main; var canvas = Object.FindAnyObjectByType<ArenaHUD>().GetComponent<Canvas>(); var scaler = canvas.GetComponent<CanvasScaler>();
            var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active; var oldMode = canvas.renderMode; var oldCamera = canvas.worldCamera; float plane = canvas.planeDistance, scale = canvas.scaleFactor; bool scaling = scaler.enabled;
            var target = new RenderTexture(width, height, 24); var picture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                scaler.enabled = false; camera.targetTexture = target; canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; canvas.scaleFactor = height / 900f;
                Canvas.ForceUpdateCanvases(); foreach (var text in canvas.GetComponentsInChildren<TMP_Text>()) text.ForceMeshUpdate(); camera.Render(); RenderTexture.active = target;
                picture.ReadPixels(new Rect(0, 0, width, height), 0, 0); picture.Apply(); File.WriteAllBytes($"Logs/survivor-{label}-{width}x{height}.png", picture.EncodeToPNG());
            }
            finally { camera.targetTexture = oldTarget; RenderTexture.active = oldActive; canvas.renderMode = oldMode; canvas.worldCamera = oldCamera; canvas.planeDistance = plane; canvas.scaleFactor = scale; scaler.enabled = scaling; Object.DestroyImmediate(picture); Object.DestroyImmediate(target); }
        }
    }
}
