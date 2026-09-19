using System;
using UnityEngine;

namespace WoollyArena
{
    public enum EnemyRole { Pursuer, Flanker, Interceptor, Skirmisher }

    // Decisions are sampled by each agent, not recalculated for the whole crowd every frame.
    public static class EnemyTactics
    {
        public static EnemyRole Role(int kind, int slot, bool boss) => boss ? EnemyRole.Pursuer : kind == 3 ? EnemyRole.Skirmisher : kind == 1 ? EnemyRole.Flanker : kind == 2 || slot % 4 == 0 ? EnemyRole.Interceptor : EnemyRole.Pursuer;
        public static Vector3 Goal(EnemyRole role, Vector3 enemy, Vector3 target, Vector3 velocity, int slot, bool clearSight)
        {
            var away = enemy - target; away.y = 0;
            float distance = away.magnitude;
            if (distance < .01f) away = Vector3.right; else away /= distance;
            float side = slot % 2 == 0 ? 1 : -1;
            var tangent = new Vector3(-away.z, 0, away.x) * side;
            velocity.y = 0; velocity = Vector3.ClampMagnitude(velocity, 5.5f);
            if (role == EnemyRole.Skirmisher)
            {
                if (distance < 4.2f) return target + away * 6.2f + tangent * 1.2f;
                if (!clearSight) return target + away * 5.8f + tangent * 3;
                if (distance > 7.2f) return target + away * 6;
                return enemy;
            }
            // Close-range attackers commit; circling never prevents their strike from landing.
            if (distance < 2.5f) return target;
            if (role == EnemyRole.Flanker && distance > 3.4f)
                return target + Vector3.ClampMagnitude(velocity * .3f, 1.5f) + away * 1.35f + tangent * 2.4f;
            if (role == EnemyRole.Interceptor)
                return target + Vector3.ClampMagnitude(velocity * .65f, 2.4f) + tangent * .45f;
            return target + tangent * .35f;
        }
    }

    public static class SquadRules
    {
        public const int Capacity = 24;
        public const float Warning = .95f, SafeDistance = 4.25f;
        public static int Size(int wave) => Math.Min(5, 3 + Math.Max(0, wave - 1) / 7);
        public static float Interval(int wave) => Math.Max(1.15f, 2.4f - Math.Max(0, wave - 1) * .065f);
        public static int Available(int alive, int pending, int limit) => Math.Max(0, Math.Min(Capacity - pending, limit - alive - pending));
        public static bool Safe(float playerDistance, bool clear, bool completePath) => playerDistance >= SafeDistance && clear && completePath;
        public static int Sector(int sequence) => ((sequence * 3) % 8 + 8) % 8;
    }
}
