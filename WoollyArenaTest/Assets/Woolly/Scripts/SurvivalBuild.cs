using System;
using System.Collections.Generic;
using UnityEngine;

namespace WoollyArena
{
    public enum RunStat { MaxHealth, Damage, AttackSpeed, MoveSpeed, Armor, Critical, Regeneration, Harvesting }
    public enum RunWeapon { Revolver, Repeater, Shotgun, Galaxy, Energy, Star }

    public readonly struct StatBonus
    {
        public readonly RunStat Stat;
        public readonly int Amount;
        public StatBonus(RunStat stat, int amount) { Stat = stat; Amount = amount; }
    }
    public sealed class RunItem
    {
        public readonly string Id, Name;
        public readonly int Price;
        public readonly RunWeapon? Weapon;
        public readonly StatBonus[] Bonuses;
        public bool IsWeapon => Weapon.HasValue;
        public RunItem(string id, string name, int price, RunWeapon weapon)
        { Id = id; Name = name; Price = price; Weapon = weapon; Bonuses = Array.Empty<StatBonus>(); }
        public RunItem(string id, string name, int price, params StatBonus[] bonuses)
        { Id = id; Name = name; Price = price; Bonuses = bonuses; }
        public bool IsSpecial => Id=="deadeye"||Id=="capacitor";
        public string SpecialText(int tier)=>Id=="deadeye"?"+"+(12*tier)+"% ateşli silah hasarı":Id=="capacitor"?"+"+(18*tier)+"% güç hasarı":"";
        public string Description(int tier)
        {
            if (IsWeapon)
            {
                switch (Weapon.Value)
                {
                    case RunWeapon.Revolver: return "" + Mathf.RoundToInt(25 * TierScale(tier)) + " hasar · 0,40 sn";
                    case RunWeapon.Repeater: return "" + Mathf.RoundToInt(10 * TierScale(tier)) + " hasar · 0,14 sn";
                    case RunWeapon.Shotgun: return "3 × " + Mathf.RoundToInt(13 * TierScale(tier)) + " hasar · 0,65 sn";
                    case RunWeapon.Galaxy: return "Alan hasarı";
                    case RunWeapon.Energy: return "Zincirleme hasar";
                    default: return "Ağır alan hasarı";
                }
            }
            string result = SpecialText(tier);
            foreach (var bonus in Bonuses) result += (result.Length > 0 ? "\n" : "") + StatText(bonus.Stat, bonus.Amount * tier);
            return result;
        }
        public static float TierScale(int tier) => 1 + (tier - 1) * .6f;
        public static string StatName(RunStat stat) => new[]{"Can", "Hasar", "Saldırı hızı", "Hareket", "Zırh", "Kritik", "İyileşme", "Hasat"}[(int)stat];
        public static string StatText(RunStat stat, int value) => (value >= 0 ? "+" : "") + value + (stat == RunStat.Damage || stat == RunStat.AttackSpeed || stat == RunStat.MoveSpeed || stat == RunStat.Critical ? "% " : " ") + StatName(stat);
    }
    public static class RunCatalog
    {
        public static readonly RunItem[] All = {
            new RunItem("revolver", "Altıpatlar", 12, RunWeapon.Revolver),
            new RunItem("repeater", "Seri Tüfek", 14, RunWeapon.Repeater),
            new RunItem("shotgun", "Çifte", 16, RunWeapon.Shotgun),
            new RunItem("galaxy", "Girdap", 18, RunWeapon.Galaxy),
            new RunItem("energy", "Yıldırım", 18, RunWeapon.Energy),
            new RunItem("star", "Meteor", 18, RunWeapon.Star),
            new RunItem("vest", "Yün Yelek", 10, new StatBonus(RunStat.MaxHealth, 12), new StatBonus(RunStat.Armor, 2), new StatBonus(RunStat.MoveSpeed, -3)),
            new RunItem("boots", "Çöl Botu", 10, new StatBonus(RunStat.MoveSpeed, 8)),
            new RunItem("scope", "Keskin Göz", 12, new StatBonus(RunStat.Critical, 7), new StatBonus(RunStat.AttackSpeed, -3)),
            new RunItem("trigger", "Hızlı Tetik", 12, new StatBonus(RunStat.AttackSpeed, 10), new StatBonus(RunStat.Damage, -3)),
            new RunItem("powder", "Barut", 12, new StatBonus(RunStat.Damage, 10), new StatBonus(RunStat.Armor, -1)),
            new RunItem("canteen", "Matara", 10, new StatBonus(RunStat.Regeneration, 3), new StatBonus(RunStat.MaxHealth, 10)),
            new RunItem("charm", "Bereket", 9, new StatBonus(RunStat.Harvesting, 5)),
            new RunItem("plates", "Zırh Plakası", 12, new StatBonus(RunStat.Armor, 3), new StatBonus(RunStat.MoveSpeed, -5)),
            new RunItem("fang", "Yaratık Dişi", 14, new StatBonus(RunStat.Damage, 10), new StatBonus(RunStat.Critical, 5), new StatBonus(RunStat.MaxHealth, -5)),
            new RunItem("deadeye", "Düellocu Rozeti", 22, new StatBonus(RunStat.AttackSpeed, -3)),
            new RunItem("capacitor", "Prizma Çekirdeği", 24, new StatBonus(RunStat.MaxHealth, -8)),
            new RunItem("sprout", "Filiz", 11, new StatBonus(RunStat.Regeneration, 2), new StatBonus(RunStat.Harvesting, 3))
        };
        public static RunItem Find(string id) => Array.Find(All, x => x.Id == id);
        public static string TierName(int tier) => tier == 1 ? "I" : tier == 2 ? "II" : tier == 3 ? "III" : "IV";
        public static Color TierColor(int tier) => tier == 1 ? new Color(.65f, .78f, .82f) : tier == 2 ? new Color(.27f, .7f, 1) : tier == 3 ? new Color(.77f, .42f, 1) : new Color(1, .73f, .26f);
    }
    public sealed class OwnedGear
    {
        public RunItem Item;
        public int Tier, Paid;
        public OwnedGear(RunItem item, int tier, int paid) { Item = item; Tier = tier; Paid = paid; }
        public int SellValue => Mathf.Max(1, Paid / 2);
    }
    public sealed class ShopOffer
    {
        public RunItem Item;
        public int Tier, Price;
        public bool Locked, Sold;
    }
    public sealed partial class SurvivalBuild
    {
        public const int WeaponLimit = 6;
        public readonly List<OwnedGear> Weapons = new List<OwnedGear>();
        public readonly List<OwnedGear> Items = new List<OwnedGear>();
        public readonly ShopOffer[] Offers = new ShopOffer[4];
        public readonly RunStat[] LevelChoices = new RunStat[4];
        public readonly int[] LevelChoiceTiers = {1,1,1,1};
        public int RewardLevel => Level-PendingLevels+1;
        public int Materials { get; private set; }
        public int Level { get; private set; } = 1;
        public int Experience { get; private set; }
        public int PendingLevels { get; private set; }
        public int Rerolls { get; private set; }
        public int ShopWave { get; private set; }
        public int Revision { get; private set; }
        public int NextLevelXP => RunBalance.ExperienceForLevel(Level);
        public int RerollCost => RunBalance.RerollPrice(ShopWave, Rerolls);
        readonly int[] bonuses = new int[8];
        readonly RunRandom random;
        public int CharacterId { get; private set; } = -1;
        internal void RestoreCharacterProfile(int id) { if(id < -1 || id >= CharacterDefinition.Count) throw new System.IO.InvalidDataException("Invalid character stat profile."); CharacterId=id; }
        public SurvivalBuild(int seed, int characterId = -1)
        {
            RestoreCharacterProfile(characterId);
            random = new RunRandom(seed);
            Weapons.Add(new OwnedGear(RunCatalog.Find("revolver"), 1, 12));
        }
        public int Stat(RunStat stat)
        {
            int amount = CharacterDefinition.BaseStat(CharacterId, stat) + CharacterDefinition.LevelGrowth(CharacterId,stat,Level) + bonuses[(int)stat];
            foreach (var gear in Items) foreach (var bonus in gear.Item.Bonuses) if (bonus.Stat == stat) amount += bonus.Amount * gear.Tier;
            switch (stat)
            {
                case RunStat.MaxHealth: return Mathf.Max(1, amount);
                case RunStat.Armor: return Mathf.Clamp(amount, -20, 50);
                case RunStat.Critical: return Mathf.Clamp(amount, 0, 75);
                case RunStat.AttackSpeed: return Mathf.Clamp(amount, -70, 300);
                case RunStat.MoveSpeed: return Mathf.Clamp(amount, -50, 100);
                case RunStat.Damage: return Mathf.Max(-80, amount);
                default: return Mathf.Max(0, amount);
            }
        }
        public float WeaponDamageMultiplier(RunWeapon weapon){
            float bonus=0;foreach(var gear in Items){if(gear.Item.Id=="deadeye"&&(int)weapon<3)bonus+=.12f*gear.Tier;if(gear.Item.Id=="capacitor"&&(int)weapon>=3)bonus+=.18f*gear.Tier;}return 1+bonus;
        }
        public float DamageMultiplier => 1 + Stat(RunStat.Damage) / 100f;
        public float AttackMultiplier => 1 + Stat(RunStat.AttackSpeed) / 100f;
        public float MoveMultiplier => 1 + Stat(RunStat.MoveSpeed) / 100f;
        public static float ArmorMultiplier(int armor) => armor >= 0 ? 1 / (1 + armor * .06f) : 1 - armor * .06f;
        public void AddMaterials(int amount) { Materials += Mathf.Max(0, amount); }
        public void AddExperience(int amount)
        {
            Experience += Mathf.Max(0, amount);
            while (Experience >= NextLevelXP) { Experience -= NextLevelXP; Level++; PendingLevels++; }
        }
        public void RollLevelChoices()
        {
            var bag = new List<RunStat>((RunStat[])Enum.GetValues(typeof(RunStat)));
            for (int i = 0; i < LevelChoices.Length; i++) { int n = random.Next(bag.Count); LevelChoices[i] = bag[n]; bag.RemoveAt(n);
                int roll=random.Next(100), level=RewardLevel;
                int minimum=level>=25&&level%5==0?4:level>=10&&level%5==0?3:level==5?2:1;
                int tier=level>=12&&roll<3?4:level>=6&&roll<14?3:level>=3&&roll<35?2:1;
                LevelChoiceTiers[i]=Math.Max(minimum,tier); }
        }
        public static int LevelBonus(RunStat stat) => stat == RunStat.MaxHealth ? 8 : stat == RunStat.Armor ? 1 : stat == RunStat.Regeneration ? 1 : stat == RunStat.Harvesting ? 3 : stat == RunStat.Critical ? 3 : stat == RunStat.MoveSpeed ? 4 : 5;
        public static int LevelBonus(RunStat stat,int tier){
            tier=Mathf.Clamp(tier,1,4);
            if(stat==RunStat.MaxHealth)return 4+4*tier;
            if(stat==RunStat.Armor||stat==RunStat.Regeneration)return tier;
            if(stat==RunStat.Harvesting||stat==RunStat.Critical)return 1+2*tier;
            if(stat==RunStat.MoveSpeed)return 2+2*tier;
            return new[]{5,8,12,16}[tier-1];
        }
        public bool RerollLevels(){if(PendingLevels<=0||Materials<RerollCost)return false;Materials-=RerollCost;Rerolls++;RollLevelChoices();return true;}
        public bool ChooseLevel(int index)
        {
            if (PendingLevels <= 0 || index < 0 || index >= LevelChoices.Length) return false;
            var stat = LevelChoices[index]; bonuses[(int)stat] += LevelBonus(stat,LevelChoiceTiers[index]); PendingLevels--; Revision++;
            if (PendingLevels > 0) RollLevelChoices(); else Rerolls=0; return true;
        }
        public void OpenShop(int wave)
        {
            ShopWave = wave; Rerolls = 0;
            RollOffers();
        }
        void RollOffers()
        {
            var used = new HashSet<string>();
            foreach (var offer in Offers) if (offer != null && offer.Locked && !offer.Sold) used.Add(offer.Item.Id);
            for (int i = 0; i < Offers.Length; i++)
            {
                if (Offers[i] != null && Offers[i].Locked && !Offers[i].Sold) continue;
                var candidates = new List<RunItem>();
                foreach (var item in RunCatalog.All) if (!used.Contains(item.Id) && (!item.IsSpecial || ShopWave>=4) && (i >= (ShopWave<=2?2:1) || item.IsWeapon)) candidates.Add(item);
                // Owned weapons are more likely, making upgrades deliberate while keeping unique offers.
                var weighted=new List<RunItem>();foreach(var candidate in candidates){weighted.Add(candidate);if(candidate.IsWeapon&&Weapons.Exists(w=>w.Item==candidate)) {weighted.Add(candidate);weighted.Add(candidate);}}
                var chosen = weighted[random.Next(weighted.Count)]; used.Add(chosen.Id);
                int tier = 1; int roll = random.Next(100);
                if (ShopWave >= 3 && roll < Mathf.Min(55, ShopWave * 5)) tier = 2;
                if (ShopWave >= 7 && roll < 12) tier = 3;
                if (ShopWave >= 12 && roll < 5) tier = 4;
                Offers[i] = new ShopOffer { Item = chosen, Tier = tier, Price = RunBalance.Price(chosen.Price, tier, ShopWave) };
            }
        }
        public bool Reroll()
        {
            if (Materials < RerollCost || Array.TrueForAll(Offers, x => x != null && x.Locked && !x.Sold)) return false;
            Materials -= RerollCost; Rerolls++; RollOffers(); return true;
        }
        public bool ToggleLock(int index)
        {
            if (index < 0 || index >= Offers.Length || Offers[index] == null || Offers[index].Sold) return false;
            Offers[index].Locked = !Offers[index].Locked; return true;
        }
        public bool CanBuy(int index)
        {
            if (index < 0 || index >= Offers.Length) return false;
            var offer = Offers[index];
            if (offer == null || offer.Sold || Materials < offer.Price) return false;
            return !offer.Item.IsWeapon || Weapons.Count < WeaponLimit || FindMatch(offer.Item, offer.Tier) >= 0;
        }
        int FindMatch(RunItem item, int tier, int except = -1)
        {
            if (tier >= 4) return -1;
            return Weapons.FindIndex(x => x.Item == item && x.Tier == tier && Weapons.IndexOf(x) != except);
        }
        public bool Buy(int index)
        {
            if (!CanBuy(index)) return false;
            var offer = Offers[index]; Materials -= offer.Price; offer.Sold = true; offer.Locked = false;
            if (!offer.Item.IsWeapon) Items.Add(new OwnedGear(offer.Item, offer.Tier, offer.Price));
            else if (Weapons.Count < WeaponLimit) Weapons.Add(new OwnedGear(offer.Item, offer.Tier, offer.Price));
            else { var target = Weapons[FindMatch(offer.Item, offer.Tier)]; target.Tier++; target.Paid += offer.Price; }
            Revision++; return true;
        }
        public bool CanCombine(int index) => index >= 0 && index < Weapons.Count && FindMatch(Weapons[index].Item, Weapons[index].Tier, index) >= 0;
        public bool Combine(int index)
        {
            if (!CanCombine(index)) return false;
            var selected = Weapons[index]; int other = FindMatch(selected.Item, selected.Tier, index);
            selected.Tier++; selected.Paid += Weapons[other].Paid; Weapons.RemoveAt(other); Revision++; return true;
        }
        public bool SellWeapon(int index)
        {
            if (index < 0 || index >= Weapons.Count || Weapons.Count <= 1) return false;
            Materials += Weapons[index].SellValue; Weapons.RemoveAt(index); Revision++; return true;
        }
        public bool SellItem(int index)
        {
            if (index < 0 || index >= Items.Count) return false;
            Materials += Items[index].SellValue; Items.RemoveAt(index); Revision++; return true;
        }
        public int PowerLevel(int index)
        {
            int result = 0;
            foreach (var weapon in Weapons) if ((int)weapon.Item.Weapon.Value == index + 3) result += weapon.Tier;
            return result;
        }
    }
}
