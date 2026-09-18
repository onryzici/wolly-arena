using System;
using UnityEngine;
namespace WoollyArena {
 // Authored per-character fits; the animated bones own position, rotation and scale.
 public sealed class CharacterAccessories:MonoBehaviour {
  [Serializable] sealed class Fit {public Vector3 hat,hatScale,glasses,glassesScale,necklace;public float neckWidth,neckDepth;}
  [Serializable] sealed class Fits {public Fit[] profiles;}
  static Fits catalog;
  Transform head,chest,foot;readonly Transform[] pieces=new Transform[3];Material dark,brown,gold,glass;int equipped=-1,profile=-1;
  public static void Apply(Transform avatar,int character){if(!avatar)return;var owner=avatar.GetComponent<CharacterAccessories>();if(!owner)owner=avatar.gameObject.AddComponent<CharacterAccessories>();owner.Rebuild(CareerStore.Load().AccessoriesFor(character),character);}
  void Rebuild(int flags,int character){
   if(equipped==flags&&profile==character)return;
   if(catalog==null){var asset=Resources.Load<TextAsset>("Characters/AccessoryFits");if(asset)catalog=JsonUtility.FromJson<Fits>(asset.text);}
   if(catalog?.profiles==null||character<0||character>=catalog.profiles.Length){Debug.LogError("Missing character accessory fit.");return;}
   foreach(var bone in GetComponentsInChildren<Transform>(true)){if(bone.name=="mixamorig:Head")head=bone;else if(bone.name=="mixamorig:Spine2")chest=bone;else if(bone.name=="mixamorig:LeftFoot")foot=bone;}
   if(!head||!chest||!foot)return;
   equipped=flags;profile=character;
   foreach(var piece in pieces)if(piece){piece.gameObject.SetActive(false);Destroy(piece.gameObject);}
   Array.Clear(pieces,0,pieces.Length);
   // All three avatars share the Woolly rig. Account for FBX unit scaling once;
   // subsequent lobby scaling and animation are inherited directly from the bone.
   float unit=Vector3.Distance(head.position,foot.position)/.914f;
   float localUnit=unit/Mathf.Max(.001f,Mathf.Abs(transform.lossyScale.y));
   if(!dark){var shader=Resources.Load<Shader>("CombatModel");dark=new Material(shader){color=new Color(.08f,.09f,.14f)};brown=new Material(shader){color=new Color(.52f,.25f,.10f)};gold=new Material(shader){color=new Color(1,.71f,.18f)};glass=new Material(shader){color=new Color(.16f,.65f,.82f)};}
   var fit=catalog.profiles[character];
   for(int i=0;i<3;i++)if((flags&(1<<i))!=0){
    var anchor=i==2?chest:head;var root=new GameObject(CareerProgress.AccessoryNames[i]).transform;root.SetParent(anchor,false);pieces[i]=root;
    Vector3 point=i==0?fit.hat:i==1?fit.glasses:fit.necklace;
    root.SetPositionAndRotation(transform.TransformPoint(point*localUnit),transform.rotation);
    Vector3 size=i==0?fit.hatScale:i==1?fit.glassesScale:Vector3.one;var scale=anchor.lossyScale;
    root.localScale=new Vector3(unit*size.x/Mathf.Max(.001f,Mathf.Abs(scale.x)),unit*size.y/Mathf.Max(.001f,Mathf.Abs(scale.y)),unit*size.z/Mathf.Max(.001f,Mathf.Abs(scale.z)));
    Transform Part(PrimitiveType kind,Vector3 pos,Vector3 dimensions,Material material)=>CreatureModel.Part(root,kind,pos,dimensions,material);
    if(i==0){
     Part(PrimitiveType.Cylinder,Vector3.zero,new Vector3(.88f,.018f,.91f),dark);
     Part(PrimitiveType.Cylinder,Vector3.up*.02f,new Vector3(.85f,.018f,.88f),brown);
     Part(PrimitiveType.Cylinder,Vector3.up*.135f,new Vector3(.66f,.12f,.78f),brown);
     Part(PrimitiveType.Cylinder,Vector3.up*.058f,new Vector3(.667f,.022f,.787f),gold);
    }
    else if(i==1){
     for(int side=-1;side<=1;side+=2){
      Part(PrimitiveType.Cube,new Vector3(side*.14f,0,0),new Vector3(.25f,.15f,.045f),dark);
      Part(PrimitiveType.Cube,new Vector3(side*.14f,0,.027f),new Vector3(.19f,.095f,.014f),glass);
      Part(PrimitiveType.Cube,new Vector3(side*.255f,0,-.15f),new Vector3(.025f,.026f,.32f),dark);
     }
     Part(PrimitiveType.Cube,Vector3.zero,new Vector3(.085f,.026f,.035f),gold);
    }
    else{
     var chain=root.gameObject.AddComponent<LineRenderer>();chain.sharedMaterial=gold;chain.useWorldSpace=false;chain.loop=true;chain.positionCount=40;chain.startWidth=chain.endWidth=.014f;
     for(int n=0;n<40;n++){float a=n*Mathf.PI/20;float front=Mathf.Sin(a);chain.SetPosition(n,new Vector3(Mathf.Cos(a)*fit.neckWidth,-.12f*Mathf.Max(0,front),front*fit.neckDepth));}
     var pendant=Part(PrimitiveType.Cube,new Vector3(0,-.17f,fit.neckDepth+.018f),new Vector3(.10f,.10f,.028f),gold);pendant.localRotation=Quaternion.Euler(0,0,45);
    }
    foreach(var child in root.GetComponentsInChildren<Transform>())child.gameObject.layer=gameObject.layer;
   }
  }
  void OnDestroy(){foreach(var piece in pieces)if(piece)Destroy(piece.gameObject);if(dark)Destroy(dark);if(brown)Destroy(brown);if(gold)Destroy(gold);if(glass)Destroy(glass);}
 }
}
