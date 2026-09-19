using UnityEngine;
namespace WoollyArena
{
    public sealed partial class LoadoutCombat
    {
        readonly WeaponAttackCycle[] cycles = new WeaponAttackCycle[6];
        readonly Vector3[] lockedPoints = new Vector3[6], lockedDirections = new Vector3[6];
        readonly RaycastHit[] rayHits = new RaycastHit[64];
        readonly EnemyAgent[] pierced = new EnemyAgent[3];
        readonly int[] followups=new int[6];
        readonly float[] followupAt=new float[6];
        readonly Vector3[] castOrigins=new Vector3[6];
        readonly RepeaterHeat[] heat={new RepeaterHeat(),new RepeaterHeat(),new RepeaterHeat(),new RepeaterHeat(),new RepeaterHeat(),new RepeaterHeat()};
        ArsenalVFX arsenalFX;
        public void CancelAttacks()
        {
            foreach (var cycle in cycles) cycle?.Cancel();
            for(int i=0;i<6;i++)followups[i]=0;
            if (arsenalFX) arsenalFX.Clear();
        }
        public void AnimatePower(int kind)
        {
            for (int i = 0; i < run.Build.Weapons.Count; i++)
                if ((int)run.Build.Weapons[i].Item.Weapon.Value == kind + 3) recoil[i] = .23f;
        }
        bool ClearRoute(Vector3 from, Vector3 to) => CombatSight.Clear(from,to);
        int Roll(OwnedGear gear, float baseDamage) => player.Stats.RollDamage(baseDamage * RunItem.TierScale(gear.Tier) * run.Build.WeaponDamageMultiplier(gear.Item.Weapon.Value), run.Build.WeaponCriticalBonus(gear.Item.Weapon.Value));
        void BeginAttack(int slot, OwnedGear gear, Vector3 point)
        {
            var kind = gear.Item.Weapon.Value;
            var profile = WeaponArsenal.Profile(kind);
            lockedPoints[slot] = point;
            castOrigins[slot]=player.transform.position+Vector3.up*.8f;
            var direction = point - player.transform.position; direction.y = 0;
            lockedDirections[slot] = direction.sqrMagnitude > .001f ? direction.normalized : player.visual.forward;
            if (kind == RunWeapon.Grenade||kind==RunWeapon.Mortar||kind==RunWeapon.Boomerang) arsenalFX.Play(kind, muzzles[slot], point, profile.Radius, cycles[slot].ImpactAt - Time.time);
            if (profile.Windup > 0) run.Sfx.Play(profile.Motion == WeaponMotion.Sweep || profile.Motion == WeaponMotion.Thrust ? CombatCue.Whoosh : CombatCue.Charge, muzzles[slot]);
        }
        void ResolveAttack(int slot, OwnedGear gear)
        {
            var kind = gear.Item.Weapon.Value; var p = WeaponArsenal.Profile(kind);
            var origin = muzzles[slot]; var point = lockedPoints[slot];
            var chest = player.transform.position + Vector3.up * .85f;
            var forward = lockedDirections[slot];
            recoil[slot] = kind == RunWeapon.Shotgun || kind == RunWeapon.Railgun ? .18f : .09f;
            Shots++;
            if (kind == RunWeapon.Revolver || kind == RunWeapon.Repeater || kind == RunWeapon.Shotgun)
            {
                if(kind==RunWeapon.Repeater)heat[slot].Fire(Time.time);
                run.Sfx.Play(kind == RunWeapon.Shotgun ? CombatCue.Shotgun : kind == RunWeapon.Repeater ? CombatCue.Repeater : CombatCue.Revolver, origin);
                int pellets = kind == RunWeapon.Shotgun ? 3 : 1;
                for (int pellet = 0; pellet < pellets; pellet++)
                    player.FireEquipment(origin, Quaternion.Euler(0, pellets == 1 ? 0 : (pellet - 1) * 9, 0) * (point - origin).normalized, Roll(gear, p.Damage), p.Range);
                return;
            }
            if (kind == RunWeapon.Railgun||kind==RunWeapon.Crossbow) { Rail(origin, point, gear); return; }
            if(kind==RunWeapon.BurstRifle){BurstRound(slot,gear);followups[slot]=2;followupAt[slot]=Time.time+.08f;return;}
            if(kind==RunWeapon.Ricochet){Ricochet(origin,point,gear);return;}
            if(kind==RunWeapon.Boomerang){ReturnSweep(castOrigins[slot],point,gear);followups[slot]=1;followupAt[slot]=Time.time+.35f;return;}
            if (kind == RunWeapon.Grenade || kind == RunWeapon.Frost||kind==RunWeapon.Mortar||kind==RunWeapon.Hammer)
            {
                if(kind==RunWeapon.Hammer)point=chest;
                float radius = p.Radius * run.Build.AreaMultiplier;
                // A locked blast location may be dodged. Scenery blocks each splash ray.
                foreach (var enemy in run.Director.Enemies)
                {
                    if (!Valid(enemy) || (enemy.transform.position - point).sqrMagnitude > radius * radius) continue;
                    var end = enemy.transform.position + Vector3.up * enemy.AimHeight;
                    if (!ClearRoute(point, end)) continue;
                    if (enemy.Damage(Roll(gear, p.Damage), end - point, player.Stats.LastCritical) && kind == RunWeapon.Frost) enemy.Chill(run.Build.ChillStrength);
                }
                arsenalFX.Play(kind, point, new Vector3(point.x, .1f, point.z), radius, .42f, true);
                run.ActionFX.Burst(point, WeaponArsenal.ColorFor(kind), 14);
                run.Sfx.Play(CombatCue.Power, point);
                return;
            }
            // Wide slash, narrow thrust and short flame cone have different positioning requirements.
            foreach (var enemy in run.Director.Enemies)
            {
                if (!Valid(enemy)) continue;
                var delta = enemy.transform.position - player.transform.position; delta.y = 0;
                float distance = delta.magnitude, along = Vector3.Dot(delta, forward);
                if (distance > p.Range || (along < 0&&kind!=RunWeapon.Saw)) continue;
                bool inside = kind == RunWeapon.Spear ? along <= p.Range && (delta - forward * along).sqrMagnitude <= .5f * .5f
                    : kind==RunWeapon.Saw || Vector3.Dot(delta.normalized, forward) >= (kind == RunWeapon.Blade ? .5f :kind==RunWeapon.Axe?0:.87f);
                var end = enemy.transform.position + Vector3.up * enemy.AimHeight;
                if (!inside || !ClearRoute(chest, end)) continue;
                if (enemy.Damage(Roll(gear, p.Damage), forward, player.Stats.LastCritical, kind != RunWeapon.Flame&&kind!=RunWeapon.Saw) && kind == RunWeapon.Flame)
                    enemy.Ignite(Mathf.Max(1, Mathf.RoundToInt(5 * RunItem.TierScale(gear.Tier) * run.Build.DamageMultiplier * run.Build.BurnMultiplier)));
            }
            if(kind==RunWeapon.Flame)arsenalFX.Spray(tools[slot],Mathf.Max(.3f,p.Range-Vector3.Distance(chest,muzzles[slot])));
            else arsenalFX.Play(kind, chest, chest + forward * p.Range, p.Range, .14f);
            run.Sfx.Play(kind == RunWeapon.Flame ? CombatCue.Laser : CombatCue.Whoosh, origin);
        }
        static bool Valid(EnemyAgent enemy) => enemy && enemy.isActiveAndEnabled && !enemy.Defeated && enemy.vitals.Health > 0;
        void Rail(Vector3 origin, Vector3 point, OwnedGear gear)
        {
            var kind=gear.Item.Weapon.Value;var p = WeaponArsenal.Profile(kind);int maximum=kind==RunWeapon.Crossbow?2:3;
            var chest = player.transform.position + Vector3.up * .85f;
            var direction = (point - origin).normalized; var end = origin + direction * p.Range;
            int mask = ~(1 << player.gameObject.layer);
            if (Physics.Linecast(chest, origin, out var obstruction, mask, QueryTriggerInteraction.Ignore))
            {
                var enemy = obstruction.collider.GetComponentInParent<EnemyAgent>();
                if (enemy) enemy.Damage(Roll(gear, p.Damage), direction, player.Stats.LastCritical);
                arsenalFX.Play(kind, chest, obstruction.point, 1, .2f); return;
            }
            int count = Physics.RaycastNonAlloc(origin, direction, rayHits, p.Range, mask, QueryTriggerInteraction.Ignore);
            // Insertion sort a fixed buffer; penetration is ordered and never jumps a wall.
            for (int i = 1; i < count; i++) { var hit = rayHits[i]; int j = i - 1; while (j >= 0 && rayHits[j].distance > hit.distance) { rayHits[j + 1] = rayHits[j]; j--; } rayHits[j + 1] = hit; }
            int struck = 0;
            for (int i = 0; i < count; i++)
            {
                var hit = rayHits[i]; var enemy = hit.collider.GetComponentInParent<EnemyAgent>();
                if (!enemy) { hit.collider.GetComponentInParent<BreakableProp>()?.Damage(Roll(gear, p.Damage)); end = hit.point; break; }
                bool duplicate = false; for (int j = 0; j < struck; j++) if (pierced[j] == enemy) duplicate = true;
                if (duplicate) continue;
                enemy.Damage(Roll(gear, p.Damage * Mathf.Pow(.75f, struck)), direction, player.Stats.LastCritical);
                pierced[struck++] = enemy;
                if (struck == maximum) { end = hit.point; break; }
            }
            arsenalFX.Play(kind, origin, end, 1, .24f);
            run.Sfx.Play(CombatCue.Laser, origin);
        }
        void BurstRound(int slot,OwnedGear gear){var origin=muzzles[slot];player.FireEquipment(origin,(lockedPoints[slot]-origin).normalized,Roll(gear,WeaponArsenal.Profile(RunWeapon.BurstRifle).Damage),11);recoil[slot]=.09f;run.Sfx.Play(CombatCue.Repeater,origin);}
        void TickFollowups(int slot,OwnedGear gear){if(followups[slot]==0||Time.time<followupAt[slot])return;followups[slot]--;followupAt[slot]=Time.time+.08f;if(gear.Item.Weapon.Value==RunWeapon.BurstRifle)BurstRound(slot,gear);else if(gear.Item.Weapon.Value==RunWeapon.Boomerang){var end=player.transform.position+Vector3.up*.8f;ReturnSweep(lockedPoints[slot],end,gear);arsenalFX.Play(RunWeapon.Boomerang,lockedPoints[slot],end,1,.3f);}}
        void ReturnSweep(Vector3 from,Vector3 to,OwnedGear gear){var axis=to-from;axis.y=0;float length=axis.magnitude;if(length<.01f)return;axis/=length;foreach(var enemy in run.Director.Enemies){if(!Valid(enemy))continue;var delta=enemy.transform.position-from;delta.y=0;float along=Vector3.Dot(delta,axis);if(along<0||along>length+.3f||(delta-axis*along).sqrMagnitude>.45f*.45f||!ClearRoute(from,enemy.transform.position+Vector3.up*enemy.AimHeight))continue;enemy.Damage(Roll(gear,24),axis,player.Stats.LastCritical);}}
        void Ricochet(Vector3 origin,Vector3 point,OwnedGear gear){
            int mask=~(1<<player.gameObject.layer);var direction=(point-origin).normalized;var chest=player.transform.position+Vector3.up*.85f;
            bool hit=Physics.Linecast(chest,origin,out var impact,mask,QueryTriggerInteraction.Ignore)||Physics.Raycast(origin,direction,out impact,10,mask,QueryTriggerInteraction.Ignore);
            if(!hit)return;var current=impact.collider.GetComponentInParent<EnemyAgent>();var from=origin;
            for(int bounce=0;bounce<3&&current;bounce++){
                var end=current.transform.position+Vector3.up*current.AimHeight;current.Damage(Roll(gear,23*Mathf.Pow(.65f,bounce)),(end-from).normalized,player.Stats.LastCritical);pierced[bounce]=current;
                arsenalFX.Play(RunWeapon.Ricochet,from,end,1,.18f);from=end;EnemyAgent next=null;float nearest=3.5f;
                foreach(var enemy in run.Director.Enemies){if(!Valid(enemy))continue;bool visited=false;for(int j=0;j<=bounce;j++)if(pierced[j]==enemy)visited=true;if(visited)continue;float distance=Vector3.Distance(enemy.transform.position,current.transform.position);if(distance>=nearest||!ClearRoute(from,enemy.transform.position+Vector3.up*enemy.AimHeight))continue;nearest=distance;next=enemy;}current=next;
            }
            run.Sfx.Play(CombatCue.Revolver,origin);
        }
    }
}
