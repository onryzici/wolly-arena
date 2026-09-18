using System.Collections.Generic;
using UnityEngine;
namespace WoollyArena {
// Generated painted artwork, shared by shop offers, inventory slots and the combat HUD.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class ArenaIcon : UnityEngine.UI.Image {
 static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
 static readonly string[] keys={
  "revolver","repeater","shotgun","galaxy","energy",
  "star","vest","boots","scope","trigger",
  "powder","canteen","charm","plates","fang",
  "sprout","MaxHealth","Damage","AttackSpeed","MoveSpeed",
  "Armor","Critical","Regeneration","Harvesting","coin"
 };
 int statGlyph=-1;
 public override Texture mainTexture=>statGlyph>=0?Texture2D.whiteTexture:base.mainTexture;
 protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh){if(statGlyph>=0)StatGlyph.Draw(vh,GetPixelAdjustedRect(),statGlyph);else base.OnPopulateMesh(vh);}
 static Texture2D atlas;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
 static void ResetCache(){foreach(var sprite in sprites.Values)if(sprite)Destroy(sprite);sprites.Clear();atlas=null;}
 static void Load(){
  if(atlas)return;
  atlas=Resources.Load<Texture2D>("CombatArt/CartoonInventory");
  if(!atlas){Debug.LogError("Cartoon inventory atlas is missing.");return;}
  for(int i=0;i<keys.Length;i++){
   int column=i%5,row=4-i/5;
   int x0=Mathf.RoundToInt(column*atlas.width/5f),x1=Mathf.RoundToInt((column+1)*atlas.width/5f);
   int y0=Mathf.RoundToInt(row*atlas.height/5f),y1=Mathf.RoundToInt((row+1)*atlas.height/5f);
   var sprite=Sprite.Create(atlas,new Rect(x0,y0,x1-x0,y1-y0),Vector2.one*.5f,100,0,SpriteMeshType.FullRect);
   sprite.name=keys[i];sprites[keys[i]]=sprite;
  }
 }
 public void Set(string key){statGlyph=-1;if(System.Enum.TryParse<RunStat>(key,out var stat)&&System.Enum.IsDefined(typeof(RunStat),stat)){statGlyph=(int)stat;sprite=null;color=Color.white;raycastTarget=false;SetAllDirty();return;}if(key=="deadeye")key="scope";else if(key=="capacitor")key="energy";Load();sprite=sprites.TryGetValue(key,out var result)?result:null;preserveAspect=true;type=Type.Simple;raycastTarget=false;color=sprite?Color.white:Color.clear;}
}
}
