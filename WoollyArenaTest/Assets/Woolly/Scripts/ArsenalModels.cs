using UnityEngine;
namespace WoollyArena
{
    // Compact silhouettes at arena scale, using the same flat-shaded palette as existing equipment.
    public static class ArsenalModels
    {
        public static void Build(Transform root, RunWeapon kind, Material metal, Material wood, Material brass, Material glow)
        {
            Transform Part(string label, PrimitiveType shape, Vector3 at, Vector3 size, Material material, Vector3 euler = default)
            {
                var p = CreatureModel.Part(root, shape, at, size, material); p.name = label; p.localRotation = Quaternion.Euler(euler); return p;
            }
            void Box(string label, Vector3 at, Vector3 size, Material mat, Vector3 euler = default) => Part(label, PrimitiveType.Cube, at, size, mat, euler);
            void Barrel(Vector3 at, float radius, float length, Material mat) => Part("Barrel", PrimitiveType.Cylinder, at, new Vector3(radius, length / 2, radius), mat, new Vector3(90, 0, 0));
            if(kind==RunWeapon.Axe||kind==RunWeapon.Hammer){
                Barrel(new Vector3(0,0,.1f),.08f,.9f,wood);Barrel(new Vector3(0,0,-.2f),.095f,.22f,brass);
                if(kind==RunWeapon.Hammer){Box("Hammer head",new Vector3(0,0,.53f),new Vector3(.6f,.27f,.3f),metal);for(int s=-1;s<=1;s+=2)Box("Impact face",new Vector3(s*.31f,0,.53f),new Vector3(.06f,.3f,.33f),glow);}
                else{for(int s=-1;s<=1;s+=2){Box("Axe cheek",new Vector3(s*.18f,0,.49f),new Vector3(.30f,.07f,.33f),metal,new Vector3(0,s*25,0));Box("Axe cutting edge",new Vector3(s*.32f,0,.49f),new Vector3(.08f,.05f,.38f),glow,new Vector3(0,s*25,0));}}
                return;
            }
            if(kind==RunWeapon.Saw){
                Part("Saw disc",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.65f,.025f,.65f),metal);Part("Rotor hub",PrimitiveType.Cylinder,Vector3.zero,new Vector3(.24f,.06f,.24f),brass);
                for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Box("Saw tooth",new Vector3(Mathf.Cos(a)*.34f,0,Mathf.Sin(a)*.34f),new Vector3(.13f,.04f,.11f),glow,new Vector3(0,-i*30+25,0));}return;
            }
            if(kind==RunWeapon.Boomerang){
                for(int side=-1;side<=1;side+=2){Box("Returning wing",new Vector3(side*.18f,0,.13f),new Vector3(.16f,.06f,.56f),metal,new Vector3(0,side*52,0));Box("Runic wing edge",new Vector3(side*.21f,.035f,.16f),new Vector3(.035f,.02f,.48f),glow,new Vector3(0,side*52,0));}
                Part("Return stone",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.15f,brass);return;
            }
            if (kind == RunWeapon.Blade || kind == RunWeapon.Spear)
            {
                bool spear = kind == RunWeapon.Spear;
                Barrel(new Vector3(0,0,spear?.13f:-.13f),.065f,spear?1:.32f,wood);
                for(int i=0;i<4;i++)Barrel(new Vector3(0,0,-.26f+i*.075f),.08f,.025f,brass);
                Box("Guard",new Vector3(0,0,.03f),new Vector3(spear?.22f:.4f,.09f,.065f),brass);
                Box("Steel blade",new Vector3(0,0,spear?.7f:.37f),new Vector3(spear?.15f:.22f,.045f,spear?.36f:.6f),metal);
                Box("Runic edge",new Vector3(-.09f,.025f,spear?.7f:.35f),new Vector3(.035f,.04f,spear?.27f:.5f),glow);
                Part("Diamond point",PrimitiveType.Cube,new Vector3(0,0,spear?.93f:.69f),new Vector3(.16f,.05f,.16f),glow,new Vector3(0,45,0));
                Part("Pommel",PrimitiveType.Sphere,new Vector3(0,0,-.32f),Vector3.one*.115f,brass);
                if(!spear)Box("Crescent hook",new Vector3(.12f,0,.59f),new Vector3(.12f,.05f,.26f),glow,new Vector3(0,28,0));
                return;
            }
            if(kind==RunWeapon.Frost)
            {
                Part("Frozen core",PrimitiveType.Sphere,Vector3.zero,Vector3.one*.3f,glow);
                for(int i=0;i<6;i++){
                    float a=i*Mathf.PI/3;
                    Part("Ice crystal",PrimitiveType.Cube,new Vector3(Mathf.Cos(a)*.23f,Mathf.Sin(a)*.23f,0),new Vector3(.12f,.12f,.3f),glow,new Vector3(0,45,i*60+45));
                    Box("Gyro mount",new Vector3(Mathf.Cos(a)*.33f,Mathf.Sin(a)*.33f,0),new Vector3(.10f,.14f,.1f),brass,new Vector3(0,0,i*60));
                }
                return;
            }
            Box("Receiver",Vector3.zero,new Vector3(.22f,.21f,.34f),metal);
            Box("Grip",new Vector3(0,-.18f,-.10f),new Vector3(.12f,.25f,.14f),wood,new Vector3(-16,0,0));
            Box("Stock",new Vector3(0,-.025f,-.31f),new Vector3(.17f,.2f,.26f),wood);
            if(kind==RunWeapon.Crossbow){
                Barrel(new Vector3(0,0,.29f),.065f,.68f,metal);
                for(int side=-1;side<=1;side+=2){Box("Bow limb",new Vector3(side*.21f,.01f,.18f),new Vector3(.44f,.065f,.075f),wood,new Vector3(0,side*18,0));Box("Bow string",new Vector3(side*.18f,0,-.02f),new Vector3(.38f,.022f,.022f),glow,new Vector3(0,-side*24,0));}
                Box("Bolt tip",new Vector3(0,0,.65f),new Vector3(.09f,.06f,.14f),glow,new Vector3(0,45,0));
            }
            else if(kind==RunWeapon.BurstRifle){
                Barrel(new Vector3(0,.02f,.38f),.09f,.6f,metal);Box("Magazine",new Vector3(0,-.21f,.11f),new Vector3(.12f,.26f,.18f),metal,new Vector3(-10,0,0));
                for(int i=0;i<3;i++)Box("Burst indicator",new Vector3(.12f,.03f,-.08f+i*.085f),new Vector3(.025f,.07f,.04f),glow);
            }
            else if(kind==RunWeapon.Ricochet){
                Barrel(new Vector3(0,.03f,.23f),.14f,.34f,metal);Part("Prism chamber",PrimitiveType.Sphere,new Vector3(0,.07f,-.03f),Vector3.one*.2f,glow);Box("Deflector",new Vector3(0,.04f,.44f),new Vector3(.23f,.22f,.06f),brass);
            }
            else if(kind==RunWeapon.Railgun)
            {
                for(int side=-1;side<=1;side+=2){
                    Box("Magnetic rail",new Vector3(side*.105f,.01f,.4f),new Vector3(.075f,.12f,.72f),metal);
                    Box("Plasma channel",new Vector3(side*.065f,.085f,.4f),new Vector3(.035f,.045f,.69f),glow);
                }
                for(int i=0;i<4;i++)Box("Coil bridge",new Vector3(0,0,.14f+i*.15f),new Vector3(.3f,.2f,.045f),brass);
                Barrel(new Vector3(0,.2f,-.025f),.11f,.25f,metal);
                Part("Scope lens",PrimitiveType.Sphere,new Vector3(0,.2f,.10f),new Vector3(.09f,.09f,.035f),glow);
            }
            else if(kind==RunWeapon.Flame)
            {
                Barrel(new Vector3(0,.015f,.34f),.21f,.45f,metal);
                Barrel(new Vector3(0,.015f,.59f),.28f,.10f,brass);
                Part("Pilot flame",PrimitiveType.Sphere,new Vector3(0,-.07f,.66f),Vector3.one*.1f,glow);
                for(int side=-1;side<=1;side+=2){
                    Part("Fuel tank",PrimitiveType.Capsule,new Vector3(side*.18f,-.07f,-.08f),new Vector3(.16f,.24f,.16f),glow);
                    Box("Tank strap",new Vector3(side*.18f,-.07f,-.08f),new Vector3(.18f,.065f,.18f),brass);
                }
            }
            else
            {
                Barrel(new Vector3(0,.035f,.31f),.34f,.5f,metal);
                Barrel(new Vector3(0,.035f,.56f),.38f,.07f,brass);
                Barrel(new Vector3(0,.035f,.602f),.24f,.015f,wood);
                Part("Rotary drum",PrimitiveType.Cylinder,new Vector3(0,-.16f,.05f),new Vector3(.35f,.17f,.35f),brass,new Vector3(0,0,90));
                for(int i=0;i<3;i++)Box("Hazard stripe",new Vector3(0,.205f,.15f+i*.11f),new Vector3(.2f,.025f,.045f),glow);
                if(kind==RunWeapon.Mortar){Box("Siege housing",new Vector3(0,-.04f,.16f),new Vector3(.5f,.34f,.42f),metal);for(int side=-1;side<=1;side+=2)Box("Stabilizer",new Vector3(side*.3f,-.15f,.1f),new Vector3(.07f,.08f,.53f),brass,new Vector3(0,side*18,0));}
            }
        }
    }
}
