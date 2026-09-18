using UnityEngine;
using System.Collections.Generic;
namespace WoollyArena {
 public static class MenuSkin {
  static readonly Dictionary<string,Sprite> sprites=new Dictionary<string,Sprite>();
  public static Sprite Get(string name){if(sprites.TryGetValue(name,out var sprite)&&sprite)return sprite;var texture=Resources.Load<Texture2D>("MenuSkin/"+name);if(!texture)return null;float b=name=="Panel"?20:25;sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(b,b,b,b));sprites[name]=sprite;return sprite;}
 }
}
