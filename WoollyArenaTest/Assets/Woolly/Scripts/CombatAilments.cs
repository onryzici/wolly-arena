using System;
namespace WoollyArena
{
    // Refreshing an ailment never stacks its strength or postpones an already scheduled tick.
    public sealed class CombatAilments
    {
        float burnEnd, nextBurn, chillEnd, slow;
        int burnDamage;
        public void Ignite(float now, int damage, float duration = 2)
        {
            if (damage <= 0 || duration <= 0) return;
            if (now >= burnEnd) { nextBurn = now + .5f; burnDamage = 0; }
            burnDamage = Math.Max(burnDamage, damage); burnEnd = now + duration;
        }
        public int TickBurn(float now)
        {
            if (burnDamage == 0 || now < nextBurn) return 0;
            // One bounded catch-up hit, retaining total damage across long frames.
            int ticks = Math.Max(0, (int)Math.Floor((Math.Min(now, burnEnd) - nextBurn + .0001f) / .5f) + 1);
            nextBurn += ticks * .5f;
            int damage = ticks * burnDamage;
            if (now >= burnEnd) burnDamage = 0;
            return damage;
        }
        public void Chill(float now, float strength, float duration = 2.2f)
        {
            if (duration <= 0) return;
            if (now >= chillEnd) slow = 0;
            slow = Math.Max(slow, Math.Clamp(strength, 0, .45f)); chillEnd = now + duration;
        }
        public float Slow(float now, bool boss) => now < chillEnd ? Math.Min(boss ? .15f : .45f, slow) : 0;
        public bool Burning(float now) => burnDamage > 0 && now < burnEnd;
        public void Clear() { burnDamage = 0; burnEnd = nextBurn = chillEnd = slow = 0; }
    }
}
