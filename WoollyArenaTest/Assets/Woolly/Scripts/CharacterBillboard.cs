using UnityEngine;
using TMPro;
namespace WoollyArena {
[DefaultExecutionOrder(300)]public sealed class CharacterBillboard:MonoBehaviour {
 public CharacterVitals vitals;public ArenaPlayer player;public TMP_Text nameLabel,healthLabel;public RectTransform healthFill;public UnityEngine.UI.Image[] ammoPips;
 Camera view;
 void Awake(){view=Camera.main;}
 void LateUpdate(){if(!view)return;transform.rotation=view.transform.rotation;nameLabel.text=vitals.displayName;healthLabel.text=vitals.Health.ToString();healthFill.anchorMax=new Vector2(Mathf.Clamp01((float)vitals.Health/vitals.maxHealth),1);for(int i=0;i<ammoPips.Length;i++)ammoPips[i].color=i<player.weapon.Ammo?new Color(1,.82f,.3f):new Color(.22f,.19f,.25f);}
}
}
