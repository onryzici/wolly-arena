using System.Collections.Generic;
using UnityEngine;
namespace WoollyArena {
// Generated painted artwork, shared by shop offers, inventory slots and the combat HUD.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArenaIcon : UnityEngine.UI.Image {
 static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
 static readonly string[] keys={
  "revolver","repeater","shotgun","galaxy","energy","star",
  "blade","spear","railgun","flame","frost","grenade",
  "axe","hammer","crossbow","burst","ricochet","boomerang",
  "saw","mortar","vest","boots","scope","trigger",
  "powder","canteen","charm","plates","fang","deadeye",
  "capacitor","brawler","cinder","coolant","payload","sprout"
 };
 // Authored row gutters are slightly uneven; keep the next row out of each sprite.
 static readonly int[] rowEdges={0,221,429,635,841,1025,1254};
 static readonly int[] columnEdges={0,215,413,624,828,1036,1254};
 int statGlyph=-1;
 public override Texture mainTexture=>statGlyph>=0?Texture2D.whiteTexture:base.mainTexture;
 protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh){if(statGlyph>=0)StatGlyph.Draw(vh,GetPixelAdjustedRect(),statGlyph);else base.OnPopulateMesh(vh);}
 static Texture2D atlas;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void ResetCache(){foreach(var sprite in sprites.Values)if(sprite)Destroy(sprite);sprites.Clear();atlas=null;}
 static void Load(){
  if(atlas)return;
  atlas=Resources.Load<Texture2D>("CombatArt/FlatInventory");
  if(!atlas){Debug.LogError("Cartoon inventory atlas is missing.");return;}
  for(int i=0;i<keys.Length;i++){
   int column=i%6,row=i/6;
   int x0=Mathf.RoundToInt(columnEdges[column]*atlas.width/1254f),x1=Mathf.RoundToInt(columnEdges[column+1]*atlas.width/1254f);
   int y0=atlas.height-Mathf.RoundToInt(rowEdges[row+1]*atlas.height/1254f),y1=atlas.height-Mathf.RoundToInt(rowEdges[row]*atlas.height/1254f);
   var sprite=Sprite.Create(atlas,new Rect(x0,y0,x1-x0,y1-y0),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
   sprite.name=keys[i];sprites[keys[i]]=sprite;
  }
 }
 public void Set(string key){statGlyph=-1;
  if(key=="coin")key="Harvesting";
  if(System.Enum.TryParse<RunStat>(key,out var stat)&&System.Enum.IsDefined(typeof(RunStat),stat)){statGlyph=(int)stat;sprite=null;color=Color.white;raycastTarget=false;SetAllDirty();return;}Load();sprite=sprites.TryGetValue(key,out var result)?result:null;preserveAspect=true;type=Type.Simple;raycastTarget=false;color=sprite?Color.white:Color.clear;SetAllDirty();}
}
}
