using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
namespace WoollyArena.Editor
{
    public static class RunCheckpointChecks
    {
        public static void Run(Action<bool, string> check)
        {
            var build = new SurvivalBuild(812); build.AddMaterials(200); build.AddExperience(30); build.RollLevelChoices(); build.OpenShop(3);
            build.Offers[0] = new ShopOffer { Item = RunCatalog.Find("vest"), Tier = 2, Price = 25 };
            build.Buy(0); build.ToggleLock(1);
            var original = new RunCheckpoint { Build = build, Wave = 3, Phase = 4, Health = 88, Kills = 30, Harvest = 5 };
            byte[] bytes = RunCheckpointStore.Encode(original); var loaded = RunCheckpointStore.Decode(bytes);
            check(loaded.Wave == 3 && loaded.Phase == 4 && loaded.Health == 88 && loaded.Kills == 30, "Checkpoint restores wave, phase, health and kills");
            check(loaded.Build.Materials == build.Materials && loaded.Build.Level == build.Level && loaded.Build.PendingLevels == build.PendingLevels, "Checkpoint restores wallet, XP and pending levels");
            check(loaded.Build.Items.Count == 1 && loaded.Build.Items[0].Tier == 2 && loaded.Build.Stat(RunStat.Armor) == build.Stat(RunStat.Armor), "Checkpoint restores equipped passive modifiers");
            check(loaded.Build.Offers[0].Sold && loaded.Build.Offers[1].Locked && loaded.Build.Offers[1].Price == build.Offers[1].Price, "Sold state and locked offer price survive reload");
            check(loaded.Build.LevelChoices.SequenceEqual(build.LevelChoices), "Outstanding stat choices survive reload");
            build.ChooseLevel(0); loaded.Build.ChooseLevel(0);
            check(loaded.Build.LevelChoices.SequenceEqual(build.LevelChoices), "Following level choices use the same saved RNG state");
            build.Reroll(); loaded.Build.Reroll();
            check(build.Offers.Select(x => x.Item.Id + x.Tier + x.Price).SequenceEqual(loaded.Build.Offers.Select(x => x.Item.Id + x.Tier + x.Price)), "Reload cannot change the next shop reroll");
            check(build.Materials == loaded.Build.Materials && build.RerollCost == loaded.Build.RerollCost, "Reroll spending and next cost stay consistent after restore");
            Reject(check, null, "Null save is rejected"); Reject(check, new byte[20], "Truncated save is rejected"); Reject(check, new byte[1048577], "Oversized save is rejected");
            byte[] corrupt = (byte[])bytes.Clone(); corrupt[45] ^= 0x40; Reject(check, corrupt, "Corrupted payload fails checksum verification");
            corrupt = (byte[])bytes.Clone(); corrupt[4] = 99;
            using (var sha = SHA256.Create()) Buffer.BlockCopy(sha.ComputeHash(corrupt, 0, corrupt.Length - 32), 0, corrupt, corrupt.Length - 32, 32);
            Reject(check, corrupt, "Unknown save version is rejected even with a valid checksum");
            var valid = new RunCheckpoint { Build = new SurvivalBuild(11), Wave = 1, Phase = 0, Health = 100, Kills = 0 };
            check(RunCheckpointStore.Decode(RunCheckpointStore.Encode(valid)).Build.Weapons.Count == 1, "First-wave checkpoint works without shop offers");
            bool invalid = false; valid.Health = 101; try { RunCheckpointStore.Encode(valid); } catch (InvalidDataException) { invalid = true; }
            check(invalid, "Health beyond build maximum cannot be saved"); valid.Health = 100;
            invalid = false; valid.Phase = 2; try { RunCheckpointStore.Encode(valid); } catch (InvalidDataException) { invalid = true; }
            check(invalid, "Defeated runs cannot be encoded as resumable saves"); valid.Phase = 0;
            string directory = Path.Combine(Path.GetTempPath(), "woolly-save-check-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(directory, "run.sav");
            try
            {
                RunCheckpointStore.Save(path, valid);
                check(RunCheckpointStore.TryLoad(path, out var first) && first.Wave == 1, "Save creates a readable checkpoint on disk");
                valid.Wave = 2; RunCheckpointStore.Save(path, valid);
                check(File.Exists(path + ".bak") && RunCheckpointStore.TryLoad(path, out var second) && second.Wave == 2, "Atomic replacement retains previous checkpoint as backup");
                File.WriteAllBytes(path + ".tmp", new byte[] { 1, 2 });
                check(RunCheckpointStore.TryLoad(path, out second) && second.Wave == 2, "Abandoned partial write cannot replace the good checkpoint");
                File.WriteAllBytes(path, new byte[] { 3, 4 });
                check(RunCheckpointStore.TryLoad(path, out var backup) && backup.Wave == 1, "Damaged main save recovers the previous checkpoint");
                valid.Wave = 3; RunCheckpointStore.Save(path, valid);
                check(RunCheckpointStore.TryLoad(path, out var recovered) && recovered.Wave == 3, "Saving after recovery replaces the corrupt primary");
                check(RunCheckpointStore.Decode(File.ReadAllBytes(path + ".bak")).Wave == 1, "Recovery save preserves the healthy backup instead of copying corrupt data");
                File.WriteAllBytes(path, new byte[] { 9 });
                check(RunCheckpointStore.TryLoad(path, out backup) && backup.Wave == 1, "A second primary failure can still recover the healthy backup");
                File.WriteAllBytes(path + ".bak", new byte[] { 5 });
                check(!RunCheckpointStore.TryLoad(path, out _), "Two damaged saves fail safely without crashing");
                RunCheckpointStore.Clear(path);
                check(!File.Exists(path) && !File.Exists(path + ".bak") && !File.Exists(path + ".tmp"), "New-run cleanup removes main, backup and incomplete save");
                check(!RunCheckpointStore.TryLoad(path, out _), "Completed run cannot return from a leftover backup");
            }
            finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        }
        static void Reject(Action<bool, string> check, byte[] bytes, string message)
        {
            bool rejected = false; try { RunCheckpointStore.Decode(bytes); } catch (InvalidDataException) { rejected = true; }
            check(rejected, message);
        }
    }
}
