using System;
using UnityEngine;

namespace WoollyArena
{
    public enum WeaponFamily { Gunslinger, Brawler, Elemental, Demolition }
    public enum WeaponMotion { Recoil, Sweep, Thrust, Charge, Spray, Lob }

    // One source for combat, shop descriptions, animation timing and balance measurements.
    public readonly struct WeaponProfile
    {
        public readonly WeaponFamily Family;
        public readonly WeaponMotion Motion;
        public readonly float Damage, Interval, Range, Windup, Radius;
        public WeaponProfile(WeaponFamily family, WeaponMotion motion, float damage, float interval, float range, float windup = 0, float radius = 0)
        { Family = family; Motion = motion; Damage = damage; Interval = interval; Range = range; Windup = windup; Radius = radius; }
    }

    public static class WeaponArsenal
    {
        public static bool IsLegacyPower(RunWeapon weapon) => weapon == RunWeapon.Galaxy || weapon == RunWeapon.Energy || weapon == RunWeapon.Star;
        public static bool IsFirearm(RunWeapon weapon) => Profile(weapon).Family == WeaponFamily.Gunslinger || weapon == RunWeapon.Grenade;
        public static bool IsMagic(RunWeapon weapon) => IsLegacyPower(weapon) || weapon == RunWeapon.Frost;
        public static WeaponProfile Profile(RunWeapon weapon)
        {
            switch (weapon)
            {
                case RunWeapon.Revolver: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Recoil, 25, .4f, 14);
                case RunWeapon.Repeater: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Recoil, 8, .17f, 10);
                case RunWeapon.Shotgun: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Recoil, 13, .65f, 7);
                case RunWeapon.Railgun: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Charge, 66, 1.5f, 16, .3f);
                case RunWeapon.Blade: return new WeaponProfile(WeaponFamily.Brawler, WeaponMotion.Sweep, 38, .78f, 2.5f, .16f);
                case RunWeapon.Spear: return new WeaponProfile(WeaponFamily.Brawler, WeaponMotion.Thrust, 48, 1.05f, 3.8f, .22f);
                case RunWeapon.Flame: return new WeaponProfile(WeaponFamily.Elemental, WeaponMotion.Spray, 6, .3f, 4.5f);
                case RunWeapon.Frost: return new WeaponProfile(WeaponFamily.Elemental, WeaponMotion.Charge, 18, 1.9f, 9, .24f, 1.65f);
                case RunWeapon.Grenade: return new WeaponProfile(WeaponFamily.Demolition, WeaponMotion.Lob, 64, 2.2f, 10, .65f, 2.1f);
                case RunWeapon.Axe: return new WeaponProfile(WeaponFamily.Brawler, WeaponMotion.Sweep, 60, 1.2f, 2.3f, .25f);
                case RunWeapon.Hammer: return new WeaponProfile(WeaponFamily.Brawler, WeaponMotion.Thrust, 85, 1.8f, 2.3f, .38f, 2.3f);
                case RunWeapon.Crossbow: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Thrust, 40, .95f, 12, .12f);
                case RunWeapon.BurstRifle: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Recoil, 11, .75f, 11);
                case RunWeapon.Ricochet: return new WeaponProfile(WeaponFamily.Gunslinger, WeaponMotion.Recoil, 23, .7f, 10);
                case RunWeapon.Boomerang: return new WeaponProfile(WeaponFamily.Brawler, WeaponMotion.Sweep, 24, 1.6f, 8, .35f);
                case RunWeapon.Saw: return new WeaponProfile(WeaponFamily.Brawler, WeaponMotion.Sweep, 9, .22f, 1.8f);
                case RunWeapon.Mortar: return new WeaponProfile(WeaponFamily.Demolition, WeaponMotion.Lob, 112, 3.4f, 13, .9f, 2.8f);
                case RunWeapon.Galaxy: return new WeaponProfile(WeaponFamily.Demolition, WeaponMotion.Charge, 45, 3.2f, 14, .28f, 2.7f);
                case RunWeapon.Energy: return new WeaponProfile(WeaponFamily.Elemental, WeaponMotion.Charge, 27.5f, 2, 14, .08f);
                case RunWeapon.Star: return new WeaponProfile(WeaponFamily.Demolition, WeaponMotion.Lob, 75, 2.8f, 14, .28f, 1.25f);
                default: throw new ArgumentOutOfRangeException(nameof(weapon));
            }
        }
        public static string FamilyName(WeaponFamily family) => new[] { "NİŞANCI", "YAKIN DÖVÜŞ", "ELEMENT", "PATLAYICI" }[(int)family];
        public static Color ColorFor(RunWeapon weapon)
        {
            switch (weapon)
            {
                case RunWeapon.Blade: case RunWeapon.Spear: return new Color(.55f, 1, .7f);
                case RunWeapon.Railgun: return new Color(.65f, .45f, 1);
                case RunWeapon.Flame: return new Color(1, .36f, .08f);
                case RunWeapon.Frost: return new Color(.3f, .85f, 1);
                case RunWeapon.Grenade: return new Color(1, .72f, .18f);
                case RunWeapon.Axe: case RunWeapon.Hammer: return new Color(1,.59f,.28f);
                case RunWeapon.Crossbow: case RunWeapon.Boomerang: return new Color(.5f,1,.75f);
                case RunWeapon.BurstRifle: return new Color(1,.85f,.4f);
                case RunWeapon.Ricochet: return new Color(1,.42f,.7f);
                case RunWeapon.Saw: return new Color(.8f,.95f,1);
                case RunWeapon.Mortar: return new Color(1,.42f,.13f);
                default: return new Color(1, .8f, .4f);
            }
        }
        public static string Description(RunWeapon weapon, int tier)
        {
            var p = Profile(weapon);
            string damage = Mathf.RoundToInt(p.Damage * RunItem.TierScale(tier)).ToString();
            string effect;
            switch (weapon)
            {
                case RunWeapon.Blade: effect = "120° süpürme · " + damage + " hasar"; break;
                case RunWeapon.Spear: effect = "3,8 m saplama · " + damage + " hasar"; break;
                case RunWeapon.Railgun: effect = "3 hedef deler · " + damage + " hasar"; break;
                case RunWeapon.Flame: effect = "Koni + 2 sn yanma"; break;
                case RunWeapon.Frost: effect = "%30 yavaşlatır · alan hasarı"; break;
                case RunWeapon.Grenade: effect = "Gecikmeli patlama · " + damage + " hasar"; break;
                case RunWeapon.Shotgun: effect = "3 × " + damage + " hasar · 0,65 sn"; break;
                case RunWeapon.Revolver: effect = damage + " hasar · 0,40 sn"; break;
                case RunWeapon.Repeater: effect = damage + " hasar · 18 atış / 1,35 sn soğuma"; break;
                case RunWeapon.Axe: effect = "180° yarma · " + damage + " hasar"; break;
                case RunWeapon.Hammer: effect = "Çevresel şok · " + damage + " hasar"; break;
                case RunWeapon.Crossbow: effect = "2 hedef deler · " + damage + " hasar"; break;
                case RunWeapon.BurstRifle: effect = "3'lü seri · 3 × " + damage + " hasar"; break;
                case RunWeapon.Ricochet: effect = "3 hedefe seker · azalan hasar"; break;
                case RunWeapon.Boomerang: effect = "Gidiş + dönüş · 2 × " + damage; break;
                case RunWeapon.Saw: effect = "Çevresel kesim · kısa menzil"; break;
                case RunWeapon.Mortar: effect = "0,9 sn düşüş · " + damage + " hasar"; break;
                case RunWeapon.Galaxy: effect = "Geniş alan hasarı"; break;
                case RunWeapon.Energy: effect = "Zincirleme hasar"; break;
                default: effect = "Ağır alan hasarı"; break;
            }
            return FamilyName(p.Family) + "\n" + effect;
        }
    }

    public sealed partial class SurvivalBuild
    {
        public int FamilyCount(WeaponFamily family)
        {
            int count = 0;
            foreach (var gear in Weapons) if (WeaponArsenal.Profile(gear.Item.Weapon.Value).Family == family) count++;
            return count;
        }
        public int FamilyRank(WeaponFamily family) => Math.Min(2, FamilyCount(family) / 2);
        int ItemTiers(string id) { int sum = 0; foreach (var gear in Items) if (gear.Item.Id == id) sum += gear.Tier; return sum; }
        public int WeaponCriticalBonus(RunWeapon weapon) => WeaponArsenal.Profile(weapon).Family == WeaponFamily.Gunslinger ? 4 * FamilyRank(WeaponFamily.Gunslinger) : 0;
        public float WeaponAttackMultiplier(RunWeapon weapon) => WeaponArsenal.Profile(weapon).Family == WeaponFamily.Brawler ? 1 + .08f * FamilyRank(WeaponFamily.Brawler) : 1;
        public int FamilyArmor => 2 * FamilyRank(WeaponFamily.Brawler);
        public float BurnMultiplier => 1 + .15f * FamilyRank(WeaponFamily.Elemental) + .20f * ItemTiers("cinder");
        public float ChillStrength => Math.Min(.45f, .30f + .04f * FamilyRank(WeaponFamily.Elemental) + .04f * ItemTiers("coolant"));
        public float AreaMultiplier => 1 + Math.Min(.35f, .10f * FamilyRank(WeaponFamily.Demolition) + .06f * ItemTiers("payload"));
        public string FamilySummary()
        {
            string result = "";
            foreach (WeaponFamily family in Enum.GetValues(typeof(WeaponFamily)))
            {
                int count = FamilyCount(family); if (count == 0) continue;
                result += (result.Length > 0 ? "  ·  " : "") + WeaponArsenal.FamilyName(family) + " " + count + "/" + (count < 2 ? 2 : 4);
            }
            return result;
        }
        public string FamilyDescription(WeaponFamily family)
        {
            int rank = FamilyRank(family);
            int shown=Math.Max(1,rank);
            string bonus = family == WeaponFamily.Gunslinger ? "+%"+(4*shown)+" kritik" : family == WeaponFamily.Brawler ? "+"+(2*shown)+" zırh, +%"+(8*shown)+" yakın hız" : family == WeaponFamily.Elemental ? "+%"+(15*shown)+" yanma, +%"+(4*shown)+" yavaşlatma" : "+%"+(10*shown)+" alan yarıçapı";
            return WeaponArsenal.FamilyName(family)+" "+FamilyCount(family)+"/"+(rank<1?2:4)+" · "+(rank==0?"2 silahta: ":"")+bonus;
        }
    }
}
