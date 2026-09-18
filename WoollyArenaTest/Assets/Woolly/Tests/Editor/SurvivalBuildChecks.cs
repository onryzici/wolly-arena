using System;
using System.Linq;
using UnityEngine;
namespace WoollyArena.Editor
{
    public static class SurvivalBuildChecks
    {
        static void Offer(SurvivalBuild build, string id, int index = 0, int tier = 1, int price = 10)
        { build.Offers[index] = new ShopOffer { Item = RunCatalog.Find(id), Tier = tier, Price = price }; }
        public static void Run(Action<bool, string> check)
        {
            var b = new SurvivalBuild(42);
            check(b.Weapons.Count == 1 && b.Stat(RunStat.MaxHealth) == 100 && b.Materials == 0, "Fresh build has one weapon, base stats and empty wallet");
            check(!b.SellWeapon(0), "Last weapon cannot be recycled");
            b.AddExperience(b.NextLevelXP); check(b.Level == 2 && b.PendingLevels == 1 && b.Experience == 0, "XP crossing grants a pending level");
            b.RollLevelChoices(); check(b.LevelChoices.Distinct().Count() == 4, "Level choices are distinct");
            var stat = b.LevelChoices[0]; int before = b.Stat(stat);
            check(!b.ChooseLevel(-1) && b.ChooseLevel(0) && b.Stat(stat) == before + SurvivalBuild.LevelBonus(stat) && !b.ChooseLevel(0), "Level bonus is free and applies exactly once");
            b.AddExperience(100); check(b.PendingLevels > 1, "Multiple level thresholds queue separate choices");
            b.OpenShop(1); Offer(b, "vest");
            check(!b.Buy(0) && b.Materials == 0, "Unaffordable purchase changes nothing");
            b.AddMaterials(1000); int wallet = b.Materials; int hp = b.Stat(RunStat.MaxHealth), speed = b.Stat(RunStat.MoveSpeed);
            check(b.Buy(0) && b.Materials == wallet - 10 && b.Items.Count == 1, "Passive item charges exact price and enters inventory");
            check(b.Stat(RunStat.MaxHealth) == hp + 12 && b.Stat(RunStat.MoveSpeed) == speed - 3, "Both positive and negative item stats apply");
            check(!b.Buy(0) && b.Items.Count == 1, "Sold offers cannot be purchased twice");
            check(b.SellItem(0) && b.Stat(RunStat.MaxHealth) == hp && b.Stat(RunStat.MoveSpeed) == speed && b.Materials == wallet - 5, "Recycling removes modifiers and refunds half the price");
            Offer(b, "revolver"); b.Buy(0);
            check(b.CanCombine(0) && b.Combine(0) && b.Weapons.Count == 1 && b.Weapons[0].Tier == 2, "Matching weapons combine into the next tier and free a slot");
            Offer(b, "revolver", tier: 1); b.Buy(0);
            check(!b.CanCombine(0), "Different tiers cannot combine");
            foreach (var id in new[] { "repeater", "shotgun", "galaxy", "energy" }) { Offer(b, id); check(b.Buy(0), "Weapon acquisition: " + id); }
            check(b.Weapons.Count == 6, "Six weapons fill the loadout");
            Offer(b, "star"); wallet = b.Materials;
            check(!b.Buy(0) && b.Materials == wallet && b.Weapons.Count == 6, "Full loadout rejects unmatched weapon without charging");
            Offer(b, "repeater"); check(b.Buy(0) && b.Weapons.Count == 6 && b.Weapons.Any(x => x.Item.Id == "repeater" && x.Tier == 2), "Full loadout purchases auto-combine a matching weapon");
            int refund = b.Weapons[0].SellValue; wallet = b.Materials;
            check(b.SellWeapon(0) && b.Materials == wallet + refund && b.Weapons.Count == 5, "Weapon recycling refunds combined investment correctly");
            b.OpenShop(3); var locked = b.Offers[1]; b.ToggleLock(1); wallet = b.Materials; int cost = b.RerollCost;
            check(b.Reroll() && b.Materials == wallet - cost && ReferenceEquals(locked, b.Offers[1]) && b.RerollCost > cost, "Reroll charges increasing cost and preserves locked offers");
            b.OpenShop(4); check(ReferenceEquals(locked, b.Offers[1]) && b.Rerolls == 0, "Locks and original prices carry to the next shop");
            for (int i = 0; i < 4; i++) b.Offers[i].Locked = true;
            wallet = b.Materials; check(!b.Reroll() && b.Materials == wallet, "All-locked shop cannot waste reroll currency");
            b.Offers[0].Locked = false;
            check(!b.Buy(-1) && !b.Buy(4) && !b.SellItem(99) && !b.Combine(99), "Invalid inventory indices are rejected");
            var cap = new SurvivalBuild(1); cap.AddMaterials(1000); Offer(cap, "revolver", tier: 4); cap.Buy(0); Offer(cap, "revolver", tier: 4); cap.Buy(0);
            check(!cap.CanCombine(1), "Legendary tier cannot combine beyond the cap");
            var empty = new SurvivalBuild(3); empty.OpenShop(1); check(!empty.Reroll() && empty.Materials == 0, "Insufficient funds cannot reroll");
            check(SurvivalBuild.ArmorMultiplier(10) < 1 && SurvivalBuild.ArmorMultiplier(-2) > 1, "Positive armor reduces damage; negative armor increases it");
            for (int seed = 0; seed < 12; seed++)
            {
                var sample = new SurvivalBuild(seed); sample.OpenShop(1);
                check(sample.Offers.Select(x => x.Item.Id).Distinct().Count() == 4 && sample.Offers.All(x => x.Tier == 1) && sample.Offers[0].Item.IsWeapon, "Seed " + seed + ": unique starter offers include a weapon");
            }
                }
    }
}
