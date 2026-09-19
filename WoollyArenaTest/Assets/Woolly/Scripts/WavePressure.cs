using System;
namespace WoollyArena
{
    // Pressure comes from readable threats and reinforcements; the early-wave stat curve stays intact.
    public static class WavePressure
    {
        public static float AssaultInterval(int wave) => wave < 6 ? float.PositiveInfinity : Math.Max(7, 13 - (wave - 6) * .4f);
        public static int AssaultSize(int wave) => wave < 6 ? 0 : Math.Min(8, 3 + (wave - 6) / 3);
        public static int EliteLimit(int wave) => wave < 7 ? 0 : Math.Min(4, 1 + (wave - 7) / 4);
        public static bool EliteSpawn(int wave, int serial, int alive) => wave >= 7 && alive < EliteLimit(wave) && serial % Math.Max(7, 13 - wave / 3) == 0;
        public static float EliteCooldown(int wave) => Math.Max(3.6f, 5.8f - Math.Max(0, wave - 7) * .13f);
        public const float EliteWindup = 1.15f;
        public const float EliteRadius = 1.85f;
    }
    public sealed class LockedAreaStrike
    {
        public bool Pending { get; private set; }
        public float ImpactAt { get; private set; }
        public void Begin(float now) { if (!Pending) { Pending = true; ImpactAt = now + WavePressure.EliteWindup; } }
        public bool Resolve(float now, bool allowed)
        {
            if (!allowed) { Cancel(); return false; }
            if (!Pending || now < ImpactAt) return false;
            Pending = false; return true;
        }
        public void Cancel() { Pending = false; }
    }
}
