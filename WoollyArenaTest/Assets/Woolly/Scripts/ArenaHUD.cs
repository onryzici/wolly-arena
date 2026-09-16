using UnityEngine;
using TMPro;
namespace WoollyArena {
public sealed class ArenaHUD:MonoBehaviour {
 public ArenaPlayer player;public TMP_Text ammo,status,stats;public RectTransform reticle;
 void Update(){if(!player)return;ammo.text=player.weapon.Reloading?"RELOADING  "+Mathf.RoundToInt(player.weapon.ReloadProgress(Time.time)*100)+"%":player.weapon.Ammo+" / "+player.weapon.Capacity;
 status.text=player.weapon.Reloading?"RELOAD":player.Speed>player.walkSpeed+.2f?"RUN":player.Speed>.15f?"WALK":"IDLE";
 stats.text="SHOTS  "+player.weapon.Shots+"     HITS  "+player.Hits;
 if(reticle)reticle.gameObject.SetActive(!Application.isMobilePlatform);
 if(reticle&&UnityEngine.InputSystem.Mouse.current!=null)reticle.position=UnityEngine.InputSystem.Mouse.current.position.ReadValue();}
}
}
