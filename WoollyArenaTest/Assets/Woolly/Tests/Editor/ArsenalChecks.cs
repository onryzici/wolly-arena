using System;
using System.Linq;
namespace WoollyArena.Editor
{
    public static class ArsenalChecks
    {
        public static void Run(Action<bool,string> check)
        {
            check(RunCatalog.All.Where(i=>i.IsWeapon).Count()==20,"Twenty weapons have unique stable catalog entries");
            check(RunCatalog.All.Select(i=>i.Id).Distinct().Count()==RunCatalog.All.Length,"Catalog IDs are unique");
            var b=new SurvivalBuild(17);b.Weapons.Clear();
            foreach(var id in new[]{"blade","spear","railgun","flame","frost","grenade"})b.Weapons.Add(new OwnedGear(RunCatalog.Find(id),2,25));
            check(b.FamilyRank(WeaponFamily.Brawler)==1&&b.FamilyArmor==2,"Two melee slots grant one armor synergy");
            check(b.FamilyRank(WeaponFamily.Elemental)==1&&Math.Abs(b.BurnMultiplier-1.15f)<.001f,"Two elemental slots grant bounded status synergy");
            check(b.PowerLevel(0)==0&&b.PowerLevel(1)==0&&b.PowerLevel(2)==0,"New enum values cannot accidentally activate legacy powers");
            var checkpoint=new RunCheckpoint{Build=b,Wave=8,Phase=0,Health=100};
            var loaded=RunCheckpointStore.Decode(RunCheckpointStore.Encode(checkpoint)).Build;
            check(loaded.Weapons.Select(w=>w.Item.Id).SequenceEqual(b.Weapons.Select(w=>w.Item.Id))&&loaded.Weapons.All(w=>w.Tier==2),"All six new weapons survive real checkpoint serialization");
            check(loaded.FamilyArmor==b.FamilyArmor&&loaded.ChillStrength==b.ChillStrength,"Build family effects recompute correctly after loading");
            b.Items.Add(new OwnedGear(RunCatalog.Find("capacitor"),1,20));
            check(b.WeaponDamageMultiplier(RunWeapon.Frost)>1&&b.WeaponDamageMultiplier(RunWeapon.Blade)==1&&b.WeaponDamageMultiplier(RunWeapon.Grenade)==1,"Prism core includes frost, excludes melee and explosives");
            b.Items.Clear();b.Items.Add(new OwnedGear(RunCatalog.Find("deadeye"),1,20));
            check(b.WeaponDamageMultiplier(RunWeapon.Railgun)>1&&b.WeaponDamageMultiplier(RunWeapon.Flame)==1,"Firearm specialization includes rails without buffing flame");
            b.Items.Clear();b.Items.Add(new OwnedGear(RunCatalog.Find("brawler"),2,20));
            check(Math.Abs(b.WeaponDamageMultiplier(RunWeapon.Spear)-1.36f)<.001f&&b.WeaponDamageMultiplier(RunWeapon.Railgun)==1,"Melee specialization affects only melee weapons");
            b.SellItem(0);check(b.WeaponDamageMultiplier(RunWeapon.Spear)==1,"Selling specialization removes its damage bonus");
            b.Weapons.Clear();for(int i=0;i<6;i++)b.Weapons.Add(new OwnedGear(RunCatalog.Find("blade"),4,20));
            check(b.FamilyRank(WeaponFamily.Brawler)==2&&b.FamilyArmor==4,"Six tier-IV copies cannot exceed the four-slot family bonus");
            b.SellWeapon(5);b.SellWeapon(4);b.SellWeapon(3);
            check(b.FamilyRank(WeaponFamily.Brawler)==1&&b.Stat(RunStat.Armor)==2,"Selling below four slots immediately removes the higher synergy");
            b.Weapons.Clear();b.Weapons.Add(new OwnedGear(RunCatalog.Find("frost"),1,20));
            for(int i=0;i<20;i++){b.Items.Add(new OwnedGear(RunCatalog.Find("coolant"),4,20));b.Items.Add(new OwnedGear(RunCatalog.Find("payload"),4,20));}
            check(b.ChillStrength<=.45f&&b.AreaMultiplier<=1.35f,"Stacked control and blast items respect their hard caps");
            bool available=true;
            foreach(string id in new[]{"blade","spear","railgun","flame","frost","grenade"}){
                bool seen=false;for(int seed=1;seed<=300;seed++){var shop=new SurvivalBuild(seed);shop.OpenShop(1);seen|=shop.Offers.Any(o=>o.Item.Id==id);}available&=seen;
            }
            check(available,"Every new weapon can appear in a normal starter shop");
            check(Math.Abs(RunBalance.AttackMultiplier(60)-1.6f)<.001f&&Math.Abs(RunBalance.AttackMultiplier(300)-2.44f)<.001f,"Attack speed has diminishing returns above sixty percent");
            check(RunBalance.ArmoredDamage(8,20,true,false)==1&&RunBalance.ArmoredDamage(66,20,true,false)==59,"Boss plating counters weak repeated hits while heavy attacks retain impact");
            check(RunBalance.StaggerCooldown(20)>MeleeStrike.Duration+MeleeStrike.Windup,"Late enemies have time to finish a melee strike between staggers");
            var heat=new RepeaterHeat();for(int i=0;i<18;i++)check(heat.Fire(i*.05f),"Repeater accepts shot "+(i+1)+" before venting");
            check(!heat.Fire(.9f)&&!heat.Ready(2.19f)&&heat.Ready(2.21f),"Repeater vent duration cannot be bypassed with attack speed");
            check(heat.Fire(2.21f)&&heat.Fired==1,"Repeater resumes with a fresh heat budget after venting");
            check(WaveDifficulty.BossHealth(5)==1800&&WaveDifficulty.BossHealth(20)==38550,"Boss durability preserves the first milestone and scales to final builds");
            var ailments=new CombatAilments();ailments.Ignite(0,5);ailments.Ignite(.3f,3);
            check(ailments.TickBurn(.49f)==0&&ailments.TickBurn(.5f)==5,"Refreshing weaker burn neither delays a tick nor lowers damage");
            check(ailments.TickBurn(.5f)==0,"A burn tick cannot resolve twice");
            check(ailments.TickBurn(3)==15&&ailments.TickBurn(4)==0,"Long frames preserve remaining burn damage without extending it");
            ailments.Chill(4,.9f);check(ailments.Slow(4,false)==.45f&&ailments.Slow(4,true)==.15f,"Crowd control is capped separately for regular enemies and bosses");
            check(ailments.Slow(7,false)==0,"Movement returns to normal after chill expires");
            ailments.Ignite(7,9);ailments.Chill(7,.3f);ailments.Clear();check(ailments.TickBurn(8)==0&&ailments.Slow(8,false)==0,"Clearing ailments removes pending damage and slow");
            foreach(var weapon in new[]{RunWeapon.Blade,RunWeapon.Spear,RunWeapon.Railgun,RunWeapon.Grenade}){
                var cycle=new WeaponAttackCycle();var profile=WeaponArsenal.Profile(weapon);
                check(cycle.Begin(10,profile,1)&&!cycle.Resolve(10)&&!cycle.Begin(10.01f,profile,1),weapon+" respects windup and cannot queue duplicate attacks");
                check(cycle.Resolve(20)&&!cycle.Resolve(20),weapon+" resolves once across a delayed frame");
                cycle.Begin(25,profile,1);cycle.Cancel();check(!cycle.Resolve(100),weapon+" cancels its delayed hit on wave end");
            }
            var grenade=new WeaponAttackCycle();grenade.Begin(0,WeaponArsenal.Profile(RunWeapon.Grenade),5);
            check(Math.Abs(grenade.ImpactAt-.65f)<.001f&&grenade.ReadyAt>grenade.ImpactAt,"Attack speed cannot skip grenade flight or overlap a slot's casts");
            check(WavePressure.AssaultSize(5)==0&&WavePressure.EliteLimit(6)==0,"Early waves exclude assaults and elites");
            check(WavePressure.AssaultSize(20)<=8&&WavePressure.EliteLimit(20)==4&&WavePressure.AssaultInterval(20)>=7,"Final-wave reinforcements and elites stay within mobile budgets");
            bool monotonic=true;for(int wave=7;wave<=20;wave++)monotonic&=WavePressure.AssaultInterval(wave)<=WavePressure.AssaultInterval(wave-1)&&WavePressure.AssaultSize(wave)>=WavePressure.AssaultSize(wave-1);
            check(monotonic,"Reinforcement pressure continues rising in later waves");
            check(!WavePressure.EliteSpawn(20,63,4),"Elite population cap prevents additional special attacks");
            var strike=new LockedAreaStrike();strike.Begin(10);strike.Begin(10.5f);
            check(Math.Abs(strike.ImpactAt-11.15f)<.001f&&!strike.Resolve(11,true),"Elite warning has a fixed readable deadline");
            check(strike.Resolve(12,true)&&!strike.Resolve(12,true),"Elite area hit resolves only once");
            strike.Begin(14);check(!strike.Resolve(16,false)&&!strike.Pending,"Elite death or wave clear cancels its telegraph damage");
        }
    }
}
