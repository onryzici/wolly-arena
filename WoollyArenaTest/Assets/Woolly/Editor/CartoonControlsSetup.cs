using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using TMPro;using System.Linq;
namespace WoollyArena.Editor {
public static class CartoonControlsSetup {
 const string Root="Assets/Woolly/UI/";
 static Color Hex(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
 static Sprite Art(string name,Color face,int icon,bool baseRing=false){
 string path=Root+name+".png";var t=new Texture2D(256,256,TextureFormat.RGBA32,false);
 for(int y=0;y<256;y++)for(int x=0;x<256;x++){
 float a=(x+.5f-128)/126,b=(y+.5f-128)/126,r=Mathf.Sqrt(a*a+b*b);Color c;
 if(baseRing){c=face;c.a=r>.968f?.32f:.055f;}else{c=r>.92f?Hex("#123568"):r>.86f?face*.60f:Color.Lerp(face*.72f,face,Mathf.Clamp01((b+1)*.55f));c.a=.88f;if(icon==2){c=Hex("#C34032");c.a=r>.9f?.48f:.10f;}
 bool glyph=false;
 if(icon==1){float ax=Mathf.Abs(a),ay=Mathf.Abs(b);glyph=(ay>.19f&&ay<.46f&&ax<.46f-ay)||(ax>.19f&&ax<.46f&&ay<.46f-ax);}
 if(icon==2){glyph=Mathf.Abs(r-.31f)<.035f||r<.075f||(Mathf.Abs(a)<.033f&&Mathf.Abs(b)>.23f&&Mathf.Abs(b)<.48f)||(Mathf.Abs(b)<.033f&&Mathf.Abs(a)>.23f&&Mathf.Abs(a)<.48f);}
 if(icon==3){float angle=Mathf.Atan2(b,a);glyph=(Mathf.Abs(r-.34f)<.055f&&angle<2.5f)||(a<-.19f&&a>-.48f&&b>.14f&&b<.37f&&b<.7f+a);}
 if(glyph){c=icon==1?Hex("#185395"):icon==2?Hex("#B43C2F"):Hex("#90C8F2");c.a=icon==2?.65f:1;}}
 c.a*=Mathf.Clamp01((1-r)*126);t.SetPixel(x,y,c);
 }t.Apply();System.IO.File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.mipmapEnabled=false;imp.alphaIsTransparency=true;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
 }
 public static string Apply(){
 var canvas=GameObject.Find("Test HUD").GetComponent<Canvas>();var controls=canvas.GetComponentInChildren<MobileControls>(true);if(!controls)throw new System.Exception("Missing controls");
 var path=Root+"Fonts/LilitaOne SDF.asset";var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
 if(!font){font=TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>(Root+"Fonts/LilitaOne-Regular.ttf"),80,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024);font.name="LilitaOne SDF";string chars=new string(Enumerable.Range(32,224).Select(i=>(char)i).ToArray())+"ĞğİıŞş";font.TryAddCharacters(chars,out string missing);font.atlasPopulationMode=AtlasPopulationMode.Static;AssetDatabase.CreateAsset(font,path);AssetDatabase.AddObjectToAsset(font.material,font);foreach(var tex in font.atlasTextures){tex.name="LilitaOne Atlas";AssetDatabase.AddObjectToAsset(tex,font);}font.material.EnableKeyword("OUTLINE_ON");font.material.SetColor("_OutlineColor",Hex("#35283F"));font.material.SetFloat("_OutlineWidth",.16f);}
 foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)){text.font=font;text.fontSharedMaterial=font.material;text.fontStyle=FontStyles.Normal;text.enableAutoSizing=false;text.characterSpacing=.5f;text.UpdateMeshPadding();EditorUtility.SetDirty(text);}
 Color blue=Hex("#247ED1"),red=Hex("#D9513C");
 var sticks=new[]{controls.move,controls.aim};for(int i=0;i<2;i++){var st=sticks[i];var c=i==0?blue:red;var bg=st.GetComponent<UnityEngine.UI.Image>();bg.sprite=Art(i==0?"MoveBase":"AimBase",c,0,true);bg.color=Color.white;bg.raycastTarget=true;var knob=st.knob.GetComponent<UnityEngine.UI.Image>();knob.sprite=Art(i==0?"MoveThumb":"AimThumb",c,i+1);knob.color=Color.white;knob.raycastTarget=false;st.knob.sizeDelta=Vector2.one*(i==0?82:88);st.radius=74;((RectTransform)st.transform).sizeDelta=Vector2.one*224;foreach(var label in st.GetComponentsInChildren<TMP_Text>()){label.gameObject.SetActive(false);label.text=i==0?"MOVE":"AIM / FIRE";label.fontSize=19;label.color=Hex("#FFF4D9");}}
 var reload=controls.transform.Find("Reload");var ri=reload.GetComponent<UnityEngine.UI.Image>();ri.sprite=Art("ReloadButton",Hex("#2365A8"),3);ri.color=Color.white;var labelReload=reload.GetComponentInChildren<TMP_Text>();labelReload.gameObject.SetActive(false);labelReload.fontSize=16;labelReload.rectTransform.anchoredPosition=new Vector2(0,-62);labelReload.color=Hex("#FFF4D9");
 foreach(var t in canvas.GetComponentsInChildren<TMP_Text>(true))if(t.name.StartsWith("WASD"))t.gameObject.SetActive(false);
 var button=reload.GetComponent<UnityEngine.UI.Button>();var colors=button.colors;colors.pressedColor=new Color(.75f,.75f,.75f,1);colors.highlightedColor=Color.white;button.colors=colors;
 EditorUtility.SetDirty(font);EditorUtility.SetDirty(font.material);EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Lilita One font and layered cartoon controls saved";
 }
}}
