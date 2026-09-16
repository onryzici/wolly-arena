using UnityEngine;
using System.Collections.Generic;
namespace WoollyArena {
public sealed class FootprintTrail:MonoBehaviour {
 public Material material;public int capacity=48;public float lifetime=10,opacity=.24f;
 public int Stamps {get;private set;}public int ActiveCount {get;private set;}
 Transform root;Mesh mesh;MeshRenderer[] marks;float[] born;MaterialPropertyBlock block;int cursor;
 void Awake(){root=new GameObject("Footprint Pool").transform;mesh=MakeSole();marks=new MeshRenderer[capacity];born=new float[capacity];block=new MaterialPropertyBlock();for(int i=0;i<capacity;i++){var g=new GameObject("Sole "+i,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root);g.GetComponent<MeshFilter>().sharedMesh=mesh;var r=g.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;r.enabled=false;marks[i]=r;}}
 public void Stamp(Vector3 position,Vector3 forward){if(forward.sqrMagnitude<.001f)return;forward.y=0;forward.Normalize();var r=marks[cursor];r.transform.position=position+forward*.035f;r.transform.rotation=Quaternion.LookRotation(forward,Vector3.up);r.enabled=true;born[cursor]=Time.time;Tint(r,opacity);cursor=(cursor+1)%capacity;Stamps++;}
 void Tint(MeshRenderer r,float alpha){block.SetColor("_Color",new Color(.37f,.19f,.105f,alpha));r.SetPropertyBlock(block);}
 void Update(){ActiveCount=0;for(int i=0;i<capacity;i++){var r=marks[i];if(!r.enabled)continue;float age=Time.time-born[i];if(age>=lifetime){r.enabled=false;continue;}ActiveCount++;Tint(r,opacity*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(lifetime*.5f,lifetime,age))));}}
 static Mesh MakeSole(){var v=new List<Vector3>();var t=new List<int>();void Row(float z0,float z1,float w0,float w1){int i=v.Count;v.Add(new Vector3(-w0,0,z0));v.Add(new Vector3(w0,0,z0));v.Add(new Vector3(w1,0,z1));v.Add(new Vector3(-w1,0,z1));t.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});}Row(-.15f,-.085f,.055f,.060f);Row(-.055f,-.008f,.058f,.067f);Row(.005f,.055f,.068f,.075f);Row(.068f,.115f,.075f,.070f);Row(.128f,.16f,.065f,.047f);var m=new Mesh{name="Stylized boot tread"};m.SetVertices(v);m.SetTriangles(t,0);m.RecalculateNormals();return m;}
 void OnDestroy(){if(root)Destroy(root.gameObject);if(mesh)Destroy(mesh);}
}
}
