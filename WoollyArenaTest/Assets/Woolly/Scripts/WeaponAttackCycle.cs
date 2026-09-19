using System;
namespace WoollyArena
{
    // A locked attack lands once after its windup, even if a frame crosses the deadline.
    public sealed class WeaponAttackCycle
    {
        public bool Pending { get; private set; }
        public float StartedAt { get; private set; } = -100;
        public float ImpactAt { get; private set; }
        public float ReadyAt { get; private set; }
        public bool Begin(float now, WeaponProfile profile, float speed)
        {
            if (Pending || now < ReadyAt) return false;
            speed = Math.Clamp(speed, .3f, 5);
            StartedAt = now;
            // Grenades retain readable travel; melee/charge animations follow attack speed.
            float windup = profile.Motion == WeaponMotion.Lob ? profile.Windup : profile.Windup / speed;
            ImpactAt = now + windup;
            ReadyAt = now + Math.Max(windup + .08f, profile.Interval / speed);
            Pending = true; return true;
        }
        public bool Resolve(float now) { if (!Pending || now < ImpactAt) return false; Pending = false; return true; }
        public void Cancel() { Pending = false; StartedAt = -100; ImpactAt = ReadyAt = 0; }
    }
}
