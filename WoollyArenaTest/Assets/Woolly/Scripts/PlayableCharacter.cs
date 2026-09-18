using UnityEngine;
using System.Collections.Generic;
namespace WoollyArena {
 public sealed class PlayableCharacter:MonoBehaviour {
  public static int Selected {get=>Mathf.Clamp(PlayerPrefs.GetInt("Woolly.Character",0),0,CharacterDefinition.Count-1);set{PlayerPrefs.SetInt("Woolly.Character",Mathf.Clamp(value,0,CharacterDefinition.Count-1));PlayerPrefs.Save();}}
  public static int Active {get;private set;}
  public static string Title(int id)=>CharacterDefinition.Title(id);
  public static string Role(int id)=>CharacterDefinition.Role(id);
  Material bodyMaterial,outlineMaterial,knifeMaterial;AnimatorOverrideController controller;
  public static void ApplyToPlayer(ArenaPlayer player){
   int id=Selected;if(SurvivalRun.CheckpointsEnabled&&RunSession.ResumeRequested&&RunCheckpointStore.TryLoad(RunSession.SavePath,out var saved))id=saved.Character;
   Active=id;if(id==0)return;
   var old=player.visual;var avatar=Create(old.parent,player.animator.runtimeAnimatorController,false,old,id);if(!avatar)return;
   avatar.transform.SetLocalPositionAndRotation(old.localPosition,old.localRotation);avatar.transform.localScale=old.localScale;
   old.gameObject.SetActive(false);player.visual=avatar.transform;player.animator=avatar;
   var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(avatar.transform,false);muzzle.localPosition=new Vector3(0,.9f,.35f);player.muzzle=muzzle;
   player.GetComponent<CharacterVitals>().displayName=Title(id);
  }
  public static Animator Create(Transform parent,RuntimeAnimatorController template,bool lobby=false,Transform styleReference=null,int characterId=1){
   string modelName=CharacterDefinition.ModelName(characterId);
   string resource="Characters/"+modelName;
   var prefab=Resources.Load<GameObject>(resource);var clips=Resources.LoadAll<AnimationClip>(resource);
   AnimationClip Find(string name){foreach(var clip in clips)if(!clip.name.StartsWith("__preview__")&&clip.name.EndsWith(name,System.StringComparison.OrdinalIgnoreCase))return clip;return null;}
   var idle=Find("Idle");var walk=lobby?idle:Find("Walk");var run=lobby?idle:Find("Run");
   if(!prefab||!template||!idle||!walk||!run){Debug.LogError(modelName+" model or Idle/Walk/Run clips have not imported correctly.");return null;}
   var instance=Instantiate(prefab,parent,false);instance.name=Title(characterId);var owner=instance.AddComponent<PlayableCharacter>();
   Material bodyStyle=null,inkStyle=null;
   if(styleReference)foreach(var renderer in styleReference.GetComponentsInChildren<SkinnedMeshRenderer>(true)){
    if(renderer.sharedMaterials.Length==0)continue;
    if(renderer.name.Contains("Outline"))inkStyle=renderer.sharedMaterials[0];else bodyStyle=renderer.sharedMaterials[0];
   }
   owner.bodyMaterial=bodyStyle?new Material(bodyStyle):new Material(Resources.Load<Shader>("CombatModel"));
   owner.bodyMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>(resource+"_BaseColor"));owner.bodyMaterial.SetColor("_BaseColor",Color.white);
   owner.outlineMaterial=inkStyle?new Material(inkStyle):new Material(Resources.Load<Shader>("PlayableOutline"));
   if(characterId==2){owner.knifeMaterial=new Material(Resources.Load<Shader>("CombatModel"));owner.knifeMaterial.SetColor("_BaseColor",Color.white);owner.knifeMaterial.SetFloat("_UseVertexColors",1);}
   if(!inkStyle){owner.outlineMaterial.SetColor("_OutlineColor",new Color(.23f,.125f,.095f));owner.outlineMaterial.SetFloat("_Width",.019f);}
   foreach(var source in instance.GetComponentsInChildren<SkinnedMeshRenderer>()){
    Material surface=(source.name=="Patchwork_Knife"||source.name=="Patchwork_Grip")&&owner.knifeMaterial?owner.knifeMaterial:owner.bodyMaterial;
    var materials=new Material[source.sharedMesh.subMeshCount];for(int i=0;i<materials.Length;i++)materials[i]=surface;source.sharedMaterials=materials;
    // The thin blade uses shader contours; shell outlines would cover its painted blood marks.
    if(source.name=="Patchwork_Knife"||source.name=="Patchwork_Grip")continue;
    var edge=new GameObject(source.name+"_Outline");edge.transform.SetParent(source.transform,false);var outline=edge.AddComponent<SkinnedMeshRenderer>();outline.sharedMesh=source.sharedMesh;outline.bones=source.bones;outline.rootBone=source.rootBone;outline.localBounds=source.localBounds;
    var ink=new Material[materials.Length];for(int i=0;i<ink.Length;i++)ink[i]=owner.outlineMaterial;outline.sharedMaterials=ink;outline.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;outline.receiveShadows=false;
   }
   // Identical bones and Idle allow exact socket-local transfer, including its ink mesh.
   if(lobby&&styleReference&&characterId!=2){
    Transform socket=null;
    foreach(var t in styleReference.GetComponentsInChildren<Transform>(true))if(t.name=="Revolver_HandSocket"){socket=t;break;}
    if(socket){
     Transform hand=null;
     foreach(var t in instance.GetComponentsInChildren<Transform>(true))if(t.name==socket.parent.name){hand=t;break;}
     if(hand){var gun=Instantiate(socket.gameObject,hand,false);gun.name=socket.name;gun.SetActive(true);}
     else Debug.LogError(modelName+" is missing the matching revolver hand bone.");
    }else Debug.LogError("Woolly's lobby revolver socket is missing.");
   }
   foreach(var t in instance.GetComponentsInChildren<Transform>())t.gameObject.layer=parent?parent.gameObject.layer:0;
   var animator=instance.GetComponent<Animator>();if(!animator)animator=instance.AddComponent<Animator>();
   owner.controller=new AnimatorOverrideController(template);var overrides=new List<KeyValuePair<AnimationClip,AnimationClip>>();
   foreach(var clip in template.animationClips){string name=clip.name.ToLowerInvariant();overrides.Add(new KeyValuePair<AnimationClip,AnimationClip>(clip,name.Contains("walk")?walk:name.Contains("run")?run:idle));}
   owner.controller.ApplyOverrides(overrides);animator.runtimeAnimatorController=owner.controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   if(animator.layerCount>1)animator.SetLayerWeight(1,0);return animator;
  }
  void OnDestroy(){if(bodyMaterial)Destroy(bodyMaterial);if(outlineMaterial)Destroy(outlineMaterial);if(knifeMaterial)Destroy(knifeMaterial);if(controller)Destroy(controller);}
 }
}
