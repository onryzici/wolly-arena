using UnityEngine;
using System.Collections.Generic;
namespace WoollyArena {
 public sealed class PlayableCharacter:MonoBehaviour {
  public static int Selected {get=>Mathf.Clamp(PlayerPrefs.GetInt("Woolly.Character",0),0,1);set{PlayerPrefs.SetInt("Woolly.Character",Mathf.Clamp(value,0,1));PlayerPrefs.Save();}}
  public static int Active {get;private set;}
  public static string Title(int id)=>id==1?"PUNK VERA":"WOOLLY";
  public static string Role(int id)=>id==1?"MUŞTA DÖVÜŞÇÜSÜ":"REVOLVER USTASI";
  readonly List<Material> accessoryMaterials=new List<Material>();
  Material bodyMaterial,outlineMaterial;AnimatorOverrideController controller;
  public static void ApplyToPlayer(ArenaPlayer player){
   int id=Selected;if(SurvivalRun.CheckpointsEnabled&&RunSession.ResumeRequested&&RunCheckpointStore.TryLoad(RunSession.SavePath,out var saved))id=saved.Character;
   Active=id;if(id==0)return;
   var old=player.visual;var avatar=Create(old.parent,player.animator.runtimeAnimatorController);if(!avatar)return;
   avatar.transform.SetLocalPositionAndRotation(old.localPosition,old.localRotation);avatar.transform.localScale=old.localScale;
   old.gameObject.SetActive(false);player.visual=avatar.transform;player.animator=avatar;
   var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(avatar.transform,false);muzzle.localPosition=new Vector3(0,.9f,.35f);player.muzzle=muzzle;
   player.GetComponent<CharacterVitals>().displayName="PUNK VERA";
  }
  public static Animator Create(Transform parent,RuntimeAnimatorController template,bool lobby=false){
   string resource=lobby?"Characters/PunkVeraLobby":"Characters/PunkVera";
   var prefab=Resources.Load<GameObject>(resource);var clips=Resources.LoadAll<AnimationClip>(resource);
   AnimationClip Find(string name){foreach(var clip in clips)if(!clip.name.StartsWith("__preview__")&&clip.name.EndsWith(name,System.StringComparison.OrdinalIgnoreCase))return clip;return null;}
   var idle=Find("Idle");var walk=lobby?idle:Find("Walk");var run=lobby?idle:Find("Run");
   if(!prefab||!template||!idle||!walk||!run){Debug.LogError("Punk Vera model or Idle/Walk/Run clips have not imported correctly.");return null;}
   var instance=Instantiate(prefab,parent,false);instance.name="Punk Vera";var owner=instance.AddComponent<PlayableCharacter>();
   owner.bodyMaterial=new Material(Resources.Load<Shader>("CombatModel"));owner.bodyMaterial.SetTexture("_BaseMap",Resources.Load<Texture2D>("Characters/PunkVera_BaseColor"));owner.bodyMaterial.color=Color.white;
   owner.outlineMaterial=new Material(Resources.Load<Shader>("PlayableOutline"));owner.outlineMaterial.SetColor("_OutlineColor",new Color(.10f,.065f,.13f));owner.outlineMaterial.SetFloat("_Width",.012f);
   foreach(var source in instance.GetComponentsInChildren<SkinnedMeshRenderer>()){
    Material surface=owner.bodyMaterial;
    if(source.name.StartsWith("Vera_")){
     surface=new Material(Resources.Load<Shader>("CombatModel"));
     surface.color=source.name.Contains("Knuckles")?new Color(.82f,.54f,.16f):source.name.Contains("Pink")?new Color(.71f,.09f,.28f):source.name.Contains("Grip")?new Color(.11f,.065f,.13f):new Color(.80f,.77f,.69f);
     owner.accessoryMaterials.Add(surface);
    }
    var materials=new Material[source.sharedMesh.subMeshCount];for(int i=0;i<materials.Length;i++)materials[i]=surface;source.sharedMaterials=materials;
    var edge=new GameObject(source.name+"_Outline");edge.transform.SetParent(source.transform,false);var outline=edge.AddComponent<SkinnedMeshRenderer>();outline.sharedMesh=source.sharedMesh;outline.bones=source.bones;outline.rootBone=source.rootBone;outline.localBounds=source.localBounds;
    var ink=new Material[materials.Length];for(int i=0;i<ink.Length;i++)ink[i]=owner.outlineMaterial;outline.sharedMaterials=ink;outline.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;outline.receiveShadows=false;
   }
   foreach(var t in instance.GetComponentsInChildren<Transform>())t.gameObject.layer=parent?parent.gameObject.layer:0;
   var animator=instance.GetComponent<Animator>();if(!animator)animator=instance.AddComponent<Animator>();
   owner.controller=new AnimatorOverrideController(template);var overrides=new List<KeyValuePair<AnimationClip,AnimationClip>>();
   foreach(var clip in template.animationClips){string name=clip.name.ToLowerInvariant();overrides.Add(new KeyValuePair<AnimationClip,AnimationClip>(clip,name.Contains("walk")?walk:name.Contains("run")?run:idle));}
   owner.controller.ApplyOverrides(overrides);animator.runtimeAnimatorController=owner.controller;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
   if(animator.layerCount>1)animator.SetLayerWeight(1,0);return animator;
  }
  void OnDestroy(){foreach(var material in accessoryMaterials)if(material)Destroy(material);if(bodyMaterial)Destroy(bodyMaterial);if(outlineMaterial)Destroy(outlineMaterial);if(controller)Destroy(controller);}
 }
}
