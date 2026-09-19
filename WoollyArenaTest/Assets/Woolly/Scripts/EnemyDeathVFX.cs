using UnityEngine;
namespace WoollyArena
{
    // Shared bone meshes and a fixed debris pool replace spawning rigidbodies on every kill.
    public sealed class EnemyDeathVFX : MonoBehaviour
    {
        const int Capacity=144;
        sealed class Fragment { public Transform root; public MeshFilter mesh; public Renderer renderer; public Vector3 velocity,spin; public float age,life; public int bounces; public Vector3 size; public bool active; }
        readonly Fragment[] fragments=new Fragment[Capacity];
        readonly RaycastHit[] hits=new RaycastHit[8];
        Mesh bone,skull,ribs,shard;
        Material ivory,dark,ember,dustMaterial;
        ParticleSystem dust;
        SurvivalRun run;int cursor;
        public int ActiveCount {get;private set;}
        public const int MaximumFragments=Capacity;
        public void Initialize(SurvivalRun owner)
        {
            run=owner;var shader=Resources.Load<Shader>("CombatModel");
            ivory=new Material(shader){color=new Color(.86f,.82f,.63f)};dark=new Material(shader){color=new Color(.25f,.28f,.35f)};ember=new Material(shader){color=new Color(1,.42f,.13f)};
            dustMaterial=new Material(Resources.Load<Shader>("CombatParticle"));dustMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("VFX/Smoke"));
            var cloud=new GameObject("Pooled defeat dust");cloud.transform.SetParent(transform,false);dust=cloud.AddComponent<ParticleSystem>();dust.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=dust.main;main.loop=true;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=96;main.startSpeed=0;
            var emission=dust.emission;emission.enabled=false;var shape=dust.shape;shape.enabled=false;
            var size=dust.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(.25f,1),new Keyframe(1,1.4f)));
            var fade=dust.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.75f,0),new GradientAlphaKey(.45f,.25f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var cloudRenderer=dust.GetComponent<ParticleSystemRenderer>();cloudRenderer.sharedMaterial=dustMaterial;cloudRenderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;cloudRenderer.maxParticleSize=.035f;dust.Play();
            var cube=Primitive(PrimitiveType.Cube);var sphere=Primitive(PrimitiveType.Sphere);
            bone=Combine(sphere,new[]{new Vector3(0,-.32f,0),new Vector3(0,.32f,0)},new[]{new Vector3(.30f,.28f,.3f),new Vector3(.30f,.28f,.3f)},cube,Vector3.zero,new Vector3(.15f,.65f,.15f));
            skull=Combine(cube,new[]{new Vector3(0,.06f,0),new Vector3(0,-.2f,.12f)},new[]{new Vector3(.62f,.48f,.45f),new Vector3(.4f,.12f,.35f)},sphere,new Vector3(0,.18f,-.03f),new Vector3(.6f,.4f,.45f));
            ribs=Combine(cube,new[]{new Vector3(0,-.20f,0),Vector3.zero,new Vector3(0,.2f,0)},new[]{new Vector3(.7f,.10f,.30f),new Vector3(.8f,.1f,.35f),new Vector3(.65f,.1f,.28f)},cube,Vector3.zero,new Vector3(.12f,.6f,.15f));
            shard=Combine(cube,new[]{Vector3.zero},new[]{new Vector3(.7f,.12f,.45f)},cube,new Vector3(.08f,.08f,0),new Vector3(.2f,.18f,.3f));
            for(int i=0;i<Capacity;i++){
                var go=new GameObject("Pooled skeleton fragment",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);
                var renderer=go.GetComponent<Renderer>();renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                fragments[i]=new Fragment{root=go.transform,mesh=go.GetComponent<MeshFilter>(),renderer=renderer};go.SetActive(false);
            }
        }
        static Mesh Primitive(PrimitiveType type){var go=GameObject.CreatePrimitive(type);var mesh=go.GetComponent<MeshFilter>().sharedMesh;go.SetActive(false);Destroy(go);return mesh;}
        static Mesh Combine(Mesh first,Vector3[] at,Vector3[] sizes,Mesh last,Vector3 lastAt,Vector3 lastSize){var parts=new CombineInstance[at.Length+1];for(int i=0;i<at.Length;i++)parts[i]=new CombineInstance{mesh=first,transform=Matrix4x4.TRS(at[i],Quaternion.identity,sizes[i])};parts[at.Length]=new CombineInstance{mesh=last,transform=Matrix4x4.TRS(lastAt,Quaternion.identity,lastSize)};var result=new Mesh();result.CombineMeshes(parts);return result;}
        public void BreakApart(EnemyAgent enemy,Vector3 incoming,bool critical)
        {
            incoming.y=0;if(incoming.sqrMagnitude<.01f)incoming=Vector3.forward;incoming.Normalize();
            var side=Vector3.Cross(Vector3.up,incoming);var point=enemy.transform.position;
            for(int i=0;i<6;i++){
                float angle=i*Mathf.PI/3;var radial=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                dust.Emit(new ParticleSystem.EmitParams{position=point+Vector3.up*.4f+radial*.15f,velocity=radial*1.1f+incoming*.6f+Vector3.up*.35f,startSize=enemy.IsBoss?.85f:.48f,startLifetime=.42f,startColor=new Color(.76f,.69f,.53f,.75f),rotation=Random.Range(0,360)},1);
            }
            float scale=enemy.IsBoss?1.4f:1;int count=enemy.IsBoss?12:7;
            for(int i=0;i<count;i++){
                var f=fragments[cursor];cursor=(cursor+1)%Capacity;
                bool head=i==0,torso=i==1;f.mesh.sharedMesh=head?skull:torso?ribs:i>5?shard:bone;
                f.renderer.sharedMaterial=enemy.IsElite&&i>5?ember:i>5?dark:ivory;
                f.root.position=point+Vector3.up*(head?1.0f:torso?.65f:.38f)*scale+side*((i%2==0?1:-1)*.18f*scale);
                f.size=Vector3.one*(head?.62f:torso?.60f:.45f)*scale;f.root.localScale=f.size;
                f.root.rotation=Quaternion.Euler(Random.Range(-25,25),Random.Range(0,360),Random.Range(-35,35));
                f.velocity=incoming*(critical?3.1f:2.1f)+side*Random.Range(-1.7f,1.7f)+Vector3.up*Random.Range(2.6f,4.4f);
                f.spin=new Vector3(Random.Range(160,350),Random.Range(-300,300),Random.Range(-260,260));
                f.age=0;f.life=Random.Range(1.55f,2.1f);f.bounces=0;f.active=true;f.root.gameObject.SetActive(true);
            }
            run.ActionFX.Burst(point+Vector3.up*.6f,enemy.IsElite?new Color(1,.5f,.15f):new Color(.9f,.87f,.7f),critical?16:9);
            // The original body vanishes on the same frame its separate pieces take over.
            enemy.HideDefeatedBody();
        }
        void Update()
        {
            if(!run||run.IsPaused)return;ActiveCount=0;
            foreach(var f in fragments){
                if(!f.active)continue;f.age+=Time.deltaTime;
                if(f.age>=f.life){f.active=false;f.root.gameObject.SetActive(false);continue;}ActiveCount++;
                if(f.bounces<2){
                    f.velocity+=Vector3.down*12*Time.deltaTime;var travel=f.velocity*Time.deltaTime;float length=travel.magnitude;
                    if(length>.001f){int count=Physics.RaycastNonAlloc(f.root.position,travel/length,hits,length+.04f,1,QueryTriggerInteraction.Ignore);float nearest=length;Vector3 normal=Vector3.up;bool hit=false;for(int i=0;i<count;i++)if(hits[i].distance<nearest){nearest=hits[i].distance;normal=hits[i].normal;hit=true;}f.root.position+=travel.normalized*Mathf.Max(0,nearest-.02f);if(hit){f.velocity=Vector3.Reflect(f.velocity,normal)*.35f;f.bounces++;}}
                    if(f.root.position.y<.065f){var at=f.root.position;at.y=.065f;f.root.position=at;f.velocity=new Vector3(f.velocity.x*.5f,Mathf.Abs(f.velocity.y)*.3f,f.velocity.z*.5f);f.bounces++;}
                    f.root.Rotate(f.spin*Time.deltaTime,Space.World);
                }
                float fade=Mathf.Clamp01((f.life-f.age)/.35f);f.root.localScale=f.size*fade;
            }
        }
        void OnDestroy(){if(bone)Destroy(bone);if(skull)Destroy(skull);if(ribs)Destroy(ribs);if(shard)Destroy(shard);if(ivory)Destroy(ivory);if(dark)Destroy(dark);if(ember)Destroy(ember);if(dustMaterial)Destroy(dustMaterial);}
    }
}
