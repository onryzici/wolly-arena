using UnityEngine;
using UnityEngine.UI;
namespace WoollyArena
{
    public static class ArsenalGlyph
    {
        public static void Draw(VertexHelper mesh,Rect rect,RunWeapon weapon)
        {
            mesh.Clear();float size=Mathf.Min(rect.width,rect.height)*.85f;Vector2 center=rect.center;
            Color ink=new Color32(28,37,55,255),metal=new Color32(193,223,236,255),gold=new Color32(244,186,64,255),glow=WeaponArsenal.ColorFor(weapon);
            void Poly(Color color,params Vector2[] points){int first=mesh.currentVertCount;Vector2 mean=Vector2.zero;foreach(var p in points)mean+=p;mean/=points.Length;mesh.AddVert(center+mean*size,color,Vector2.zero);foreach(var p in points)mesh.AddVert(center+p*size,color,Vector2.zero);for(int i=0;i<points.Length;i++)mesh.AddTriangle(first,first+1+i,first+1+(i+1)%points.Length);}
            void Box(float x,float y,float w,float h,Color color)=>Poly(color,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
            void Diamond(float x,float y,float r,Color color)=>Poly(color,new Vector2(x-r,y),new Vector2(x,y+r),new Vector2(x+r,y),new Vector2(x,y-r));
            if(weapon==RunWeapon.Axe||weapon==RunWeapon.Hammer){Box(-.045f,-.45f,.09f,.72f,gold);if(weapon==RunWeapon.Hammer){Box(-.35f,.1f,.7f,.26f,ink);Box(-.29f,.14f,.58f,.18f,metal);Box(-.37f,.06f,.1f,.34f,glow);Box(.27f,.06f,.1f,.34f,glow);}else{Diamond(-.19f,.18f,.23f,metal);Diamond(.19f,.18f,.23f,metal);Box(-.05f,.03f,.1f,.34f,glow);}return;}
            if(weapon==RunWeapon.Saw){for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Diamond(Mathf.Cos(a)*.33f,Mathf.Sin(a)*.33f,.10f,metal);}Diamond(0,0,.35f,ink);Diamond(0,0,.25f,metal);Diamond(0,0,.12f,gold);return;}
            if(weapon==RunWeapon.Boomerang){Poly(ink,new Vector2(-.45f,-.08f),new Vector2(0,.42f),new Vector2(.45f,-.08f),new Vector2(.29f,-.23f),new Vector2(0,.1f),new Vector2(-.29f,-.23f));Poly(glow,new Vector2(-.36f,-.06f),new Vector2(0,.32f),new Vector2(.36f,-.06f),new Vector2(.29f,-.14f),new Vector2(0,.18f),new Vector2(-.29f,-.14f));return;}
            if(weapon==RunWeapon.Blade||weapon==RunWeapon.Spear){
                Box(-.045f,-.43f,.09f,.63f,gold);
                Poly(ink,new Vector2(-.16f,-.06f),new Vector2(-.10f,.32f),new Vector2(0,.49f),new Vector2(.17f,.27f),new Vector2(.13f,-.06f));
                Poly(metal,new Vector2(-.10f,-.02f),new Vector2(-.06f,.30f),new Vector2(0,.43f),new Vector2(.11f,.25f),new Vector2(.08f,-.02f));
                Box(-.19f,-.10f,.38f,.06f,gold);Box(-.035f,0,.035f,.29f,glow);
                if(weapon==RunWeapon.Spear){Diamond(0,.19f,.16f,glow);Box(-.035f,-.4f,.07f,.25f,ink);}return;
            }
            if(weapon==RunWeapon.Frost){
                Diamond(0,0,.35f,ink);Diamond(0,0,.28f,glow);Diamond(-.055f,.055f,.14f,Color.white);
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Diamond(Mathf.Cos(a)*.4f,Mathf.Sin(a)*.4f,.065f,glow);}return;
            }
            Box(-.42f,-.07f,.70f,.24f,ink);Box(-.37f,-.02f,.63f,.13f,metal);Box(-.22f,-.3f,.14f,.25f,gold);
            if(weapon==RunWeapon.Crossbow){Box(-.10f,-.4f,.08f,.8f,gold);Box(-.3f,-.32f,.03f,.64f,glow);Diamond(.4f,.03f,.10f,metal);return;}
            if(weapon==RunWeapon.BurstRifle){Box(.12f,.015f,.35f,.07f,ink);Box(-.025f,-.28f,.1f,.24f,metal);for(int i=0;i<3;i++)Box(-.28f+i*.09f,.02f,.04f,.06f,glow);return;}
            if(weapon==RunWeapon.Ricochet){Diamond(.02f,.16f,.13f,glow);Diamond(.4f,.26f,.08f,glow);Box(.25f,.015f,.1f,.15f,gold);return;}
            if(weapon==RunWeapon.Mortar){Box(-.3f,-.04f,.65f,.28f,ink);Box(-.25f,.01f,.55f,.18f,glow);Box(-.34f,-.18f,.72f,.065f,gold);return;}
            if(weapon==RunWeapon.Railgun){Box(-.05f,.10f,.5f,.09f,ink);Box(-.05f,-.10f,.5f,.09f,ink);Box(-.01f,.02f,.46f,.05f,glow);Box(-.25f,.2f,.27f,.09f,ink);Box(-.22f,.22f,.20f,.045f,glow);}
            else if(weapon==RunWeapon.Flame){Box(-.40f,-.15f,.15f,.45f,glow);Box(.13f,-.06f,.15f,.19f,gold);Poly(glow,new Vector2(.27f,-.02f),new Vector2(.43f,.18f),new Vector2(.40f,.025f),new Vector2(.50f,-.08f),new Vector2(.31f,-.1f));}
            else{Box(.04f,-.10f,.33f,.27f,ink);Box(.06f,-.03f,.28f,.11f,gold);Diamond(-.06f,-.16f,.16f,glow);for(int i=0;i<3;i++)Box(.08f+i*.08f,.12f,.035f,.07f,glow);}
        }
    }
}
