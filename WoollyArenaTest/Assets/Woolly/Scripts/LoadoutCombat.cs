using UnityEngine;

namespace WoollyArena
{
    // Six independent attack clocks; ranged tools and magic cores share the same inventory.
    public sealed partial class LoadoutCombat : MonoBehaviour
    {
        public int Shots { get; private set; }
        readonly Transform[] tools = new Transform[6];
        readonly float[] recoil = new float[6];
        readonly Vector3[] muzzles = new Vector3[6];
        readonly Material[] palette = new Material[6];
        Material metal, wood, brass;
        readonly int[] modelKinds = { -1,-1,-1,-1,-1,-1 };
        SurvivalRun run; ArenaPlayer player; int revision = -1;
        public void Initialize(SurvivalRun owner, ArenaPlayer hero)
        {
            run = owner; player = hero;
            var shader = Resources.Load<Shader>("CombatModel");
            metal = new Material(shader) { color = new Color(.18f,.23f,.28f) };
            wood = new Material(shader) { color = new Color(.35f,.16f,.08f) };
            brass = new Material(shader) { color = new Color(.92f,.65f,.22f) };
            arsenalFX = gameObject.AddComponent<ArsenalVFX>(); arsenalFX.Initialize(run);
            foreach(var renderer in player.visual.GetComponentsInChildren<Renderer>())
                if(renderer.name.ToLowerInvariant().Contains("revolver") || renderer.name.ToLowerInvariant().Contains("weapon") || renderer.name.ToLowerInvariant().Contains("gun")) renderer.enabled=false;
            if(player.animator.layerCount > 1) player.animator.SetLayerWeight(1,0);
            for (int i = 0; i < 6; i++)
            {
                palette[i] = new Material(shader);
                palette[i].SetColor("_BaseColor", i < 3 ? new Color(.2f + i * .18f, .4f, .53f) : CartoonPowerVFX.ColorFor(i - 3));
                tools[i] = new GameObject("Equipped weapon " + i).transform; tools[i].SetParent(player.transform, false);
                cycles[i] = new WeaponAttackCycle();
                tools[i].gameObject.SetActive(false);
            }
        }
        void Model(int slot, int kind)
        {
            var parent=tools[slot];
            foreach(Transform child in parent) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            Transform Part(PrimitiveType type, Vector3 pos, Vector3 scale, Material mat) => CreatureModel.Part(parent,type,pos,scale,mat);
            string[] names={"FrontierRevolver","CopperRepeater","TwinBarrel","GalaxyRelic","ArcCoil","StarLance","CrescentBlade","ThornSpear","RailRifle","EmberThrower","FrostOrb","GrenadeLauncher","CleaverAxe","QuakeHammer","HarpoonCrossbow","BurstCarbine","RicochetPistol","ReturnBlade","RotorSaw","SiegeMortar"};
            if(CraftedModel.Attach(names[kind],parent))return;
            if(kind>=6){ArsenalModels.Build(parent,(RunWeapon)kind,metal,wood,brass,palette[slot]);return;}
            if(kind < 3) {
                Part(PrimitiveType.Cube,new Vector3(0,0,.03f),new Vector3(.18f,.18f,.32f),metal);
                var grip=Part(PrimitiveType.Cube,new Vector3(0,-.16f,-.08f),new Vector3(.13f,.26f,.13f),wood);grip.localRotation=Quaternion.Euler(-18,0,0);
                int barrels=kind==2?2:1;
                for(int i=0;i<barrels;i++){var barrel=Part(PrimitiveType.Cylinder,new Vector3((i-(barrels-1)*.5f)*.12f,.025f,.29f),new Vector3(.085f,kind==1?.27f:.20f,.085f),metal);barrel.localRotation=Quaternion.Euler(90,0,0);}
                var chamber=Part(PrimitiveType.Cylinder,new Vector3(0,.02f,.02f),new Vector3(.23f,.1f,.23f),brass);chamber.localRotation=Quaternion.Euler(90,0,0);
                if(kind==1){Part(PrimitiveType.Cube,new Vector3(0,-.05f,-.27f),new Vector3(.14f,.19f,.32f),wood);Part(PrimitiveType.Cube,new Vector3(0,.14f,.13f),new Vector3(.05f,.09f,.11f),brass);}
                if(kind==2)Part(PrimitiveType.Cube,new Vector3(0,-.065f,.2f),new Vector3(.23f,.13f,.23f),wood);
            } else {
                Part(PrimitiveType.Sphere,Vector3.zero,Vector3.one*.28f,palette[slot]);
                for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;var fin=Part(PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.22f,0),new Vector3(.12f,.12f,kind==4?.4f:.2f),brass);fin.localRotation=Quaternion.Euler(0,0,i*90+45);}
                if(kind==5)Part(PrimitiveType.Cube,new Vector3(0,0,.25f),new Vector3(.13f,.13f,.35f),palette[slot]);
            }
        }
        public Vector3 PowerOrigin(int kind) {
            for(int i=0;i<run.Build.Weapons.Count;i++)if((int)run.Build.Weapons[i].Item.Weapon.Value==kind+3)return tools[i].position;
            return player.transform.position+Vector3.up;
        }
        void Update()
        {
            if (!run || run.Build == null) return;
            var build = run.Build;
            if(run.IsPaused)return;
            if (revision != build.Revision)
            {
                revision = build.Revision;
                for (int i = 0; i < 6; i++)
                {
                    tools[i].gameObject.SetActive(i < build.Weapons.Count);
                    cycles[i].Cancel();
                    heat[i].Reset();
                    if (i >= build.Weapons.Count) continue;
                    int kind=(int)build.Weapons[i].Item.Weapon.Value;
                    palette[i].SetColor("_BaseColor",WeaponArsenal.ColorFor((RunWeapon)kind));
                    if(modelKinds[i]!=kind){Model(i,kind);modelKinds[i]=kind;}
                    tools[i].localScale = Vector3.one * (1 + (build.Weapons[i].Tier - 1) * .13f);
                }
            }
            bool fighting = run.Phase == SurvivalRun.RunPhase.Wave && !player.CombatPaused && player.GetComponent<CharacterVitals>().Health > 0;
            if(!fighting)CancelAttacks();
            var target = fighting ? player.FindTarget() : null;
            for (int i = 0; i < build.Weapons.Count; i++)
            {
                float angle = i * Mathf.PI * 2 / Mathf.Max(1, build.Weapons.Count) + Time.time * .18f;
                tools[i].localPosition = new Vector3(Mathf.Cos(angle) * .95f, .9f + Mathf.Sin(Time.time * 2 + i) * .06f, Mathf.Sin(angle) * .95f);
                var gear = build.Weapons[i]; var kind = gear.Item.Weapon.Value;
                var profile = WeaponArsenal.Profile(kind);
                var point = cycles[i].Pending ? lockedPoints[i] : target ? target.transform.position + Vector3.up * target.AimHeight : player.transform.position + player.visual.forward * 5 + Vector3.up;
                var facing=point-tools[i].position;
                if(profile.Motion==WeaponMotion.Sweep||profile.Motion==WeaponMotion.Spray)facing.y=0;
                tools[i].rotation = Quaternion.LookRotation(facing.sqrMagnitude>.001f?facing:player.visual.forward);
                recoil[i]=Mathf.MoveTowards(recoil[i],0,Time.deltaTime*1.2f);
                tools[i].position-=tools[i].forward*recoil[i];
                AnimateWeapon(i,profile);
                if(kind==RunWeapon.Repeater&&heat[i].Cooling(Time.time)>0)tools[i].rotation*=Quaternion.Euler(-35,0,Mathf.Sin(Time.time*18)*4);
                muzzles[i] = tools[i].TransformPoint(new Vector3(0,0,kind==RunWeapon.Flame?.70f:modelKinds[i]==1?.65f:.5f));
                if (!fighting || player.SimulatedInput || WeaponArsenal.IsLegacyPower(kind)) continue;
                TickFollowups(i,gear);
                if (!cycles[i].Pending && followups[i]==0 && (kind!=RunWeapon.Repeater||heat[i].Ready(Time.time)) && target && Vector3.Distance(player.transform.position, target.transform.position) <= profile.Range
                    && ClearRoute(player.transform.position+Vector3.up*.85f,point)
                    && cycles[i].Begin(Time.time,profile,player.Stats.AttackMultiplier*build.WeaponAttackMultiplier(kind))) BeginAttack(i,gear,point);
                if (cycles[i].Resolve(Time.time)) ResolveAttack(i,gear);
            }
        }
        void AnimateWeapon(int slot,WeaponProfile profile)
        {
            float age=Time.time-cycles[slot].StartedAt;
            float windup=cycles[slot].ImpactAt-cycles[slot].StartedAt;
            float recovery=Mathf.Clamp01(1-(age-windup)/.23f);
            float charge=cycles[slot].Pending?Mathf.Clamp01(age/Mathf.Max(.01f,windup)):0;
            var tool=tools[slot];
            switch(profile.Motion)
            {
                case WeaponMotion.Sweep:
                    if(age<windup+.23f){float swing=cycles[slot].Pending?-65*charge:Mathf.Lerp(80,0,1-recovery);tool.rotation*=Quaternion.Euler(0,swing,-15*recovery);tool.position+=tool.forward*(cycles[slot].Pending?.15f:.6f*recovery);}break;
                case WeaponMotion.Thrust:
                    if(age<windup+.23f)tool.position+=tool.forward*(cycles[slot].Pending?-.28f*charge:.85f*recovery);break;
                case WeaponMotion.Charge:
                    tool.rotation*=Quaternion.Euler(-charge*12,0,modelKinds[slot]==(int)RunWeapon.Frost?Time.time*65+charge*40:0);tool.position+=Vector3.up*charge*.12f;break;
                case WeaponMotion.Lob:
                    if(age<windup+.23f)tool.rotation*=Quaternion.Euler(-Mathf.Sin(Mathf.Clamp01(age/Mathf.Max(.01f,windup+.23f))*Mathf.PI)*38,0,0);break;
                case WeaponMotion.Spray:
                    tool.rotation*=Quaternion.Euler(0,Mathf.Sin(Time.time*28)*recoil[slot]*24,0);break;
                default:tool.rotation*=Quaternion.Euler(-recoil[slot]*95,0,0);break;
            }
            if(modelKinds[slot]==(int)RunWeapon.Saw)tool.rotation*=Quaternion.Euler(0,Time.time*950,0);
            if(modelKinds[slot]==(int)RunWeapon.Boomerang&&age>=0&&age<windup+.35f){float u=age/Mathf.Max(.01f,windup+.35f);tool.position=Vector3.Lerp(player.transform.position+Vector3.up*.8f,lockedPoints[slot],Mathf.Sin(u*Mathf.PI));tool.rotation*=Quaternion.Euler(0,age*1100,0);}
        }
        void OnDisable(){CancelAttacks();}
        void OnDestroy() { Destroy(metal); Destroy(wood); Destroy(brass); foreach (var material in palette) if (material) Destroy(material); }
    }
}
