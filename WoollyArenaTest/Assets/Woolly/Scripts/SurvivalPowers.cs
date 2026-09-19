using UnityEngine;

namespace WoollyArena
{
    public sealed class SurvivalPowers : MonoBehaviour
    {
        public enum Power { Galaxy, Energy, Star }
        public CartoonPowerVFX Effects { get; private set; }
        public int Casts { get; private set; }
        readonly int[] levels = new int[3];
        readonly float[] readyAt = new float[3], impactAt = new float[3];
        readonly bool[] pending = new bool[3];
        readonly Vector3[] targets = new Vector3[3];
        readonly EnemyAgent[] hitChain = new EnemyAgent[5];
        ArenaPlayer player;
        EnemySpawnDirector director;
        SurvivalRun run;
        public void Initialize(ArenaPlayer hero, EnemySpawnDirector source, SurvivalRun survival)
        {
            player = hero; director = source; run = survival;
            Effects = gameObject.AddComponent<CartoonPowerVFX>();
        }
        public int Level(int index) => index >= 0 && index < 3 ? levels[index] : 0;
        public void SetLevel(int index,int level){if(index>=0&&index<3)levels[index]=Mathf.Max(0,level);}
        public void Upgrade(int index) { if (index >= 0 && index < 3) levels[index]++; }
        public void Cancel()
        {
            for (int i = 0; i < 3; i++) pending[i] = false;
            if (Effects) Effects.Clear();
        }
        void Update()
        {
            if (!player || !run || run.IsPaused || run.Phase != SurvivalRun.RunPhase.Wave || player.CombatPaused || player.GetComponent<CharacterVitals>().Health <= 0) return;
            for (int i = 0; i < 3; i++)
            {
                if (pending[i] && Time.time >= impactAt[i]) { pending[i] = false; Impact(i, targets[i]); }
                if (levels[i] == 0 || pending[i] || Time.time < readyAt[i]) continue;
                var target = player.FindTarget();
                if (!target) continue;
                Casts++;run.Sfx.Play(CombatCue.Power);
                if(player.Loadout)player.Loadout.AnimatePower(i);
                targets[i] = target.transform.position;
                Effects.Play(i, player.Loadout ? player.Loadout.PowerOrigin(i) : player.transform.position + Vector3.up, targets[i], i == 0 ? 1.35f : 1);
                impactAt[i] = Time.time + (i == 1 ? .08f : .28f);
                pending[i] = true;
                readyAt[i] = Time.time + Mathf.Max(.8f, (i == 0 ? 3.2f : i == 1 ? 2 : 2.8f) - (levels[i] - 1) * .25f) / (player.Stats ? player.Stats.Build.AttackMultiplier : 1);
            }
        }
        void Impact(int kind, Vector3 position)
        {
            float baseDamage = 25 * (kind == 0 ? 1.8f : kind == 1 ? 1.1f : 3f) * (1 + .3f * (levels[kind] - 1));
            int damage = player.Stats ? player.Stats.RollDamage(baseDamage*run.Build.WeaponDamageMultiplier((RunWeapon)(kind+3))) : Mathf.RoundToInt(baseDamage);
            if (kind == 1)
            {
                var origin = position;
                int count = Mathf.Min(5, 2 + levels[kind]);
                for (int n = 0; n < hitChain.Length; n++) hitChain[n] = null;
                for (int jump = 0; jump < count; jump++)
                {
                    EnemyAgent nearest = null; float distance = jump == 0 ? 1.5f : 4;
                    foreach (var enemy in director.Enemies)
                    {
                        if (!enemy || enemy.Defeated || !enemy.isActiveAndEnabled) continue;
                        bool visited = false; for (int k = 0; k < jump; k++) if (hitChain[k] == enemy) visited = true;
                        if (visited) continue;
                        float d = Vector3.Distance(origin, enemy.transform.position);
                        if (d >= distance) continue;
                        if (jump > 0 && !CombatSight.Clear(origin + Vector3.up * .65f, enemy.transform.position + Vector3.up * enemy.AimHeight)) continue;
                        nearest = enemy; distance = d;
                    }
                    if (!nearest) break;
                    hitChain[jump] = nearest;
                    var end = nearest.transform.position;
                    if (jump > 0) Effects.Play(1, origin + Vector3.up, end, .8f);
                    nearest.Damage(damage, end - origin, player.Stats && player.Stats.LastCritical);
                    origin = end;
                }
            }
            else
            {
                float radius = (kind == 0 ? 2.7f : 1.25f)*run.Build.AreaMultiplier;
                foreach (var enemy in director.Enemies)
                    if (enemy && !enemy.Defeated && enemy.isActiveAndEnabled && Vector3.Distance(enemy.transform.position, position) <= radius
                        && CombatSight.Clear(position+Vector3.up*.65f,enemy.transform.position+Vector3.up*enemy.AimHeight))
                        enemy.Damage(damage, enemy.transform.position - position, player.Stats && player.Stats.LastCritical);
            }
        }
    }
}
