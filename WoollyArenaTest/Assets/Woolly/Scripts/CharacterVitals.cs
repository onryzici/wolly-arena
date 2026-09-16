using UnityEngine;
namespace WoollyArena {
public sealed class CharacterVitals:MonoBehaviour {
 public string displayName="WOOLLY";public int maxHealth=3000;public int Health {get;private set;}
 public float InvulnerableUntil {get;private set;}
 public void ProtectFor(float seconds){InvulnerableUntil=Time.time+Mathf.Max(0,seconds);}
 void Awake(){Health=maxHealth;}
 public void Damage(int damage){if(Time.time<InvulnerableUntil)return;Health=Mathf.Max(0,Health-Mathf.Max(0,damage));}
 public void Heal(int amount){Health=Mathf.Min(maxHealth,Health+Mathf.Max(0,amount));}
}
}
