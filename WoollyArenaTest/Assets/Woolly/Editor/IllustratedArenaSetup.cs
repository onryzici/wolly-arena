using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using System.Linq;
namespace WoollyArena.Editor {
public static class IllustratedArenaSetup {
 public static string Apply(){
 if(EditorApplication.isPlaying)throw new System.Exception("Stop play first");
 string path="Assets/Woolly/Art/Environment/IllustratedArena4K.png";var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.wrapMode=TextureWrapMode.Clamp;imp.maxTextureSize=4096;imp.npotScale=TextureImporterNPOTScale.None;imp.mipmapEnabled=true;imp.anisoLevel=8;imp.filterMode=FilterMode.Trilinear;imp.textureCompression=TextureImporterCompression.Uncompressed;var ios=imp.GetPlatformTextureSettings("iPhone");ios.overridden=true;ios.maxTextureSize=4096;ios.format=TextureImporterFormat.ASTC_4x4;ios.compressionQuality=100;imp.SetPlatformTextureSettings(ios);imp.SaveAndReimport();
 string matPath="Assets/Woolly/Materials/Illustrated Arena.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);if(!mat){mat=new Material(Shader.Find("Woolly/IllustratedArena"));AssetDatabase.CreateAsset(mat,matPath);}mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(path));mat.SetFloat("_WorldScale",24);mat.SetFloat("_WorldAspect",1.5f);GameObject.Find("Ground").transform.localScale=new Vector3(36,.4f,29);mat.SetColor("_BaseColor",Color.white);
 GameObject.Find("Ground").GetComponent<Renderer>().sharedMaterial=mat;
 foreach(string n in new[]{"Environment_Art","Arena Canyon Rim"}){var o=GameObject.Find(n);if(o)o.SetActive(false);}
 var env=GameObject.Find("Arena_Geometry");var covers=env.GetComponentsInChildren<Transform>().Where(t=>t.name=="Cover").ToArray();
 // Measured silhouettes in generated raster, normalized image coordinates.
 Vector2[] uv={new Vector2(.3262f,.2725f),new Vector2(.6465f,.2725f),new Vector2(.252f,.4961f),new Vector2(.7158f,.4961f),new Vector2(.2832f,.7383f),new Vector2(.666f,.7383f)};
 for(int i=0;i<covers.Length;i++){covers[i].position=new Vector3((uv[i].x-.5f)*24,.6f,(.5f-uv[i].y)*24);covers[i].localScale=new Vector3(2.95f,1.2f,1.6f);covers[i].GetComponent<Renderer>().enabled=false;}
 foreach(Transform t in env.transform)if(t.name=="Boundary"){var q=t.position;if(Mathf.Abs(q.x)>10){q.x=Mathf.Sign(q.x)*10.8f;t.localScale=new Vector3(.5f,2.6f,22);}else{q.z=Mathf.Sign(q.z)*10.3f;t.localScale=new Vector3(22,2.6f,.5f);}t.position=q;}
 var player=Object.FindFirstObjectByType<ArenaPlayer>();player.walkSpeed=1.08f;player.runSpeed=3.36f;
 if(!player.transform.Find("Selection Ring")){var ring=GameObject.CreatePrimitive(PrimitiveType.Quad);ring.name="Selection Ring";Object.DestroyImmediate(ring.GetComponent<Collider>());ring.transform.SetParent(player.transform,false);ring.transform.localPosition=new Vector3(0,.025f,0);ring.transform.localRotation=Quaternion.Euler(90,0,0);ring.transform.localScale=Vector3.one*1.05f;var m=new Material(Shader.Find("Woolly/SelectionRing"));AssetDatabase.CreateAsset(m,"Assets/Woolly/Materials/Selection Ring.mat");var rr=ring.GetComponent<Renderer>();rr.sharedMaterial=m;rr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;rr.receiveShadows=false;}
 var cam=Camera.main;cam.orthographicSize=6.8f;var follow=cam.GetComponent<ArenaCamera>();follow.followLimits=new Vector2(.3f,.15f);
 EditorUtility.SetDirty(mat);EditorSceneManager.MarkSceneDirty(env.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Illustrated map, matching cover colliders, green selector and 20% faster movement saved";
 }
}}
