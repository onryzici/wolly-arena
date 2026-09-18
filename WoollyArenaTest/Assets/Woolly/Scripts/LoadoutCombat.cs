using UnityEngine;

namespace WoollyArena
{
    // Six independent attack clocks; ranged tools and magic cores share the same inventory.
    public sealed class LoadoutCombat : MonoBehaviour
    {
        public int Shots { get; private set; }
        readonly float[] nextAttack = new float[6];
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
            foreach(var renderer in player.visual.GetComponentsInChildren<Renderer>())
                if(renderer.name.ToLowerInvariant().Contains("revolver") || renderer.name.ToLowerInvariant().Contains("weapon") || renderer.name.ToLowerInvariant().Contains("gun")) renderer.enabled=false;
            if(player.animator.layerCount > 1) player.animator.SetLayerWeight(1,0);
            for (int i = 0; i < 6; i++)
            {
                palette[i] = new Material(shader);
                palette[i].SetColor("_BaseColor", i < 3 ? new Color(.2f + i * .18f, .4f, .53f) : CartoonPowerVFX.ColorFor(i - 3));
                tools[i] = new GameObject("Equipped weapon " + i).transform; tools[i].SetParent(player.transform, false);

                tools[i].gameObject.SetActive(false);
            }
        }
        void Model(int slot, int kind)
        {
            var parent=tools[slot];
            foreach(Transform child in parent) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            Transform Part(PrimitiveType type, Vector3 pos, Vector3 scale, Material mat) => CreatureModel.Part(parent,type,pos,scale,mat);
            string[] names={"FrontierRevolver","CopperRepeater","TwinBarrel","GalaxyRelic","ArcCoil","StarLance"};
            if(CraftedModel.Attach(names[kind],parent))return;
            if(kind < 3) {
                Part(PrimitiveType.Cube,new Vector3(0,0,.03f),new Vector3(.18f,.18f,.32f),metal);
                var grip=Part(PrimitiveType.Cube,new Vector3(0,-.16f,-.08f),new Vector3(.13f,.26f,.13f),wood);grip.localRotation=Quaternion.Euler(-18,0,0);
                int barrels=kind==2?2:1;
                for(int i=0;i<barrels;i++){var barrel=Part(PrimitiveType.Cylinder,new Vector3((i-(barrels-1)*.5f)*.12f,.025f,.29f),new Vector3(.085f,kind==1?.27f:.20f,.085f),metal);barrel.localRotation=Quaternion.Euler(90,0,0);}
                var chamber=Part(PrimitiveType.Cylinder,new Vector3(0,.02f,.02f),new Vector3(.23f,.1f,.23f),brass);chamber.localRotation=Quaternion.Euler(90,0,0);
                if(kind==1){Part(PrimitiveType.Cube,new Vector3(0,-.05f,-.27f),new Vector3(.14f,.19f,.32f),wood);Part(PrimitiveType.Cube,new Vector3(0,.14f,.13f),new Vector3(.05f,.09f,.11f),brass);}
                if(kind==2)Part(PrimitiveType.Cube,new Vector3(0,-.065f,.2f),new Vector3(.23f,.13f,.23f),wood);
            } else {
                Part(PrimitiveType.Sphere,Vector3.zero,Vector3.one*.28f,palette[kind]);
                for(int i=0;i<4;i++){float a=i*Mathf.PI*.5f;var fin=Part(PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.22f,Mathf.Sin(a)*.22f,0),new Vector3(.12f,.12f,kind==4?.4f:.2f),brass);fin.localRotation=Quaternion.Euler(0,0,i*90+45);}
                if(kind==5)Part(PrimitiveType.Cube,new Vector3(0,0,.25f),new Vector3(.13f,.13f,.35f),palette[kind]);
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
            if (revision != build.Revision)
            {
                revision = build.Revision;
                for (int i = 0; i < 6; i++)
                {
                    tools[i].gameObject.SetActive(i < build.Weapons.Count);
                    if (i >= build.Weapons.Count) continue;
                    int kind=(int)build.Weapons[i].Item.Weapon.Value;
                    if(modelKinds[i]!=kind){Model(i,kind);modelKinds[i]=kind;}
                    tools[i].localScale = Vector3.one * (1 + (build.Weapons[i].Tier - 1) * .13f);
                    nextAttack[i] = 0;
                }
            }
            bool fighting = run.Phase == SurvivalRun.RunPhase.Wave && !player.CombatPaused && player.GetComponent<CharacterVitals>().Health > 0;
            var target = fighting ? player.FindTarget() : null;
            for (int i = 0; i < build.Weapons.Count; i++)
            {
                float angle = i * Mathf.PI * 2 / Mathf.Max(1, build.Weapons.Count) + Time.time * .18f;
                tools[i].localPosition = new Vector3(Mathf.Cos(angle) * .95f, .9f + Mathf.Sin(Time.time * 2 + i) * .06f, Mathf.Sin(angle) * .95f);
                var point = target ? target.transform.position + Vector3.up * target.AimHeight : player.transform.position + player.visual.forward * 5 + Vector3.up;
                tools[i].rotation = Quaternion.LookRotation(point - tools[i].position);
                recoil[i]=Mathf.MoveTowards(recoil[i],0,Time.deltaTime*1.2f);
                tools[i].position-=tools[i].forward*recoil[i];
                muzzles[i] = tools[i].position + tools[i].forward * (modelKinds[i]==1?.65f:.5f);
                if (!fighting || !target || player.SimulatedInput || Time.time < nextAttack[i]) continue;
                var gear = build.Weapons[i]; var kind = gear.Item.Weapon.Value;
                if ((int)kind >= 3) continue;
                float range = kind == RunWeapon.Shotgun ? 7 : kind == RunWeapon.Repeater ? 10 : 14;
                if (Vector3.Distance(player.transform.position, target.transform.position) > range) continue;
                float interval = kind == RunWeapon.Revolver ? .4f : kind == RunWeapon.Repeater ? .14f : .65f;
                nextAttack[i] = Time.time + interval / player.Stats.AttackMultiplier;
                float damage = (kind == RunWeapon.Revolver ? 25 : kind == RunWeapon.Repeater ? 10 : 13) * RunItem.TierScale(gear.Tier);
                recoil[i]=kind==RunWeapon.Shotgun?.16f:.09f;
                run.Sfx.Play(kind==RunWeapon.Shotgun?CombatCue.Shotgun:kind==RunWeapon.Repeater?CombatCue.Repeater:CombatCue.Revolver,muzzles[i]);
                int pellets = kind == RunWeapon.Shotgun ? 3 : 1;
                for (int p = 0; p < pellets; p++)
                {
                    var direction = Quaternion.Euler(0, pellets == 1 ? 0 : (p - 1) * 9, 0) * (point - muzzles[i]).normalized;
                    player.FireEquipment(muzzles[i], direction, player.Stats.RollDamage(damage*build.WeaponDamageMultiplier(kind)), range); Shots++;
                }
            }
        }
        void OnDestroy() { Destroy(metal); Destroy(wood); Destroy(brass); foreach (var material in palette) if (material) Destroy(material); }
    }
}
