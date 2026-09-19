using UnityEngine;
namespace WoollyArena
{
    public sealed class EliteEnemy : MonoBehaviour
    {
        EnemyAgent agent; SurvivalRun run;
        readonly LockedAreaStrike strike = new LockedAreaStrike();
        Vector3 center;
        float next;
        Material material;
        LineRenderer rim, countdown;
        public void Initialize(EnemyAgent owner)
        {
            agent = owner; run = agent.director.Run; agent.IsElite = true;
            agent.vitals.ResetHealth(Mathf.RoundToInt(agent.vitals.maxHealth * 1.65f));
            agent.vitals.displayName = "KOR ELİTİ"; agent.nameLabel.gameObject.SetActive(true);
            material = new Material(Resources.Load<Shader>("PowerInk")); material.SetColor("_Tint",Color.white);
            rim = Line("Elite danger boundary", .065f); countdown = Line("Elite fuse countdown", .035f);
            // Small crown identifies the elite even between casts.
            for(int i=-1;i<=1;i++){
                var crystal=CreatureModel.Part(agent.visual,PrimitiveType.Cube,new Vector3(i*.22f,1.5f,0),new Vector3(.11f,.22f,.11f),material);
                crystal.localRotation=Quaternion.Euler(0,0,-i*24);
                var tint=new MaterialPropertyBlock();tint.SetColor("_Tint",new Color(1,.3f,.1f));crystal.GetComponent<Renderer>().SetPropertyBlock(tint);
            }
            next = Time.time + 2.5f + (agent.CombatSlot % 3) * .6f;
        }
        LineRenderer Line(string label,float width)
        {
            var go=new GameObject(label);go.transform.SetParent(transform,false);var line=go.AddComponent<LineRenderer>();
            line.sharedMaterial=material;line.useWorldSpace=true;line.positionCount=49;line.startWidth=line.endWidth=width;
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.enabled=false;return line;
        }
        void Update()
        {
            if(!run||run.IsPaused)return;
            if(!agent.isActiveAndEnabled||agent.Defeated||run.Phase!=SurvivalRun.RunPhase.Wave||run.Player.GetComponent<CharacterVitals>().Health<=0){Cancel();return;}
            if(!strike.Pending && Time.time>=next && agent.target && Vector3.Distance(transform.position,agent.target.position)<11){
                var from=transform.position+Vector3.up*.9f;var to=agent.target.position+Vector3.up*.8f;
                next=Time.time+.5f;
                if(!CombatSight.Clear(from,to))return;
                center=agent.target.position;center.y=.075f;strike.Begin(Time.time);agent.AttackWindup=true;
                run.Sfx.Play(CombatCue.Charge,transform.position);
            }
            if(!strike.Pending)return;
            float u=Mathf.Clamp01(1-(strike.ImpactAt-Time.time)/WavePressure.EliteWindup);
            DrawRing(rim,WavePressure.EliteRadius,new Color(1,.25f,.12f,.8f));
            DrawRing(countdown,WavePressure.EliteRadius*(1-u),new Color(1,.75f,.25f,.8f));
            if(!strike.Resolve(Time.time,true))return;
            agent.AttackWindup=false;rim.enabled=countdown.enabled=false;next=Time.time+WavePressure.EliteCooldown(run.Wave);
            var delta=agent.target.position-center;delta.y=0;
            if(delta.sqrMagnitude<=WavePressure.EliteRadius*WavePressure.EliteRadius
                && CombatSight.Clear(center+Vector3.up*.6f,agent.target.position+Vector3.up*.6f)){
                var health=run.Player.GetComponent<CharacterVitals>();if(health.Damage(agent.shotDamage+4,delta.normalized))health.ProtectFor(.3f);
            }
            run.ActionFX.Burst(center+Vector3.up*.3f,new Color(1,.33f,.08f),20);run.Sfx.Play(CombatCue.Power,center);
        }
        void DrawRing(LineRenderer line,float radius,Color color){line.enabled=true;line.startColor=line.endColor=color;for(int i=0;i<49;i++){float a=i*Mathf.PI/24;line.SetPosition(i,center+new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius);}}
        void Cancel(){strike.Cancel();if(agent)agent.AttackWindup=false;if(rim)rim.enabled=false;if(countdown)countdown.enabled=false;}
        void OnDisable(){Cancel();}
        void OnDestroy(){if(material)Destroy(material);}
    }
}
