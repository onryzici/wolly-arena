using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
namespace WoollyArena.Editor {
public static class ArenaEnvironmentPass {
 static Transform root;
 static Material Mat(string name,string hex){string path="Assets/Woolly/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Woolly/ArenaToon"));AssetDatabase.CreateAsset(m,path);}ColorUtility.TryParseHtmlString(hex,out var c);m.SetColor("_BaseColor",c);EditorUtility.SetDirty(m);return m;}
 static Material Remap(string n){if(n.Contains("Sandstone"))return Mat("Carved sandstone","#BD7255");if(n.Contains("StoneTop"))return Mat("Stone sun edges","#ECA07A");if(n.Contains("WoodLight"))return Mat("Honey wood","#D59C5B");if(n.Contains("Wood"))return Mat("Warm timber","#A86E42");if(n.Contains("Iron"))return Mat("Barrel bands","#637D88");if(n.Contains("GrassLight"))return Mat("Golden leaf tips","#FFE38A");return Mat("Golden leaves","#F6BE55");}
 static GameObject Prop(string asset,Vector3 pos,float scale=1,float yaw=0){var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Woolly/Art/Environment/"+asset+".fbx");var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.name=asset;g.transform.SetParent(root,false);g.transform.localPosition=pos;g.transform.localRotation=Quaternion.Euler(0,yaw,0)*prefab.transform.localRotation;g.transform.localScale=Vector3.one*scale;foreach(var r in g.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Remap(m.name)).ToArray();return g;}
 static void GroundPatch(Vector3 p,float radius,int sides,Material m){var vertices=new Vector3[sides+1];var tris=new int[sides*3];vertices[0]=Vector3.zero;for(int i=0;i<sides;i++){float a=i*Mathf.PI*2/sides;float r=radius*Random.Range(.7f,1.2f);vertices[i+1]=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r*.6f);tris[i*3]=0;tris[i*3+1]=(i+1)%sides+1;tris[i*3+2]=i+1;}var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=tris;mesh.RecalculateNormals();var g=new GameObject("Sandstone flake",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root);g.transform.position=p;g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=m;AssetDatabase.AddObjectToAsset(mesh,"Assets/Woolly/Art/Environment/TerrainDetails.asset");}
 [MenuItem("Woolly/Build Environment Art")]
 public static string Apply(){
 if(GameObject.Find("Environment_Art"))return "Environment art already exists";
 DesertArtDirection.Apply();Random.InitState(402);root=new GameObject("Environment_Art").transform;
 var terrainMesh=new Mesh{name="Terrain detail container"};AssetDatabase.CreateAsset(terrainMesh,"Assets/Woolly/Art/Environment/TerrainDetails.asset");
 var env=GameObject.Find("Arena_Geometry");foreach(Transform t in env.transform){if(t.name=="Cover"){t.GetComponent<Renderer>().enabled=false;Prop("CarvedStoneCover",new Vector3(t.position.x,0,t.position.z));}if(t.name=="CoverCap"){t.GetComponent<Renderer>().enabled=false;var col=t.GetComponent<Collider>();if(col)col.enabled=false;}}
 foreach(var p in new[]{new Vector3(-6.7f,0,.3f),new Vector3(6.7f,0,.5f),new Vector3(-4.5f,0,4.8f),new Vector3(4.6f,0,4.8f),new Vector3(-7.7f,0,-5),new Vector3(7.7f,0,-5)}){Prop("WoodBarrel",p,.9f,Random.Range(0,360));}
 Prop("SupplyCrate",new Vector3(-7.2f,0,3),1.1f,12);Prop("SupplyCrate",new Vector3(7.1f,0,3),1.1f,-10);
 foreach(var center in new[]{new Vector3(-5,0,2),new Vector3(5,0,2),new Vector3(-5.7f,0,-3),new Vector3(5.7f,0,-3),new Vector3(-8,0,6.5f),new Vector3(8,0,6.5f)})for(int x=-2;x<=2;x++)for(int z=-1;z<=1;z++)Prop("GoldenGrass",center+new Vector3(x*.36f+Random.Range(-.08f,.08f),0,z*.36f+Random.Range(-.08f,.08f)),Random.Range(.8f,1.1f),Random.Range(0,360));
 var patch=Mat("Sand subtle patches","#E3A071");var stone=Mat("Sand flecks","#F5B785");for(int i=0;i<65;i++){var p=new Vector3(Random.Range(-10f,10f),.018f,Random.Range(-8.8f,9f));if(Mathf.Abs(p.x)<1.7f)continue;GroundPatch(p,Random.Range(.12f,.44f),Random.Range(4,7),i%3==0?patch:stone);}
 // Tone down direct glare on the white wool while retaining shape.
 GameObject.Find("Sun").GetComponent<Light>().intensity=1.05f;
 EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Carved stone, wood props, golden foliage and terrain details added";
 }
}
}
