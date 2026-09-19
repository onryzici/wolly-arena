using UnityEngine;
using UnityEngine.Rendering;

namespace WoollyArena
{
    // Silhouette-first effects. Meshes are made once; attacks only reuse the fixed pools.
    public sealed class CartoonWeaponFX : MonoBehaviour
    {
        const int Cuts = 24, Tongues = 144, Segments = 22;
        sealed class Cut
        {
            public Mesh mesh; public MeshRenderer renderer;
            public readonly Vector3[] vertices = new Vector3[(Segments + 1) * 3];
            public readonly Color[] colors = new Color[(Segments + 1) * 3];
            public Vector3 origin, forward; public float born, life, radius; public RunWeapon weapon; public bool active, impact;
        }
        sealed class FlameBit
        {
            public Transform root; public MeshRenderer renderer; public MeshFilter filter; public Vector3 origin, direction;
            public float born, life, length, width, spin; public int frame; public bool active;
        }
        sealed class Nozzle { public Transform root; public float until, length, next; }
        readonly Cut[] cuts = new Cut[Cuts];
        readonly FlameBit[] flames = new FlameBit[Tongues];
        readonly Nozzle[] nozzles = new Nozzle[6];
        Material ink,flameInk; readonly Mesh[] flameFrames=new Mesh[8]; MaterialPropertyBlock tint; SurvivalRun run; Camera view;
        int cutCursor, flameCursor;
        public int ActiveFlames { get; private set; }
        public int ActiveCuts { get; private set; }
        public const int MaximumFlames = Tongues;
        public void Initialize(SurvivalRun owner)
        {
            run = owner; view = Camera.main; tint = new MaterialPropertyBlock();
            ink = new Material(Resources.Load<Shader>("PowerInk")); ink.SetColor("_Tint", Color.white);
            var indices = new int[Segments * 12];
            for (int i = 0; i < Segments; i++) for (int band = 0; band < 2; band++)
            {
                int at = i * 12 + band * 6, a = i * 3 + band, b = a + 3;
                indices[at] = a; indices[at + 1] = b; indices[at + 2] = a + 1;
                indices[at + 3] = a + 1; indices[at + 4] = b; indices[at + 5] = b + 1;
            }
            for (int i = 0; i < Cuts; i++)
            {
                var mesh = new Mesh { name = "Tapered cut ribbon" }; mesh.MarkDynamic();
                var cut = new Cut { mesh = mesh, renderer = Face("Sweeping blade trail", mesh) };
                mesh.vertices = cut.vertices; mesh.colors = cut.colors; mesh.triangles = indices; cuts[i] = cut;
            }
            flameInk=new Material(Resources.Load<Shader>("CombatParticle"));flameInk.SetTexture("_BaseMap",Resources.Load<Texture2D>("VFX/CartoonFire"));
            for(int i=0;i<8;i++)flameFrames[i]=MakeFlameFrame(i);
            for (int i = 0; i < Tongues; i++) { var face = Face("Cartoon flame tongue", flameFrames[0]);face.sharedMaterial=flameInk;flames[i] = new FlameBit { root = face.transform, renderer = face,filter=face.GetComponent<MeshFilter>() }; }
            for (int i = 0; i < nozzles.Length; i++) nozzles[i] = new Nozzle();
        }
        MeshRenderer Face(string label, Mesh mesh)
        {
            var go = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(transform, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh; var face = go.GetComponent<MeshRenderer>();
            face.sharedMaterial = ink; face.shadowCastingMode = ShadowCastingMode.Off; face.receiveShadows = false; face.enabled = false; return face;
        }
        public void Slash(RunWeapon weapon, Vector3 origin, Vector3 end, float radius, float life)
        {
            var cut = cuts[cutCursor++ % Cuts]; cut.weapon = weapon; cut.origin = origin;
            cut.impact=false;
            cut.forward = end - origin; cut.forward.y = 0; cut.forward.Normalize();
            if (cut.forward.sqrMagnitude < .01f) cut.forward = Vector3.forward;
            cut.radius = radius; cut.born = Time.time; cut.life = life; cut.active = true; DrawCut(cut, 0);
        }
        public void Impact(RunWeapon weapon,Vector3 point,float radius)
        {
            var c=cuts[cutCursor++%Cuts];c.weapon=weapon;c.origin=point+Vector3.up*.025f;c.radius=radius;c.born=Time.time;c.life=.24f;c.active=true;c.impact=true;DrawImpact(c,0);
        }
        public void FeedFlame(Transform weapon, float length)
        {
            Nozzle nozzle = null;
            foreach (var n in nozzles) if (n.root == weapon) { nozzle = n; break; }
            if (nozzle == null) foreach (var n in nozzles) if (!n.root || n.until <= Time.time) { nozzle = n; break; }
            if (nozzle == null) return;
            if (nozzle.root != weapon || nozzle.until < Time.time) nozzle.next = Time.time;
            nozzle.root = weapon; nozzle.until = Time.time + .32f; nozzle.length = length;
        }
        void LateUpdate()
        {
            if (!run || run.Phase != SurvivalRun.RunPhase.Wave) { Clear(); return; }
            if (run.IsPaused) return;
            ActiveCuts = ActiveFlames = 0;
            foreach (var cut in cuts)
            {
                if (!cut.active) continue; float age = (Time.time - cut.born) / cut.life;
                if (age >= 1) { cut.active = false; cut.renderer.enabled = false; } else { if(cut.impact)DrawImpact(cut,age);else DrawCut(cut, age); ActiveCuts++; }
            }
            foreach (var n in nozzles)
            {
                if (!n.root || !n.root.gameObject.activeInHierarchy || n.until < Time.time || Time.time < n.next) continue;
                // One emission per rendered frame at most. No burst catch-up after a hitch.
                n.next = Time.time + .014f;
                var origin = n.root.TransformPoint(new Vector3(0, .015f, .70f));
                if (!CombatSight.Clear(run.Player.transform.position + Vector3.up * .85f, origin)) continue;
                var direction = n.root.forward; direction.y = 0; direction.Normalize();
                float length = Mathf.Max(0,CombatSight.DistanceToCover(origin, direction, n.length)-.04f);
                if (length < .15f) continue;
                var f = flames[flameCursor++ % Tongues]; f.origin = origin; f.direction = direction;
                f.length = length; f.born = Time.time; f.life = .27f; f.width = Random.Range(.9f, 1.1f); f.spin = Random.Range(-16, 16);f.frame=Random.Range(0,8);f.active = true;
            }
            foreach (var f in flames)
            {
                if (!f.active) continue; float age = (Time.time - f.born) / f.life;
                if (age >= 1) { f.active = false; f.renderer.enabled = false; continue; }
                ActiveFlames++; var side = Vector3.Cross(Vector3.up, f.direction);
                float spread = f.spin / 16 * .28f * age;
                f.root.position = f.origin + f.direction * (age * f.length) + side * spread + Vector3.up * (.08f * age);
                // Flame tongues grow away from the muzzle, then pinch off at the tip.
                float size = Mathf.Lerp(.28f, 1.25f, age) * f.width;
                f.root.localScale = new Vector3(size, size * 1.22f, size);
                f.filter.sharedMesh=flameFrames[(f.frame+(int)(age*8))%8];
                var forward = view ? view.transform.forward : Vector3.forward;
                var up = view ? view.transform.up : Vector3.up;
                var right = view ? view.transform.right : Vector3.right;
                float angle = -Mathf.Atan2(Vector3.Dot(f.direction, right), Vector3.Dot(f.direction, up)) * Mathf.Rad2Deg;
                f.root.rotation = Quaternion.LookRotation(forward, up) * Quaternion.Euler(0, 0, angle + f.spin);
                tint.SetColor("_Tint", new Color(1, 1, 1, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.72f, 1, age))));
                f.renderer.SetPropertyBlock(tint); f.renderer.enabled = true;
            }
        }
        void DrawCut(Cut c, float age)
        {
            var toLocal=c.renderer.transform.worldToLocalMatrix;
            var side = Vector3.Cross(Vector3.up, c.forward);
            float lead = Mathf.Lerp(18, 48, 1 - Mathf.Pow(1 - age, 3));
            float tail = Mathf.Lerp(64, 16, age), fade = 1 - age * age;
            if (c.weapon == RunWeapon.Saw) { lead = age * 270; tail = 105; }
            for (int i = 0; i <= Segments; i++)
            {
                float t = i / (float)Segments;
                float angle = (lead - tail + tail * t) * Mathf.Deg2Rad;
                var radial = c.forward * Mathf.Cos(angle) + side * Mathf.Sin(angle);
                float width = Mathf.Pow(Mathf.Max(0,Mathf.Sin(t * Mathf.PI)), .7f) * (c.weapon == RunWeapon.Axe ? .42f : .27f) * (1 - age * .65f);
                for (int band = 0; band < 3; band++)
                {
                    int v = i * 3 + band;
                    c.vertices[v] = toLocal.MultiplyPoint3x4(c.origin + radial * (c.radius - width * (1 - band * .5f)));
                    var color = band == 2 ? new Color(1, .97f, .83f) : band == 1 ? new Color(.98f, .85f, .57f) : new Color(.66f, .47f, .24f);
                    color.a = fade * (band == 0 ? .08f : 1); c.colors[v] = color;
                }
            }
            c.mesh.vertices = c.vertices; c.mesh.colors = c.colors; c.mesh.RecalculateBounds(); c.renderer.enabled = true;
        }
        void DrawImpact(Cut c,float age)
        {
            var toLocal=c.renderer.transform.worldToLocalMatrix;
            bool frost=c.weapon==RunWeapon.Frost;float radius=c.radius*Mathf.Lerp(.12f,1,1-Mathf.Pow(1-age,3));
            float width=(frost?.38f:.54f)*(1-age),fade=(1-age)*(1-age);
            for(int i=0;i<=Segments;i++){
                float a=i*Mathf.PI*2/Segments;var radial=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                float rough=frost?(i%2==0?1:.92f):1+.025f*Mathf.Sin(i*2.7f);
                for(int band=0;band<3;band++){
                    int v=i*3+band;c.vertices[v]=toLocal.MultiplyPoint3x4(c.origin+radial*Mathf.Max(0,radius*rough-width*(1-band*.5f)));
                    var color=frost?(band==2?new Color(.84f,.97f,1):new Color(.33f,.73f,.85f)):(band==2?new Color(1,.89f,.58f):new Color(.80f,.50f,.25f));
                    color.a=fade*(band==0?0:band==1?.5f:.8f);c.colors[v]=color;
                }
            }
            c.mesh.vertices=c.vertices;c.mesh.colors=c.colors;c.mesh.RecalculateBounds();c.renderer.enabled=true;
        }
        static Mesh MakeFlameFrame(int frame)
        {
            float x=(frame%4)*.25f,y=(1-frame/4)*.5f;
            var mesh=new Mesh{name="Painted fire frame "+frame};
            mesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
            mesh.uv=new[]{new Vector2(x+.001f,y+.002f),new Vector2(x+.249f,y+.002f),new Vector2(x+.249f,y+.498f),new Vector2(x+.001f,y+.498f)};
            mesh.colors=new[]{Color.white,Color.white,Color.white,Color.white};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();return mesh;
        }
        public void Clear()
        {
            foreach (var c in cuts) if(c != null) { c.active = false; c.renderer.enabled = false; }
            foreach (var f in flames) if(f != null) { f.active = false; f.renderer.enabled = false; }
            foreach (var n in nozzles) if(n != null) { n.root = null; n.until = 0; }
            ActiveCuts = ActiveFlames = 0;
        }
        void OnDisable() { Clear(); }
        void OnDestroy() { foreach (var c in cuts) if (c != null && c.mesh) Destroy(c.mesh); foreach(var mesh in flameFrames)if(mesh)Destroy(mesh);if(flameInk)Destroy(flameInk);if(ink)Destroy(ink); }
    }
}
