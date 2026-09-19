using UnityEngine;
using TMPro;
namespace WoollyArena {
public sealed class ArenaHUD:MonoBehaviour {
 public ArenaPlayer player;public TMP_Text ammo,status,stats;public RectTransform reticle;
 DodgeButton dodgeButton;
 void Start(){
  var parent=transform.Find("SafeArea");if(!parent)parent=transform;
  var go=new GameObject("Dodge",typeof(RectTransform),typeof(CanvasRenderer),typeof(DodgeButton));var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=Vector2.right;rect.anchoredPosition=new Vector2(-105,110);rect.sizeDelta=Vector2.one*108;
  var button=go.GetComponent<DodgeButton>();dodgeButton=button;button.player=player;button.raycastTarget=true;
  var textGo=new GameObject("Dodge key and cooldown",typeof(RectTransform));var label=textGo.AddComponent<TextMeshProUGUI>();label.rectTransform.SetParent(rect,false);label.rectTransform.anchoredPosition=new Vector2(0,-29);label.rectTransform.sizeDelta=new Vector2(88,24);label.font=ammo.font;label.fontSharedMaterial=ammo.fontSharedMaterial;label.fontSize=17;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;button.label=label;
 }
 void Update(){if(!player)return;ammo.text=player.weapon.Reloading?"RELOADING  "+Mathf.RoundToInt(player.weapon.ReloadProgress(Time.time)*100)+"%":player.weapon.Ammo+" / "+player.weapon.Capacity;
 status.text=player.Dodge&&player.Dodge.IsDodging?"DASH":player.weapon.Reloading?"RELOAD":player.Speed>player.walkSpeed+.2f?"RUN":player.Speed>.15f?"WALK":"IDLE";
 stats.text="SHOTS  "+(player.Loadout?player.Loadout.Shots:player.weapon.Shots)+"     HITS  "+player.Hits;
 if(player.Stats){
  ammo.text="WEAPONS "+player.Stats.Build.Weapons.Count+" / 6";
  var run=player.Stats.Run;stats.text="DEFEATS "+run.Kills;
  bool controls=run.Phase==SurvivalRun.RunPhase.Wave&&!run.IsPaused;
  if(dodgeButton)dodgeButton.gameObject.SetActive(controls);
  if(player.mobile&&player.mobile.move)player.mobile.move.gameObject.SetActive(controls);
  status.text=run.IsPaused?"PAUSED":run.Phase==SurvivalRun.RunPhase.LevelUp?"LEVEL UP":run.Phase==SurvivalRun.RunPhase.Upgrade?"SHOP":player.Dodge&&player.Dodge.IsDodging?"DASH":"AUTO ATTACK";
 }
 if(reticle)reticle.gameObject.SetActive(!Application.isMobilePlatform&&!player.autoCombat);
 if(reticle&&UnityEngine.InputSystem.Mouse.current!=null)reticle.position=UnityEngine.InputSystem.Mouse.current.position.ReadValue();}
}
}
