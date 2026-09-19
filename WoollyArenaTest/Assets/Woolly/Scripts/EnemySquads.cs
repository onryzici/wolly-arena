using UnityEngine;
using UnityEngine.AI;

namespace WoollyArena
{
    public sealed partial class EnemySpawnDirector
    {
        sealed class Arrival
        {
            public Vector3 point;
            public float due;
            public int sector, squad;
            public bool active;
            public LineRenderer halo, ring;
        }
        static readonly Vector3[] Regions = {
            new Vector3(-7.5f,0,7.2f), new Vector3(0,0,8), new Vector3(7.5f,0,7.2f), new Vector3(8.5f,0,0),
            new Vector3(7.5f,0,-7.2f), new Vector3(0,0,-8), new Vector3(-7.5f,0,-7.2f), new Vector3(-8.5f,0,0)
        };
        readonly Arrival[] arrivals = new Arrival[SquadRules.Capacity];
        NavMeshPath arrivalPath;
        Material arrivalMaterial;
        int groupSequence, nextSquad;
        Vector3 observedPlayer;
        public Vector3 PlayerVelocity { get; private set; }
        public int PendingCount { get; private set; }
        public int GroupsQueued { get; private set; }
        public int LastSquadSector { get; private set; } = -1;

        void InitializeSquads()
        {
            arrivalPath = new NavMeshPath();
            observedPlayer = player.position;
            arrivalMaterial = new Material(Resources.Load<Shader>("PowerInk"));
            arrivalMaterial.SetColor("_Tint", Color.white);
            for (int i = 0; i < arrivals.Length; i++)
                arrivals[i] = new Arrival { halo = ArrivalLine("Arrival boundary", .035f, 49), ring = ArrivalLine("Arrival countdown", .055f, 49) };
        }
        LineRenderer ArrivalLine(string label, float width, int count)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = arrivalMaterial;
            line.useWorldSpace = true; line.positionCount = count; line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = new Color(.50f, .12f, .08f, .9f);
            line.numCapVertices = 3;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.enabled = false;
            return line;
        }
        int LivingCount() { int count = 0; foreach (var enemy in Enemies) if (enemy && !enemy.Defeated) count++; return count; }
        bool CellClear(Vector3 point)
        {
            foreach (var enemy in Enemies) if (enemy && !enemy.Defeated && (enemy.transform.position - point).sqrMagnitude < 1.1f * 1.1f) return false;
            foreach (var arrival in arrivals) if (arrival != null && arrival.active && (arrival.point - point).sqrMagnitude < 1.15f * 1.15f) return false;
            return !Physics.CheckCapsule(point + Vector3.up * .4f, point + Vector3.up * 1.35f, .32f, 1, QueryTriggerInteraction.Ignore);
        }
        public int QueueSquads(int requested)
        {
            if (!prefab || !Run || Run.IsPaused || Run.Phase != SurvivalRun.RunPhase.Wave || requested <= 0) return 0;
            if (!arrivalMaterial) InitializeSquads();
            int available = Mathf.Min(requested, SquadRules.Available(LivingCount(), PendingCount, Run.EnemyLimit)), queued = 0;
            for (int attempt = 0; attempt < 8 && queued < available; attempt++)
            {
                int sector = SquadRules.Sector(groupSequence++);
                if (sector == LastSquadSector) continue;
                int members = Mathf.Min(SquadRules.Size(Run.Wave), available - queued), group = ++nextSquad, placed = 0;
                for (int cell = 0; cell < 15 && placed < members; cell++)
                {
                    var candidate = Regions[sector] + new Vector3((cell % 3 - 1) * 1.3f, 0, (cell / 3 - 2) * 1.3f);
                    if (!NavMesh.SamplePosition(candidate, out var nav, .65f, NavMesh.AllAreas)) continue;
                    if (Vector3.Distance(nav.position, player.position) < SquadRules.SafeDistance) continue;
                    bool clear = CellClear(nav.position);
                    bool path = clear && NavMesh.CalculatePath(nav.position, player.position, NavMesh.AllAreas, arrivalPath) && arrivalPath.status == NavMeshPathStatus.PathComplete;
                    if (!SquadRules.Safe(Vector3.Distance(nav.position, player.position), clear, path)) continue;
                    foreach (var arrival in arrivals)
                    {
                        if (arrival.active) continue;
                        arrival.active = true; arrival.point = nav.position; arrival.squad = group; arrival.sector = sector;
                        arrival.due = Time.time + SquadRules.Warning + placed * .08f;
                        arrival.halo.enabled = arrival.ring.enabled = true;
                        DrawArrival(arrival); PendingCount++; placed++; queued++; break;
                    }
                }
                if (placed > 0) { LastSquadSector = sector; GroupsQueued++; }
            }
            return queued;
        }
        void DrawArrival(Arrival a)
        {
            var p = a.point + Vector3.up * .055f;
            float progress = 1 - Mathf.Clamp01((a.due - Time.time) / SquadRules.Warning);
            var tint = new Color(.50f, .12f, .08f, .5f + .2f * progress);
            a.halo.startColor = a.halo.endColor = tint;
            for (int i = 0; i < 49; i++) {
                float angle = i * Mathf.PI / 24;
                a.halo.SetPosition(i, p + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * .48f);
                float sweep = -Mathf.PI * .5f + angle * Mathf.Max(.015f, progress);
                a.ring.SetPosition(i, p + new Vector3(Mathf.Cos(sweep), 0, Mathf.Sin(sweep)) * .48f);
            }
        }
        void TickSquads()
        {
            if (!Run || Run.IsPaused) return;
            if (Run.Phase != SurvivalRun.RunPhase.Wave) { CancelSquads(); return; }
            if (!arrivalMaterial) InitializeSquads();
            var delta = player.position - observedPlayer; observedPlayer = player.position;
            var velocity = Time.deltaTime > .001f ? delta / Time.deltaTime : Vector3.zero;
            PlayerVelocity = Vector3.Lerp(PlayerVelocity, Vector3.ClampMagnitude(velocity, 5.5f), 1 - Mathf.Exp(-8 * Time.deltaTime));
            foreach (var a in arrivals)
            {
                if (!a.active) continue;
                if (Time.time < a.due) { DrawArrival(a); continue; }
                a.active = false; a.halo.enabled = a.ring.enabled = false; PendingCount--;
                // The player can enter a warning to deny that cell; never materialize an enemy on them.
                if (Vector3.Distance(player.position, a.point) < 3.5f || LivingCount() >= Run.EnemyLimit || !CellClear(a.point)) continue;
                if (!NavMesh.SamplePosition(a.point, out var nav, .25f, NavMesh.AllAreas)) continue;
                SpawnAt(nav.position, false, a.squad, a.sector);
            }
        }
        public void CancelSquads()
        {
            PendingCount = 0; PlayerVelocity = Vector3.zero;
            if (player) observedPlayer = player.position;
            foreach (var arrival in arrivals) if (arrival != null) { arrival.active = false; arrival.halo.enabled = arrival.ring.enabled = false; }
        }
        void DestroySquads() { if (arrivalMaterial) Destroy(arrivalMaterial); }
    }
}
