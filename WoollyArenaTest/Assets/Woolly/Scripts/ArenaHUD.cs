using UnityEngine;
using TMPro;
namespace WoollyArena {
public sealed class ArenaHUD:MonoBehaviour {
 public ArenaPlayer player;public TMP_Text ammo,status,stats;public RectTransform reticle;
 void Start(){
  var parent=transform.Find("SafeArea");if(!parent)parent=transform;
  var go=new GameObject("Dodge",typeof(RectTransform),typeof(CanvasRenderer),typeof(DodgeButton));var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=Vector2.right;rect.anchoredPosition=new Vector2(-350,245);rect.sizeDelta=Vector2.one*108;
  var button=go.GetComponent<DodgeButton>();button.player=player;button.raycastTarget=true;
  var textGo=new GameObject("Dodge key and cooldown",typeof(RectTransform));var label=textGo.AddComponent<TextMeshProUGUI>();label.rectTransform.SetParent(rect,false);label.rectTransform.anchoredPosition=new Vector2(0,-29);label.rectTransform.sizeDelta=new Vector2(88,24);label.font=ammo.font;label.fontSharedMaterial=ammo.fontSharedMaterial;label.fontSize=17;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;button.label=label;
 }
 void Update(){if(!player)return;ammo.text=player.weapon.Reloading?"RELOADING  "+Mathf.RoundToInt(player.weapon.ReloadProgress(Time.time)*100)+"%":player.weapon.Ammo+" / "+player.weapon.Capacity;
 status.text=player.Dodge&&player.Dodge.IsDodging?"DODGE":player.weapon.Reloading?"RELOAD":player.Speed>player.walkSpeed+.2f?"RUN":player.Speed>.15f?"WALK":"IDLE";
 stats.text="SHOTS  "+player.weapon.Shots+"     HITS  "+player.Hits;
 if(reticle)reticle.gameObject.SetActive(!Application.isMobilePlatform);
 if(reticle&&UnityEngine.InputSystem.Mouse.current!=null)reticle.position=UnityEngine.InputSystem.Mouse.current.position.ReadValue();}
}
}
