using UnityEngine;
using UnityEngine.UI;
namespace WoollyArena {
 // Flat vector symbols: no atlas cropping or neighbouring-cell fragments.
 public static class StatGlyph {
  public static void Draw(VertexHelper mesh,Rect rect,int stat){
   mesh.Clear();float size=Mathf.Min(rect.width,rect.height)*.78f;Vector2 center=rect.center;Color ink=new Color32(22,29,47,255);
   Vector2 V(float x,float y)=>center+new Vector2(x,y)*size;
   void Polygon(Color color,params Vector2[] points){int first=mesh.currentVertCount;Vector2 mean=Vector2.zero;foreach(var p in points)mean+=p;mean/=points.Length;mesh.AddVert(V(mean.x,mean.y),color,Vector2.zero);foreach(var p in points)mesh.AddVert(V(p.x,p.y),color,Vector2.zero);for(int i=0;i<points.Length;i++)mesh.AddTriangle(first,first+1+i,first+1+(i+1)%points.Length);}
   void Shape(Color color,params Vector2[] points){var border=new Vector2[points.Length];for(int i=0;i<points.Length;i++)border[i]=points[i]*1.12f;Polygon(ink,border);Polygon(color,points);}
   void Circle(float radius,Color color){var points=new Vector2[32];for(int i=0;i<32;i++){float a=i*Mathf.PI/16;points[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}Polygon(color,points);}
   var red=new Color32(255,90,99,255);var gold=new Color32(255,202,69,255);var blue=new Color32(66,191,239,255);var mint=new Color32(101,223,158,255);
   switch(stat){
    case 0:Shape(red,new Vector2(0,-.43f),new Vector2(-.44f,.03f),new Vector2(-.43f,.28f),new Vector2(-.22f,.41f),new Vector2(0,.23f),new Vector2(.22f,.41f),new Vector2(.43f,.28f),new Vector2(.44f,.03f));break;
    case 1:Shape(red,new Vector2(-.32f,-.4f),new Vector2(-.42f,-.3f),new Vector2(.22f,.38f),new Vector2(.44f,.45f),new Vector2(.37f,.2f));Polygon(gold,new Vector2(-.35f,-.1f),new Vector2(-.1f,-.35f),new Vector2(0,-.25f),new Vector2(-.25f,0));break;
    case 2:Shape(gold,new Vector2(.1f,.48f),new Vector2(-.4f,-.07f),new Vector2(-.08f,-.07f),new Vector2(-.2f,-.48f),new Vector2(.42f,.16f),new Vector2(.06f,.16f));break;
    case 3:Shape(blue,new Vector2(-.35f,-.43f),new Vector2(.4f,-.43f),new Vector2(.4f,-.18f),new Vector2(.07f,-.03f),new Vector2(.03f,.38f),new Vector2(-.35f,.38f));break;
    case 4:Shape(blue,new Vector2(-.4f,.38f),new Vector2(0,.48f),new Vector2(.4f,.38f),new Vector2(.3f,-.14f),new Vector2(0,-.46f),new Vector2(-.3f,-.14f));Polygon(Color.white,new Vector2(-.04f,.29f),new Vector2(.04f,.29f),new Vector2(.04f,-.23f),new Vector2(-.04f,-.23f));break;
    case 5:Circle(.47f,ink);Circle(.4f,red);Circle(.28f,gold);Circle(.16f,ink);Circle(.07f,Color.white);break;
    case 6:Shape(mint,new Vector2(-.14f,.43f),new Vector2(.14f,.43f),new Vector2(.14f,.14f),new Vector2(.43f,.14f),new Vector2(.43f,-.14f),new Vector2(.14f,-.14f),new Vector2(.14f,-.43f),new Vector2(-.14f,-.43f),new Vector2(-.14f,-.14f),new Vector2(-.43f,-.14f),new Vector2(-.43f,.14f),new Vector2(-.14f,.14f));break;
    default:Polygon(ink,new Vector2(-.06f,-.48f),new Vector2(.06f,-.48f),new Vector2(.06f,.37f),new Vector2(-.06f,.37f));for(int i=0;i<3;i++){float y=-.13f+i*.21f;Polygon(gold,new Vector2(0,y),new Vector2(-.29f,y+.1f),new Vector2(-.31f,y+.28f),new Vector2(-.08f,y+.2f));Polygon(gold,new Vector2(0,y),new Vector2(.29f,y+.1f),new Vector2(.31f,y+.28f),new Vector2(.08f,y+.2f));}break;
   }
  }
 }
}
