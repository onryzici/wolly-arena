using System.Collections.Generic;
using UnityEngine;

namespace WoollyArena
{
    // Owns presentation and the impulse curve; each motor resolves its own collisions.
    [DefaultExecutionOrder(200)]
    public sealed class HitReaction : MonoBehaviour
    {
        [Min(.01f)] public float pushDuration = .16f;
        [Min(.01f)] public float flashDuration = .12f;
        [Min(.01f)] public float flinchDuration = .22f;
        [Range(0, 30)] public float leanDegrees = 12f;

        public float impulseCooldown;
        public bool InterruptedLastHit { get; private set; }
        float nextImpulse;
        public bool Recoiling => motionTime < pushDuration;
        public float FlashAmount { get; private set; }

        static readonly int HitFlash = Shader.PropertyToID("_HitFlash");
        readonly List<Renderer> bodies = new List<Renderer>();
        MaterialPropertyBlock properties;
        CharacterVitals vitals;
        Transform visual, pivot;
        Vector3 direction;
        float distance, motionTime = float.PositiveInfinity, hitAt = float.NegativeInfinity;
        bool presenting;

        public Transform Initialize(Transform characterVisual, CharacterVitals characterVitals, float pushDistance)
        {
            if (pivot) return visual;
            vitals = characterVitals;
            properties = new MaterialPropertyBlock();
            distance = Mathf.Max(0, pushDistance);
            // Wrap the entire Animator root so animation clip paths remain intact.
            // Controllers rotate this outer transform; the inner pivot only adds flinch.
            visual = new GameObject("Character facing").transform;
            visual.SetParent(characterVisual.parent, false);
            visual.SetLocalPositionAndRotation(characterVisual.localPosition, characterVisual.localRotation);
            visual.localScale = characterVisual.localScale;
            pivot = new GameObject("Hit reaction pivot").transform;
            pivot.SetParent(visual, false);
            characterVisual.SetParent(pivot, false);
            characterVisual.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            characterVisual.localScale = Vector3.one;
            foreach (var body in pivot.GetComponentsInChildren<Renderer>())
                foreach (var material in body.sharedMaterials)
                    if (material && material.HasProperty(HitFlash)) { bodies.Add(body); break; }
            vitals.Damaged += React;
            return visual;
        }

        void React(Vector3 incoming)
        {
            if (!isActiveAndEnabled || !pivot) return;
            incoming.y = 0;
            direction = incoming.sqrMagnitude > .001f ? incoming.normalized : -visual.forward;
            // Refresh the impulse instead of stacking velocity during sustained fire.
            InterruptedLastHit = Time.time >= nextImpulse;
            if (InterruptedLastHit) { motionTime = 0; nextImpulse = Time.time + impulseCooldown; }
            hitAt = Time.time;
            presenting = true;
            SetFlash(1);
        }

        public Vector3 ConsumeDisplacement(float deltaTime)
        {
            if (!Recoiling) return Vector3.zero;
            float before = Mathf.Clamp01(motionTime / Mathf.Max(.001f, pushDuration));
            motionTime += Mathf.Max(0, deltaTime);
            float after = Mathf.Clamp01(motionTime / Mathf.Max(.001f, pushDuration));
            // Integrated ease-out: the same small distance at low and high frame rates.
            return direction * (distance * ((1 - before) * (1 - before) - (1 - after) * (1 - after)));
        }

        void LateUpdate()
        {
            if (!presenting || !pivot) return;
            float age = Time.time - hitAt;
            SetFlash(1 - Mathf.SmoothStep(0, 1, age / Mathf.Max(.001f, flashDuration)));
            float envelope = Mathf.Clamp01(age / .025f) * (1 - Mathf.SmoothStep(0, 1, age / Mathf.Max(.001f, flinchDuration)));
            var localDirection = visual.InverseTransformDirection(direction);
            pivot.localRotation = Quaternion.AngleAxis(leanDegrees * envelope, Vector3.Cross(Vector3.up, localDirection));
            pivot.localScale = new Vector3(1 + .035f * envelope, 1 - .055f * envelope, 1 + .035f * envelope);
            if (age >= Mathf.Max(flashDuration, flinchDuration)) ResetPresentation();
        }

        void SetFlash(float amount)
        {
            FlashAmount = Mathf.Clamp01(amount);
            foreach (var body in bodies)
            {
                if (!body) continue;
                body.GetPropertyBlock(properties);
                properties.SetFloat(HitFlash, FlashAmount);
                body.SetPropertyBlock(properties);
            }
        }

        void ResetPresentation()
        {
            presenting = false;
            SetFlash(0);
            if (pivot) { pivot.localRotation = Quaternion.identity; pivot.localScale = Vector3.one; }
        }

        public void ResetReaction()
        {
            motionTime = float.PositiveInfinity; nextImpulse = 0; InterruptedLastHit = false;
            ResetPresentation();
        }

        void OnDisable() { ResetReaction(); }
        void OnDestroy() { if (vitals) vitals.Damaged -= React; }
    }
}
