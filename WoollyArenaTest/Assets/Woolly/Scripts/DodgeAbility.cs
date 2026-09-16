using UnityEngine;

namespace WoollyArena
{
    [DefaultExecutionOrder(220)]
    public sealed class DodgeAbility : MonoBehaviour
    {
        [Min(.1f)] public float distance = 2.25f;
        [Min(.1f)] public float duration = .52f;
        [Min(.1f)] public float cooldown = 1.1f;
        [Min(0)] public float protectionDuration = .28f;
        public bool IsDodging { get; private set; }
        public Vector3 Direction { get; private set; }
        public float CooldownRemaining => Mathf.Max(0, readyAt - Time.time);
        public bool Ready => !IsDodging && CooldownRemaining <= 0;
        public float Progress => IsDodging ? Mathf.Clamp01((Time.time - started) / duration) : 1;

        ArenaPlayer player;
        CharacterVitals vitals;
        HitReaction hitReaction;
        ParticleSystem dust;
        static readonly int RollState = Animator.StringToHash("Base Layer.Dodge");
        static readonly int LocomotionState = Animator.StringToHash("Base Layer.Locomotion");
        bool ownsAnimation;
        float started, readyAt, travelledFraction, nextDust;

        public void Initialize(ArenaPlayer owner, CharacterVitals health, HitReaction hits)
        {
            player = owner;
            vitals = health;
            hitReaction = hits;
            var feet = GetComponent<FootstepDust>();
            if (feet) dust = feet.dust;
        }

        public bool TryBegin(Vector3 direction, bool grounded)
        {
            if (!Ready || !grounded || !vitals || vitals.Health <= 0) return false;
            direction.y = 0;
            if (direction.sqrMagnitude < .001f) direction = player.visual.forward;
            Direction = direction.normalized;
            started = Time.time;
            readyAt = started + Mathf.Max(duration, cooldown);
            travelledFraction = 0;
            nextDust = 0;
            IsDodging = true;
            hitReaction.ResetReaction();
            // A dodge must not shorten the longer protection granted by respawning.
            vitals.ProtectFor(Mathf.Max(vitals.InvulnerableUntil - Time.time, Mathf.Min(protectionDuration, duration)));
            player.visual.rotation = Quaternion.LookRotation(Direction);
            var animator = player.animator;
            animator.SetFloat("DodgePlayback", .52f / duration);
            animator.ResetTrigger("Fire");
            animator.Play("Upper Body Fire.Ready", 1, 0);
            animator.SetLayerWeight(1, 0);
            animator.CrossFadeInFixedTime(RollState, .025f, 0, 0);
            ownsAnimation = true;
            return true;
        }

        public Vector3 ConsumeDisplacement()
        {
            if (!IsDodging) return Vector3.zero;
            float u = Progress;
            // Travel follows the crouch / shoulder contact / recovery phases of the clip.
            float fraction = u - Mathf.Sin(u * Mathf.PI * 2) / (Mathf.PI * 2);
            var displacement = Direction * (distance * (fraction - travelledFraction));
            travelledFraction = fraction;
            if (u >= 1) { IsDodging = false; RestoreAnimation(); }
            return displacement;
        }

        public void ReportMovement(Vector3 movement)
        {
            movement.y = 0;
            if (!IsDodging || !dust || movement.sqrMagnitude < .0001f || Time.time < nextDust) return;
            nextDust = Time.time + .035f;
            var puff = new ParticleSystem.EmitParams
            {
                position = transform.position + Vector3.up * .06f - Direction * .18f,
                velocity = -Direction * .6f + Vector3.up * .2f,
                startLifetime = .28f,
                startSize = .24f,
                startColor = new Color(1, .81f, .53f, .48f)
            };
            dust.Emit(puff, 2);
        }

        void RestoreAnimation()
        {
            if (!ownsAnimation || !player || !player.animator) return;
            ownsAnimation = false;
            player.animator.SetLayerWeight(1, 1);
            if (player.animator.isActiveAndEnabled) player.animator.CrossFadeInFixedTime(LocomotionState, .055f, 0);
        }

        public void ResetAbility()
        {
            IsDodging = false;
            readyAt = 0;
            travelledFraction = 0;
            RestoreAnimation();
        }

        void OnDisable() { ResetAbility(); }
    }
}
