using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

namespace WoollyArena
{
    public sealed class SurvivalRun : MonoBehaviour
    {
        public enum RunPhase { Wave, Upgrade, Defeat, Victory, LevelUp, WaveClear }
        public RunPhase Phase { get; private set; }
        public int Wave { get; private set; } = 1;
        public int Materials => Build == null ? 0 : Build.Materials;
        public int Kills { get; private set; }
        public SurvivalBuild Build { get; private set; }
        public SurvivalPowers Powers { get; private set; }
        public float Remaining { get; private set; }
        public int totalWaves = 20;
        public float waveDuration = 30;
        public float CurrentWaveDuration=>Wave==totalWaves?90:waveDuration+Mathf.Min(14,Wave-1);
        public CombatRewards Rewards { get; private set; }
        public CombatAudio Sfx { get; private set; }
        public ArenaActionFX ActionFX { get; private set; }
        public int EnemyLimit => WaveDifficulty.EnemyLimit(Wave);
        public int SpawnBurst => Mathf.Min(6, 3 + (Wave - 1) / 4);
        public float SpawnInterval => WaveDifficulty.SpawnInterval(Wave);
        public int LastHarvest { get; private set; }
        public ArenaPlayer Player { get; private set; }
        public SurvivalShopUI ShopUI { get; private set; }
        readonly WaveClearTimeline clearTimeline=new WaveClearTimeline();
        int sweptEnemies, killsAtWaveStart, goldAtWaveStart;
        bool bossSpawned;float nextBossAttempt;
        public EnemyAgent Boss {get;private set;}
        public int Biome {get;private set;}
        public bool BossAlive => Boss && !Boss.Defeated;
        public int WaveKills=>Kills-killsAtWaveStart;
        public int WaveGold=>Rewards.GoldCollected-goldAtWaveStart;
        public float ClearProgress=>clearTimeline.Progress;
        EnemySpawnDirector director;
        CharacterVitals vitals;
        float nextSpawn, previousTimeScale = 1;
        public bool IsPaused { get; private set; }
        public string SaveError { get; private set; }
        public bool WasRestored { get; private set; }
        public static bool CheckpointsEnabled = true; // Editor reviews temporarily disable persistence.


        public void Initialize(EnemySpawnDirector source, ArenaPlayer hero)
        {
            Biome=ArenaBiome.ActiveBiome;director = source; Player = hero; vitals = hero.GetComponent<CharacterVitals>();
            RunCheckpoint checkpoint = null;
            if (CheckpointsEnabled && RunSession.ResumeRequested) RunCheckpointStore.TryLoad(RunSession.SavePath, out checkpoint);
            RunSession.ResumeRequested = false;
            Build = checkpoint != null ? checkpoint.Build : new SurvivalBuild(Random.Range(1, int.MaxValue), PlayableCharacter.Active);
            RunSession.ApplySound();
            vitals.ResetHealth(100); Player.autoCombat = true; Player.respawnOnDeath = false;
            Player.Stats = hero.gameObject.AddComponent<CharacterStats>(); Player.Stats.Initialize(Build, Player, this);
            Sfx=gameObject.AddComponent<CombatAudio>();Sfx.Initialize(this);
            ActionFX=gameObject.AddComponent<ArenaActionFX>();ActionFX.Initialize(this);
            Rewards = gameObject.AddComponent<CombatRewards>(); Rewards.Initialize(this);
            Powers = gameObject.AddComponent<SurvivalPowers>(); Powers.Initialize(Player, director, this);
            Player.Loadout = gameObject.AddComponent<LoadoutCombat>(); Player.Loadout.Initialize(this, Player);
            ShopUI = gameObject.AddComponent<SurvivalShopUI>(); ShopUI.Initialize(this);
            if (checkpoint != null)
            {
                WasRestored = true; Wave = checkpoint.Wave; Kills = checkpoint.Kills; LastHarvest = checkpoint.Harvest;
                vitals.Heal(vitals.maxHealth); vitals.ProtectFor(0); vitals.RestoreHealth(checkpoint.Health);
                Phase = (RunPhase)checkpoint.Phase;
                for (int i = 0; i < 3; i++) Powers.SetLevel(i, Build.PowerLevel(i));
                if (Phase == RunPhase.Wave) BeginWave();
                else { Player.CombatPaused = true; ShopUI.Refresh(); }
            }
            else BeginWave();
        }
        void BeginWave()
        {
            bossSpawned=false;Boss=null;nextBossAttempt=0;
            killsAtWaveStart=Kills;goldAtWaveStart=Rewards.GoldCollected;
            Player.Stats.Apply();vitals.Heal(vitals.maxHealth);
            ActionFX.Cue(3);ActionFX.Burst(Player.transform.position+Vector3.up*.6f,new Color(.35f,1,.75f),24);
            Player.weapon.Refill(); Phase = RunPhase.Wave;
            Remaining = CurrentWaveDuration; nextSpawn = Time.time + SpawnInterval;
            Player.CombatPaused = false; vitals.ProtectFor(1); ResetTouch(); ShopUI.Refresh(); SaveCheckpoint();
            TrySpawnBoss();SpawnGroup(Mathf.Min(EnemyLimit, 6 + Wave));
        }
        void ResetTouch()
        {
            if (!Player.mobile) return;
            if (Player.mobile.move) Player.mobile.move.ResetInput();
            if (Player.mobile.aim) Player.mobile.aim.ResetInput();
        }
        void Update()
        {
            if (!Player) return;
            if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true) SetPaused(!IsPaused);
            if (IsPaused) return;
            if (Phase == RunPhase.Wave)
            {
                if (vitals.Health <= 0) { EndPhase(RunPhase.Defeat); return; }
                Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
                director.Enemies.RemoveAll(e => !e);
                if(!bossSpawned)TrySpawnBoss();
                if (WaveDifficulty.ShouldClear(Wave,Remaining,bossSpawned,BossAlive)) { BeginWaveClear(); return; }
                if (Time.time >= nextSpawn)
                {
                    SpawnGroup(SpawnBurst);
                    nextSpawn = Time.time + SpawnInterval;
                }
            }
            if(Phase==RunPhase.WaveClear){
                clearTimeline.Advance(Time.deltaTime,false);
                int target=clearTimeline.SweepCount(director.Enemies.Count);
                while(sweptEnemies<target){var enemy=director.Enemies[sweptEnemies++];if(!enemy)continue;if(!enemy.Defeated)Rewards.Gore(enemy.transform.position,enemy.transform.position-Player.transform.position,true);enemy.gameObject.SetActive(false);Destroy(enemy.gameObject);}
                ShopUI.UpdateWaveClear(clearTimeline.Elapsed);
                if(clearTimeline.Ready){EndPhase(Wave>=totalWaves?RunPhase.Victory:RunPhase.Upgrade);return;}
            }
            ShopUI.UpdateHeader();
        }
        void BeginWaveClear(){
            Phase=RunPhase.WaveClear;Remaining=0;clearTimeline.Reset();sweptEnemies=0;
            Player.CombatPaused=true;vitals.ProtectFor(WaveClearTimeline.Duration+1);Powers.Cancel();ResetTouch();
            foreach(var enemy in director.Enemies)if(enemy)enemy.HaltForWaveClear();
            Rewards.BeginVictorySweep();ShopUI.Refresh();ShopUI.UpdateWaveClear(0);
        }
        void TrySpawnBoss(){
            if(!WaveDifficulty.BossWave(Wave)||bossSpawned||Time.time<nextBossAttempt)return;
            nextBossAttempt=Time.time+1;
            if(director.Spawn(true)){bossSpawned=true;Boss=director.Enemies[director.Enemies.Count-1];Rewards.Popup(Player.transform.position,Boss.vitals.displayName,new Color(1,.45f,.35f),1.1f,1.4f);}
        }
        void SpawnGroup(int count)
        {
            for (int i = 0; i < count && director.Enemies.Count < EnemyLimit; i++)
                if (!director.Spawn()) break;
        }
        public void RegisterKill()
        {
            if (Phase != RunPhase.Wave) return;
            int previousLevel=Build.Level;Kills++; Build.AddExperience(1);
            if(Build.Level!=previousLevel)Player.Stats.Apply();
        }
        void EndPhase(RunPhase phase)
        {
            Rewards.FinishWave(phase == RunPhase.Upgrade || phase == RunPhase.Victory);
            if(CheckpointsEnabled){var career=CareerStore.Load();career.Record(phase==RunPhase.Defeat?Wave-1:Wave,Kills,Build.Level,Build.Weapons.Count,phase==RunPhase.Victory);CareerStore.Save(career);}
            Phase = phase; Player.CombatPaused = true; Powers.Cancel(); ResetTouch();
            foreach (var enemy in director.Enemies) if (enemy) { enemy.gameObject.SetActive(false); Destroy(enemy.gameObject); }
            director.Enemies.Clear();
            if (phase == RunPhase.Upgrade)
            {
                LastHarvest = Build.Stat(RunStat.Harvesting); Build.AddMaterials(LastHarvest);
                Build.OpenShop(Wave);
                if (Build.PendingLevels > 0) { Phase = RunPhase.LevelUp; Build.RollLevelChoices(); }
            }
            if (phase == RunPhase.Defeat || phase == RunPhase.Victory)
            {
                if (CheckpointsEnabled) SaveError = RunSession.ClearCheckpoint();
            }
            else SaveCheckpoint();
            ShopUI.Refresh();
        }
        void ApplyBuild()
        {
            Player.Stats.Apply();
            if(CheckpointsEnabled){var career=CareerStore.Load();career.Record(Wave-1,Kills,Build.Level,Build.Weapons.Count,false);CareerStore.Save(career);}
            for (int i = 0; i < 3; i++) Powers.SetLevel(i, Build.PowerLevel(i));
            SaveCheckpoint(); ShopUI.Refresh();
        }
        public bool ChooseLevel(int index)
        {
            if (IsPaused || Phase != RunPhase.LevelUp || !Build.ChooseLevel(index)) return false;
            if (Build.PendingLevels == 0) Phase = RunPhase.Upgrade;
            Sfx.Play(CombatCue.Click);ApplyBuild(); return true;
        }
        public int UpgradeCost(int index) => index >= 0 && index < 4 && Build.Offers[index] != null ? Build.Offers[index].Price : int.MaxValue;
        public bool CanBuyUpgrade(int index) => !IsPaused && Phase == RunPhase.Upgrade && Build.CanBuy(index);
        public void ChooseUpgrade(int index)
        {
            if (IsPaused) return;
            if (Phase == RunPhase.Defeat || Phase == RunPhase.Victory) { if (index == 0) Replay(); return; }
            if (Phase == RunPhase.LevelUp) { ChooseLevel(index); return; }
            if (IsPaused || Phase != RunPhase.Upgrade || !Build.Buy(index)) return;
            Sfx.Play(CombatCue.Chest);ApplyBuild();
        }
        public bool Reroll() { if (IsPaused || (Phase==RunPhase.LevelUp ? !Build.RerollLevels() : Phase!=RunPhase.Upgrade || !Build.Reroll())) return false; SaveCheckpoint(); ShopUI.Refresh(); return true; }
        public bool ToggleLock(int index) { if (IsPaused || Phase != RunPhase.Upgrade || !Build.ToggleLock(index)) return false; SaveCheckpoint(); ShopUI.Refresh(); return true; }
        public bool Combine(int index) { if (IsPaused || Phase != RunPhase.Upgrade || !Build.Combine(index)) return false; Sfx.Play(CombatCue.Click);ApplyBuild(); return true; }
        public bool SellWeapon(int index) { if (IsPaused || Phase != RunPhase.Upgrade || !Build.SellWeapon(index)) return false; Sfx.Play(CombatCue.Click);ApplyBuild(); return true; }
        public bool SellItem(int index) { if (IsPaused || Phase != RunPhase.Upgrade || !Build.SellItem(index)) return false; Sfx.Play(CombatCue.Click);ApplyBuild(); return true; }
        public void BeginNextWave()
        {
            if (IsPaused || Phase != RunPhase.Upgrade || Build.PendingLevels > 0) return;
            Wave++; BeginWave();
        }
        void SaveCheckpoint()
        {
            if (!CheckpointsEnabled || !Player || vitals.Health <= 0 || Phase==RunPhase.WaveClear) return;
            try
            {
                RunCheckpointStore.Save(RunSession.SavePath, new RunCheckpoint { Build = Build, Wave = Wave, Phase = (int)Phase, Health = vitals.Health, Kills = Kills, Harvest = LastHarvest, Biome = Biome, Character = PlayableCharacter.Active });
                SaveError = null;
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException)
            { SaveError = "İlerleme kaydedilemedi. Cihazın depolama alanını kontrol et."; }
        }
        public void SetPaused(bool paused)
        {
            if (!Player || vitals.Health <= 0 || ShopUI == null || IsPaused == paused || Phase == RunPhase.Defeat || Phase == RunPhase.Victory) return;
            IsPaused = paused;
            if (paused)
            {
                SaveCareerSnapshot();
                previousTimeScale = Time.timeScale; Time.timeScale = 0;
                Player.CombatPaused = true; Powers.Cancel(); ResetTouch();
            }
            else
            {
                Time.timeScale = previousTimeScale; Player.CombatPaused = Phase != RunPhase.Wave;
                vitals.ProtectFor(1); ResetTouch();
            }
            ShopUI.ShowPause(paused);
        }
        void SaveCareerSnapshot()
        {
            if(!CheckpointsEnabled||Build==null)return;
            var career=CareerStore.Load();bool completed=Phase==RunPhase.Upgrade||Phase==RunPhase.LevelUp||Phase==RunPhase.Victory||Phase==RunPhase.WaveClear;
            career.Record(completed?Wave:Wave-1,Kills,Build.Level,Build.Weapons.Count,Phase==RunPhase.Victory);CareerStore.Save(career);
        }
        public void LeaveToLobby()
        {
            SaveCareerSnapshot();
            if (IsPaused) { Time.timeScale = previousTimeScale; IsPaused = false; }
            SceneManager.LoadSceneAsync("Lobby");
        }
        public void Replay()
        {
            if (IsPaused) { Time.timeScale = previousTimeScale; IsPaused = false; }
            RunSession.ResumeRequested = false;
            if (CheckpointsEnabled) SaveError = RunSession.ClearCheckpoint();
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }
        void OnApplicationPause(bool paused) { if (paused) SetPaused(true); }
        void OnApplicationFocus(bool focused) { if (!focused) SetPaused(true); }
        void OnDestroy() { if (IsPaused) Time.timeScale = previousTimeScale; }

    }
}
