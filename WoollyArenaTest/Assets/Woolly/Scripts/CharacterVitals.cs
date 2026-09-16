using UnityEngine;
namespace WoollyArena {
public sealed class CharacterVitals:MonoBehaviour {
 public string displayName="WOOLLY";public int maxHealth=3000;public int Health {get;private set;}
 public float InvulnerableUntil {get;private set;}
 public event System.Action<Vector3> Damaged;
 public void ProtectFor(float seconds){InvulnerableUntil=Time.time+Mathf.Max(0,seconds);}
 void Awake(){Health=maxHealth;}
 public bool Damage(int damage,Vector3 direction=default){if(Health<=0||damage<=0||Time.time<InvulnerableUntil)return false;Health=Mathf.Max(0,Health-damage);Damaged?.Invoke(direction);return true;}
 public void Heal(int amount){Health=Mathf.Min(maxHealth,Health+Mathf.Max(0,amount));}
}
}
