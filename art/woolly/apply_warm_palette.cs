var root="Assets/Woolly/Materials/";
var body=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(root+"Woolly Body.mat");
if(!UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(root+"Woolly Body Before Warm.mat"))UnityEditor.AssetDatabase.CopyAsset(root+"Woolly Body.mat",root+"Woolly Body Before Warm.mat");
body.shader=UnityEngine.Shader.Find("Woolly/WarmCharacter");UnityEditor.EditorUtility.SetDirty(body);
string[] names={"Midnight steel","Amber grip","Blue steel edges","Recesses","Brass accents"};
string[] colors={"#344852","#9B5531","#718C96","#292D32","#DFA552"};
for(int i=0;i<names.Length;i++){var m=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(root+"Revolver _ "+names[i]+".mat");UnityEngine.ColorUtility.TryParseHtmlString(colors[i],out var c);m.SetColor("_BaseColor",c);m.SetFloat("_Metallic",0);m.SetFloat("_Smoothness",.12f);UnityEditor.EditorUtility.SetDirty(m);}
var ink=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(root+"Character ink.mat");ink.SetColor("_OutlineColor",new UnityEngine.Color(.23f,.125f,.095f));UnityEditor.EditorUtility.SetDirty(ink);
UnityEditor.AssetDatabase.SaveAssets();return new {shaderErrors=UnityEditor.ShaderUtil.ShaderHasError(body.shader)};
