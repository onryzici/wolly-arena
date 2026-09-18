using UnityEngine;

namespace WoollyArena
{
    // Sixteen reusable accents: four reserved for player actions and milestones.
    // Events never allocate renderers, materials, meshes, arrays or coroutines.
    public sealed class CombatAccentFX : MonoBehaviour
    {
        const int Capacity = CombatAccentBudget.Capacity;
        sealed class Accent
        {
            public LineRenderer first, second;
            public CombatAccent kind;
            public Vector3 origin, direction;
            public float born, duration;
            public bool active;
        }
        readonly Accent[] pool = new Accent[Capacity];
        readonly CombatAccentBudget budget = new CombatAccentBudget();
        SurvivalRun run;
        Camera view;
        Material material;
        public int ActiveCount { get; private set; }
        public const int MaximumActive = Capacity;

        public void Initialize(SurvivalRun owner)
        {
            run = owner; view = Camera.main;
            material = new Material(Resources.Load<Shader>("PowerInk"));
            material.SetColor("_Tint", Color.white);
            for (int i = 0; i < Capacity; i++)
                pool[i] = new Accent { first = Line("Cartoon accent"), second = Line("Accent echo") };
        }
        LineRenderer Line(string label)
        {
            var obj = new GameObject(label); obj.transform.SetParent(transform, false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.positionCount = 3; line.numCapVertices = 2;
            line.startWidth = line.endWidth = 1;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false; line.enabled = false; return line;
        }
        public void Play(CombatAccent kind, Vector3 origin, Vector3 direction = default)
        {
            if (!run || run.IsPaused || !isActiveAndEnabled) return;
            if (run.Phase != SurvivalRun.RunPhase.Wave && run.Phase != SurvivalRun.RunPhase.WaveClear) return;
            float now = Time.time;
            if (!budget.TryTake(kind, now, out int index)) return;
            var e = pool[index]; e.kind = kind; e.origin = origin; e.born = now;
            e.duration = CombatAccentBudget.Duration(kind);
            direction.y = 0; e.direction = direction.sqrMagnitude > .001f ? direction.normalized : Vector3.forward;
            e.active = true; Draw(e, 0);
        }
        void Update()
        {
            if (!run) return;
            if (run.Phase != SurvivalRun.RunPhase.Wave && run.Phase != SurvivalRun.RunPhase.WaveClear) { Clear(); return; }
            if (run.IsPaused) return;
            ActiveCount = 0;
            foreach (var e in pool)
            {
                if (e == null || !e.active) continue;
                float u = (Time.time - e.born) / e.duration;
                if (u >= 1) { e.active = false; e.first.enabled = e.second.enabled = false; }
                else { Draw(e, Mathf.Clamp01(u)); ActiveCount++; }
            }
        }
        void Draw(Accent e, float u)
        {
            Vector3 right = view ? view.transform.right : Vector3.right;
            Vector3 up = view ? view.transform.up : Vector3.up;
            Color gold = new Color(1, .74f, .24f), mint = new Color(.4f, 1, .73f);
            Color color = e.kind == CombatAccent.LevelUp ? mint : e.kind == CombatAccent.Dash ? new Color(.6f, .88f, 1) : gold;
            float fade = 1 - u * u;
            Style(e.first, Color.Lerp(Color.white, color, Mathf.Clamp01(u * 3)), fade, .045f * (1 - u * .7f));
            Style(e.second, color, fade * .7f, .025f * (1 - u * .7f));
            var ground = new Vector3(e.origin.x, .06f, e.origin.z);
            if (e.kind == CombatAccent.Dash)
            {
                var side = Vector3.Cross(Vector3.up, e.direction);
                var at = ground - e.direction * u * .65f;
                Chevron(e.first, at, e.direction, side, .32f + u * .16f);
                Chevron(e.second, at - e.direction * .36f, e.direction, side, .23f + u * .12f);
            }
            else if (e.kind == CombatAccent.LevelUp)
            {
                // Narrow ascending ribbon around the feet, leaving the hero's face unobscured.
                e.first.loop = false; e.first.positionCount = 33;
                for (int i = 0; i < 33; i++)
                {
                    float t = i / 32f, angle = (t * 1.35f + u) * Mathf.PI * 2;
                    e.first.SetPosition(i, ground + new Vector3(Mathf.Cos(angle) * .55f, .12f + t * .75f + u * .5f, Mathf.Sin(angle) * .55f));
                }
                Ring(e.second, ground, .2f + u * 1.15f);
            }
            else if (e.kind == CombatAccent.BossDefeat)
            {
                Star(e.first, e.origin + Vector3.up * (1 + u * .7f), right, up, .4f + u * .35f, .13f, 6, u * .5f);
                Ring(e.second, ground, .25f + u * 2.1f);
            }
            else
            {
                bool pickup = e.kind == CombatAccent.Pickup, critical = e.kind == CombatAccent.Critical;
                var at = e.origin + Vector3.up * (pickup ? .35f + u * .35f : .65f + u * .18f);
                float size = (pickup ? .20f : critical ? .48f : .30f) * (1 + Mathf.Sin(u * Mathf.PI) * .35f);
                Star(e.first, at, right, up, size, size * .22f, critical || pickup ? 4 : 5, u * .45f);
                if (pickup) Star(e.second, at + right * .24f + up * .2f, right, up, .1f * (1 - u), .024f * (1 - u), 4, -u);
                else Ring(e.second, ground, .12f + u * (critical ? .65f : .42f));
            }
        }
        static void Style(LineRenderer line, Color color, float alpha, float width)
        {
            color.a = alpha; line.startColor = line.endColor = color; line.widthMultiplier = width;
            line.enabled = true;
        }
        static void Star(LineRenderer line, Vector3 at, Vector3 right, Vector3 up, float outer, float inner, int points, float spin)
        {
            line.loop = true; line.positionCount = points * 2;
            for (int i = 0; i < points * 2; i++)
            {
                float angle = i * Mathf.PI / points + spin, radius = i % 2 == 0 ? outer : inner;
                line.SetPosition(i, at + (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius);
            }
        }
        static void Ring(LineRenderer line, Vector3 at, float radius)
        {
            line.loop = true; line.positionCount = 32;
            for (int i = 0; i < 32; i++) { float a = i * Mathf.PI / 16; line.SetPosition(i, at + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius); }
        }
        static void Chevron(LineRenderer line, Vector3 at, Vector3 forward, Vector3 side, float size)
        {
            line.loop = false; line.positionCount = 3;
            line.SetPosition(0, at - side * size - forward * .18f); line.SetPosition(1, at + forward * .12f); line.SetPosition(2, at + side * size - forward * .18f);
        }
        public void Clear()
        {
            foreach (var e in pool) if (e != null) { e.active = false; e.first.enabled = e.second.enabled = false; }
            ActiveCount = 0;
        }
        void OnDisable() { Clear(); }
        void OnDestroy() { if (material) Destroy(material); }
    }
}
