using UnityEngine;
namespace WoollyArena {
public sealed class MobileControls:MonoBehaviour {
 public TouchStick move,aim;public ArenaPlayer player;public bool previewInEditor=true;
 public bool Active=>Application.isMobilePlatform||previewInEditor;
 void Awake(){if(aim)aim.buttonOnly=true;Application.targetFrameRate=60;Screen.sleepTimeout=SleepTimeout.NeverSleep;if(!Active)gameObject.SetActive(false);}
 void LateUpdate(){if(aim&&aim.knob)aim.knob.localScale=Vector3.one*(aim.Held?.9f:1);}
 public void Reload(){if(player&&!player.CombatPaused)player.weapon.Reload(Time.time);}
 void OnApplicationPause(bool paused){if(paused){if(move)move.ResetInput();if(aim)aim.ResetInput();}}
}
}
