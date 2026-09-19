using UnityEngine;
namespace WoollyArena
{
    // Bounded, reusable strokes: swing arcs, thrusts, charge rails, flame fans, frost and lob trails.
    public sealed class ArsenalVFX : MonoBehaviour
    {
        const int Capacity = 48;
        sealed class Stroke
        {
            public LineRenderer line, echo;
            public RunWeapon weapon;
            public Vector3 origin, end;
            public float born, duration, radius;
            public bool active;
        }
        readonly Stroke[] pool = new Stroke[Capacity];
        Material material;
        Material particleMaterial;
        ParticleSystem billows;
        SurvivalRun run;
        CartoonWeaponFX cartoon;
        int cursor;
        public void Initialize(SurvivalRun owner)
        {
            run = owner; material = new Material(Resources.Load<Shader>("PowerInk")); material.SetColor("_Tint", Color.white);
            cartoon=gameObject.AddComponent<CartoonWeaponFX>();cartoon.Initialize(owner);
            for (int i = 0; i < Capacity; i++) pool[i] = new Stroke { line = Line("Weapon stroke"), echo = Line("Weapon echo") };
            particleMaterial=new Material(Resources.Load<Shader>("CombatParticle"));particleMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("VFX/Smoke"));
            var go=new GameObject("Pooled elemental billows");go.transform.SetParent(transform,false);billows=go.AddComponent<ParticleSystem>();billows.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=billows.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=192;main.startSpeed=0;
            var emission=billows.emission;emission.enabled=false;var shape=billows.shape;shape.enabled=false;
            var size=billows.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.4f),new Keyframe(.35f,1),new Keyframe(1,1.5f)));
            var color=billows.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(1,.55f,.3f),1)},new[]{new GradientAlphaKey(.9f,0),new GradientAlphaKey(.75f,.35f),new GradientAlphaKey(0,1)});color.color=gradient;
            var renderer=billows.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.maxParticleSize=.035f;billows.Play();
        }
        LineRenderer Line(string label)
        {
            var obj = new GameObject(label); obj.transform.SetParent(transform, false);
            var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = material;
            line.useWorldSpace = true; line.numCapVertices = 3; line.positionCount = 25;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false; line.enabled = false; return line;
        }
        public void Play(RunWeapon weapon, Vector3 origin, Vector3 end, float radius = 1, float duration = .3f, bool impact = false)
        {
            if (!run || run.IsPaused || run.Phase != SurvivalRun.RunPhase.Wave) return;
            if(weapon==RunWeapon.Flame)return;
            if(!impact&&(weapon==RunWeapon.Axe||weapon==RunWeapon.Blade||weapon==RunWeapon.Saw)){
                cartoon.Slash(weapon,origin,end,Mathf.Min(radius,weapon==RunWeapon.Saw?1.45f:1.95f),weapon==RunWeapon.Saw?.11f:.14f);return;
            }
            if(impact){
                cartoon.Impact(weapon,end,radius);
                if(weapon!=RunWeapon.Frost)for(int i=0;i<9;i++){float angle=i*Mathf.PI*2/9;var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));billows.Emit(new ParticleSystem.EmitParams{position=end+Vector3.up*.12f+radial*.12f,velocity=radial*radius*2+Vector3.up*.5f,startSize=radius*.26f,startLifetime=.3f,startColor=new Color(.7f,.5f,.3f,.5f),rotation=Random.Range(0,360)},1);}
                return;
            }
            var e = pool[cursor]; cursor = (cursor + 1) % Capacity;
            e.weapon = weapon; e.origin = origin; e.end = end; e.radius = radius;
            e.duration = weapon==RunWeapon.Railgun||weapon==RunWeapon.Ricochet?.10f:Mathf.Max(.06f, duration); e.born = Time.time; e.active = true;
            Draw(e, 0);
        }
        public void Spray(Transform weapon,float range){if(run&&!run.IsPaused&&run.Phase==SurvivalRun.RunPhase.Wave)cartoon.FeedFlame(weapon,range);}
        void Update()
        {
            if (!run || run.Phase != SurvivalRun.RunPhase.Wave) { Clear(); return; }
            if (run.IsPaused) return;
            foreach (var e in pool)
            {
                if (!e.active) continue;
                float u = (Time.time - e.born) / e.duration;
                if (u >= 1) { e.active = false; e.line.enabled = e.echo.enabled = false; }
                else Draw(e, u);
            }
        }
        static void Style(LineRenderer line, Color tint, float width, float alpha)
        {
            tint.a = alpha; line.startColor = line.endColor = tint;
            line.startWidth = width; line.endWidth = width * .4f;
            line.enabled = true; line.loop = false; line.positionCount = 25;
        }
        void Draw(Stroke e, float u)
        {
            var color = WeaponArsenal.ColorFor(e.weapon);
            float width=e.weapon==RunWeapon.Railgun?.13f:.08f;
            Style(e.line, color, width * (1 - u * .7f), 1 - u*u);
            Style(e.echo, Color.Lerp(color, Color.white, .85f), .024f, (1 - u) * .9f);
            var forward = e.end - e.origin; forward.y = 0;
            forward = forward.sqrMagnitude > .001f ? forward.normalized : Vector3.forward;
            for (int i = 0; i < 25; i++)
            {
                float t = i / 24f;
                Vector3 a, b;
                if (e.weapon == RunWeapon.Grenade||e.weapon==RunWeapon.Mortar||e.weapon==RunWeapon.Boomerang)
                {
                    float travel = Mathf.Clamp01(u - (1 - t) * .18f);
                    a = Vector3.Lerp(e.origin, e.end, travel) + Vector3.up * (Mathf.Sin(travel * Mathf.PI) * (e.weapon==RunWeapon.Boomerang?.1f:e.weapon==RunWeapon.Mortar?3.4f:2.2f));
                    b = a + Vector3.up * .045f;
                }
                else
                {
                    float along=t;
                    if(e.weapon==RunWeapon.Spear||e.weapon==RunWeapon.Crossbow){float head=Mathf.Min(1,u*2.4f+.12f);along=Mathf.Lerp(Mathf.Max(0,head-.22f),head,t);}
                    a = Vector3.Lerp(e.origin, e.end, along);
                    b = a;
                }
                e.line.SetPosition(i, a); e.echo.SetPosition(i, b);
            }
        }
        public void Clear() { foreach (var e in pool) if (e != null) { e.active = false; e.line.enabled = e.echo.enabled = false; } if(billows)billows.Clear(); if(cartoon)cartoon.Clear(); }
        void OnDisable() { Clear(); }
        void OnDestroy() { if (material) Destroy(material); if(particleMaterial)Destroy(particleMaterial); }
    }
}
