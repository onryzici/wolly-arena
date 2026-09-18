using System;
using System.IO;
using System.Security.Cryptography;

namespace WoollyArena
{
    // Stable, independent RNG lets locked offers and future rerolls survive reloads.
    public sealed class RunRandom
    {
        public uint State;
        public RunRandom(int seed) { State = unchecked((uint)seed); if (State == 0) State = 1; }
        public int Next(int maximum)
        {
            if (maximum <= 0) throw new ArgumentOutOfRangeException(nameof(maximum));
            uint x = State; x ^= x << 13; x ^= x >> 17; x ^= x << 5; State = x;
            return (int)(x % (uint)maximum);
        }
    }
    public sealed class RunCheckpoint
    {
        public SurvivalBuild Build;
        public int Wave, Phase, Health, Kills, Harvest, Biome, Character;
        public void Validate()
        {
            if (Character<0 || Character>=CharacterDefinition.Count || Biome<0 || Biome>1 || Build == null || Wave < 1 || Wave > 20 || (Phase != 0 && Phase != 1 && Phase != 4) || Health < 1 || Health > Build.Stat(RunStat.MaxHealth) || Kills < 0 || Kills > 1000000 || Harvest < 0 || Harvest > 1000000)
                throw new InvalidDataException("Invalid run checkpoint.");
            if(Array.Exists(Build.LevelChoiceTiers,t=>t<1||t>4))throw new InvalidDataException("Invalid upgrade rarity.");
            if (Build.CharacterId >= 0 && Build.CharacterId != Character) throw new InvalidDataException("Character stat profile mismatch.");
            if (Wave == 20 && Phase != 0) throw new InvalidDataException("Final wave has no shop checkpoint.");
            if ((Phase == 4) != (Build.PendingLevels > 0) && Phase != 0) throw new InvalidDataException("Invalid level choice phase.");
            if (Phase != 0 && Array.Exists(Build.Offers, x => x == null)) throw new InvalidDataException("Missing shop offers.");
        }
    }
    public static class RunCheckpointStore
    {
        const int Version = 5, Limit = 1048576;
        public static byte[] Encode(RunCheckpoint checkpoint)
        {
            checkpoint.Validate();
            using (var payload = new MemoryStream())
            {
                using (var writer = new BinaryWriter(payload, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(0x574F4F4C); writer.Write(Version);
                    writer.Write(checkpoint.Wave); writer.Write(checkpoint.Phase); writer.Write(checkpoint.Health); writer.Write(checkpoint.Kills); writer.Write(checkpoint.Harvest);
                    checkpoint.Build.Write(writer);writer.Write(checkpoint.Biome);writer.Write(checkpoint.Character);writer.Write(checkpoint.Build.CharacterId);foreach(int tier in checkpoint.Build.LevelChoiceTiers)writer.Write(tier);
                }
                byte[] data = payload.ToArray();
                using (var hash = SHA256.Create())
                {
                    byte[] checksum = hash.ComputeHash(data); var result = new byte[data.Length + checksum.Length];
                    Buffer.BlockCopy(data, 0, result, 0, data.Length); Buffer.BlockCopy(checksum, 0, result, data.Length, checksum.Length); return result;
                }
            }
        }
        public static RunCheckpoint Decode(byte[] data)
        {
            if (data == null || data.Length < 64 || data.Length > Limit) throw new InvalidDataException("Invalid save size.");
            using (var hash = SHA256.Create())
            {
                byte[] checksum = hash.ComputeHash(data, 0, data.Length - 32);
                for (int i = 0; i < 32; i++) if (checksum[i] != data[data.Length - 32 + i]) throw new InvalidDataException("Save checksum mismatch.");
            }
            using (var stream = new MemoryStream(data, 0, data.Length - 32))
            using (var reader = new BinaryReader(stream))
            {
                if (reader.ReadInt32() != 0x574F4F4C) throw new InvalidDataException("Unsupported save signature.");
                int version=reader.ReadInt32();if(version<1||version>Version)throw new InvalidDataException("Unsupported save version.");
                var checkpoint = new RunCheckpoint { Wave = reader.ReadInt32(), Phase = reader.ReadInt32(), Health = reader.ReadInt32(), Kills = reader.ReadInt32(), Harvest = reader.ReadInt32(), Build = SurvivalBuild.Read(reader) };
                checkpoint.Biome=version>=2?reader.ReadInt32():0;checkpoint.Character=version>=3?reader.ReadInt32():0;
                checkpoint.Build.RestoreCharacterProfile(version>=4?reader.ReadInt32():-1);
                if(version>=5)for(int i=0;i<4;i++){int tier=reader.ReadInt32();if(tier<1||tier>4)throw new InvalidDataException("Invalid upgrade rarity.");checkpoint.Build.LevelChoiceTiers[i]=tier;}
                if (stream.Position != stream.Length) throw new InvalidDataException("Unexpected save data.");
                checkpoint.Validate(); return checkpoint;
            }
        }
        public static void Save(string path, RunCheckpoint checkpoint)
        {
            byte[] data = Encode(checkpoint); Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            using (var file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)) { file.Write(data, 0, data.Length); file.Flush(true); }
            // A corrupt primary must not overwrite the healthy backup used for recovery.
            if (File.Exists(path)) File.Replace(temporary, path, TryRead(path, out _) ? path + ".bak" : null);
            else File.Move(temporary, path);
        }
        public static bool TryLoad(string path, out RunCheckpoint checkpoint)
        {
            checkpoint = null;
            return TryRead(path, out checkpoint) || TryRead(path + ".bak", out checkpoint);
        }
        static bool TryRead(string path, out RunCheckpoint checkpoint)
        {
            checkpoint = null;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > Limit) return false;
                checkpoint = Decode(File.ReadAllBytes(path)); return true;
            }
            catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException || e is CryptographicException) { }
            return false;
        }
        public static void Clear(string path)
        {
            // Delete backup first: interruption cannot resurrect an older completed run.
            foreach (string suffix in new[] { ".bak", ".tmp", "" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);
        }
    }
    public sealed partial class SurvivalBuild
    {
        internal void Write(BinaryWriter writer)
        {
            writer.Write(random.State); writer.Write(Materials); writer.Write(Level); writer.Write(Experience); writer.Write(PendingLevels); writer.Write(Rerolls); writer.Write(ShopWave);
            foreach (int bonus in bonuses) writer.Write(bonus);
            foreach (var choice in LevelChoices) writer.Write((int)choice);
            WriteGear(writer, Weapons); WriteGear(writer, Items);
            foreach (var offer in Offers)
            {
                writer.Write(offer != null); if (offer == null) continue;
                writer.Write(offer.Item.Id); writer.Write(offer.Tier); writer.Write(offer.Price); writer.Write(offer.Locked); writer.Write(offer.Sold);
            }
        }
        static void WriteGear(BinaryWriter writer, System.Collections.Generic.List<OwnedGear> gear)
        {
            writer.Write(gear.Count); foreach (var item in gear) { writer.Write(item.Item.Id); writer.Write(item.Tier); writer.Write(item.Paid); }
        }
        static int Bounded(BinaryReader reader, int low, int high)
        {
            int value = reader.ReadInt32(); if (value < low || value > high) throw new InvalidDataException("Save value out of range."); return value;
        }
        internal static SurvivalBuild Read(BinaryReader reader)
        {
            var build = new SurvivalBuild(1); build.random.State = reader.ReadUInt32(); if (build.random.State == 0) throw new InvalidDataException("Invalid RNG state.");
            build.Materials = Bounded(reader, 0, 1000000); build.Level = Bounded(reader, 1, 1000); build.Experience = Bounded(reader, 0, build.NextLevelXP - 1);
            build.PendingLevels = Bounded(reader, 0, build.Level - 1); build.Rerolls = Bounded(reader, 0, 10000); build.ShopWave = Bounded(reader, 0, 20);
            for (int i = 0; i < build.bonuses.Length; i++) build.bonuses[i] = Bounded(reader, 0, 100000);
            for (int i = 0; i < 4; i++) build.LevelChoices[i] = (RunStat)Bounded(reader, 0, 7);
            ReadGear(reader, build.Weapons, true); ReadGear(reader, build.Items, false);
            for (int i = 0; i < 4; i++)
            {
                if (!reader.ReadBoolean()) continue;
                var item = RunCatalog.Find(reader.ReadString()); if (item == null) throw new InvalidDataException("Unknown offer.");
                build.Offers[i] = new ShopOffer { Item = item, Tier = Bounded(reader, 1, 4), Price = Bounded(reader, 1, 1000000), Locked = reader.ReadBoolean(), Sold = reader.ReadBoolean() };
            }
            return build;
        }
        static void ReadGear(BinaryReader reader, System.Collections.Generic.List<OwnedGear> list, bool weapons)
        {
            int count = Bounded(reader, weapons ? 1 : 0, weapons ? 6 : 10000); list.Clear();
            for (int i = 0; i < count; i++)
            {
                var item = RunCatalog.Find(reader.ReadString()); if (item == null || item.IsWeapon != weapons) throw new InvalidDataException("Unknown equipment.");
                list.Add(new OwnedGear(item, Bounded(reader, 1, 4), Bounded(reader, 0, 1000000)));
            }
        }
    }
}
