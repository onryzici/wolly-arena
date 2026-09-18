using UnityEngine;
namespace WoollyArena {
 [RequireComponent(typeof(CanvasRenderer))] public sealed class AccessoryGlyph:UnityEngine.UI.MaskableGraphic {
  public int kind;
  protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh){
   mesh.Clear();var rect=GetPixelAdjustedRect();float size=Mathf.Min(rect.width,rect.height);Color ink=new Color32(22,29,47,255),gold=new Color32(255,198,67,255);
   void Poly(Color tint,params Vector2[] points){int first=mesh.currentVertCount;Vector2 center=Vector2.zero;foreach(var point in points)center+=point;center/=points.Length;mesh.AddVert(rect.center+center*size,tint,Vector2.zero);foreach(var point in points)mesh.AddVert(rect.center+point*size,tint,Vector2.zero);for(int i=0;i<points.Length;i++)mesh.AddTriangle(first,first+i+1,first+(i+1)%points.Length+1);}
   void Box(float x,float y,float w,float h,Color tint)=>Poly(tint,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h));
   if(kind==0){Box(-.65f,-.2f,1.3f,.18f,ink);Box(-.42f,-.09f,.84f,.6f,ink);Box(-.37f,-.04f,.74f,.49f,new Color32(163,87,39,255));Box(-.37f,-.04f,.74f,.13f,gold);}
   else if(kind==1){Box(-.65f,-.22f,.58f,.45f,ink);Box(.07f,-.22f,.58f,.45f,ink);Box(-.11f,.02f,.22f,.08f,gold);Box(-.59f,-.15f,.46f,.31f,new Color32(64,198,238,255));Box(.13f,-.15f,.46f,.31f,new Color32(64,198,238,255));}
   else{Poly(ink,new Vector2(-.35f,.4f),new Vector2(-.25f,.4f),new Vector2(0,-.1f),new Vector2(.25f,.4f),new Vector2(.35f,.4f),new Vector2(0,-.27f));Poly(gold,new Vector2(0,-.05f),new Vector2(.23f,-.28f),new Vector2(0,-.51f),new Vector2(-.23f,-.28f));}
  }
 }
}
