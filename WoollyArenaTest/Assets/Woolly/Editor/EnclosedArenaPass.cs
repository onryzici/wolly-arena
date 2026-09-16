using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using System.Collections.Generic;using System.Linq;
namespace WoollyArena.Editor {
public static class EnclosedArenaPass {
 static Material Mat(string name,string hex){var path="Assets/Woolly/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Woolly/ArenaToon"));AssetDatabase.CreateAsset(m,path);}ColorUtility.TryParseHtmlString(hex,out var c);m.SetColor("_BaseColor",c);EditorUtility.SetDirty(m);return m;}
 public static string Apply(){
 if(EditorApplication.isPlaying)throw new System.Exception("Stop Play first");
 if(!GameObject.Find("Arena Canyon Rim")){
 var go=new GameObject("Arena Canyon Rim",typeof(MeshFilter),typeof(MeshRenderer));const int edge=12,n=edge*4;var verts=new List<Vector3>();float[] offsets={0,.06f,-.08f,.10f,-.03f,.30f,2.0f,2.0f};float[] heights={-.08f,.58f,.68f,1.25f,1.36f,2.05f,2.12f,-.08f};
 for(int level=0;level<8;level++)for(int i=0;i<n;i++){int side=i/edge;float t=(float)(i%edge)/edge;Vector2 p=side==0?new Vector2(Mathf.Lerp(-10.3f,10.3f,t),9.3f):side==1?new Vector2(10.3f,Mathf.Lerp(9.3f,-9.3f,t)):side==2?new Vector2(Mathf.Lerp(10.3f,-10.3f,t),-9.3f):new Vector2(-10.3f,Mathf.Lerp(-9.3f,9.3f,t));float jitter=Mathf.PerlinNoise(p.x*.41f+17,p.y*.37f+8);var expanded=new Vector2(p.x*(1+offsets[level]/10.3f),p.y*(1+offsets[level]/9.3f));float front=Mathf.Lerp(.53f,1,Mathf.InverseLerp(-9.3f,-5,p.y));float h=(heights[level]+(level>0&&level<7?(jitter-.5f)*.32f:0))*front;verts.Add(new Vector3(expanded.x,h,expanded.y));}
 var mesh=new Mesh{name="Layered closed canyon"};mesh.SetVertices(verts);mesh.subMeshCount=4;var lists=new List<int>[] {new List<int>(),new List<int>(),new List<int>(),new List<int>()};for(int l=0;l<7;l++){int mat=l==5?3:l==1||l==3?2:l<3?0:1;for(int i=0;i<n;i++){int j=(i+1)%n;int a=l*n+i,b=l*n+j,c=(l+1)*n+i,d=(l+1)*n+j;lists[mat].AddRange(new[]{a,c,b,b,c,d});}}for(int i=0;i<4;i++)mesh.SetTriangles(lists[i],i);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,"Assets/Woolly/Art/Environment/CanyonRim.asset");go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterials=new[]{Mat("Canyon lower","#B96349"),Mat("Canyon upper","#D17C58"),Mat("Canyon strata","#A85442"),Mat("Canyon cap","#EDAD79")};
 }
 var env=GameObject.Find("Arena_Geometry");foreach(Transform t in env.transform)if(t.name=="Boundary"){t.GetComponent<Renderer>().enabled=false;var p=t.position;if(Mathf.Abs(p.x)>10){p.x=Mathf.Sign(p.x)*10.55f;t.localScale=new Vector3(.5f,2.6f,20);}else{p.z=Mathf.Sign(p.z)*9.55f;t.localScale=new Vector3(22,2.6f,.5f);}p.y=1.2f;t.position=p;}
 GameObject.Find("Ground").transform.localScale=new Vector3(31,.4f,29);
 foreach(string name in new[]{"Golden leaves","Golden leaf tips"}){var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Woolly/Materials/"+name+".mat");m.shader=Shader.Find("Woolly/GrassSway");EditorUtility.SetDirty(m);}
 var cam=Camera.main;cam.orthographicSize=6.1f;var follow=cam.GetComponent<ArenaCamera>();follow.confineToArena=true;follow.followLimits=new Vector2(1.5f,2);cam.transform.position=follow.target.position+follow.offset;
 EditorSceneManager.MarkSceneDirty(env.scene);EditorSceneManager.SaveOpenScenes();AssetDatabase.SaveAssets();return "Layered arena perimeter, bounded camera and anchored grass sway ready";
 }
}
}
