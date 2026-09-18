using UnityEngine;

namespace WoollyArena
{
    public sealed class CharacterStats : MonoBehaviour
    {
        public SurvivalBuild Build { get; private set; }
        public SurvivalRun Run => run;
        public bool LastCritical { get; private set; }
        public int CriticalHits { get; private set; }
        ArenaPlayer player;
        CharacterVitals vitals;
        SurvivalRun run;
        float regeneration,hasteUntil;
        public float AttackMultiplier=>Build.AttackMultiplier*(Time.time<hasteUntil?1.25f:1);
        public void GrantDashHaste(){hasteUntil=Time.time+2;}
        public void Initialize(SurvivalBuild build, ArenaPlayer hero, SurvivalRun owner)
        {
            Build = build; player = hero; run = owner; vitals = GetComponent<CharacterVitals>();
            Apply();
        }
        public void Apply()
        {
            vitals.SetMaximum(Build.Stat(RunStat.MaxHealth));
            vitals.Armor = Build.Stat(RunStat.Armor);
            player.walkSpeed = 1.8f * Build.MoveMultiplier;
            player.runSpeed = 4.8f * Build.MoveMultiplier;
            player.acceleration = 28;
            player.turnSpeed = 1080;
            player.shotDamage = Mathf.Max(1, Mathf.RoundToInt(25 * Build.DamageMultiplier));
            player.weapon.Interval = .28f / Build.AttackMultiplier;
        }
        public int RollDamage(float baseDamage)
        {
            bool critical = Random.value * 100 < Build.Stat(RunStat.Critical);
            LastCritical = critical;
            if (critical) CriticalHits++;
            return Mathf.Max(1, Mathf.RoundToInt(baseDamage * Build.DamageMultiplier * (critical ? 2 : 1)));
        }
        void Update()
        {
            if (Build == null || !run || run.Phase != SurvivalRun.RunPhase.Wave || vitals.Health <= 0) return;
            regeneration += Time.deltaTime;
            if (regeneration >= 5) { regeneration -= 5; vitals.Heal(Build.Stat(RunStat.Regeneration)); }
        }
    }
}
