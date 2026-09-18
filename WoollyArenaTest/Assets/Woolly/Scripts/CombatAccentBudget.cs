using System;
namespace WoollyArena
{
    public enum CombatAccent { Critical, Defeat, Pickup, LevelUp, Dash, BossDefeat }

    // Pure scheduling policy, executable in offline tests without an Editor.
    public sealed class CombatAccentBudget
    {
        public const int Capacity = 16, Reserved = 4;
        readonly float[] nextAllowed = new float[6];
        int commonCursor, reservedCursor;
        public static float Duration(CombatAccent kind) => kind == CombatAccent.LevelUp ? .85f : kind == CombatAccent.BossDefeat ? 1.05f : kind == CombatAccent.Dash ? .3f : .32f;
        public bool TryTake(CombatAccent kind, float now, out int slot)
        {
            slot = -1; int index = (int)kind;
            if (index < 0 || index >= nextAllowed.Length || float.IsNaN(now) || float.IsInfinity(now) || now < 0 || now < nextAllowed[index]) return false;
            float gap = kind == CombatAccent.Pickup ? .09f : kind == CombatAccent.Defeat ? .075f : kind == CombatAccent.Critical ? .1f : .25f;
            nextAllowed[index] = now + gap;
            bool reserved = kind == CombatAccent.Dash || kind == CombatAccent.LevelUp || kind == CombatAccent.BossDefeat;
            if (reserved) { slot = reservedCursor; reservedCursor = (reservedCursor + 1) % Reserved; }
            else { slot = Reserved + commonCursor; commonCursor = (commonCursor + 1) % (Capacity - Reserved); }
            return true;
        }
    }
}
