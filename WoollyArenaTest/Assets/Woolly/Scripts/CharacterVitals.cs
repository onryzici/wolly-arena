using UnityEngine;
namespace WoollyArena {
public sealed class CharacterVitals:MonoBehaviour {
 public string displayName="WOOLLY";public int maxHealth=3000;public int Health {get;private set;}
 public int Armor;
 public int LastDamage {get;private set;}
 public float InvulnerableUntil {get;private set;}
 public event System.Action<Vector3> Damaged;
 public void ProtectFor(float seconds){InvulnerableUntil=Time.time+Mathf.Max(0,seconds);}
 void Awake(){Health=maxHealth;}
 public bool Damage(int damage,Vector3 direction=default){if(Health<=0||damage<=0||Time.time<InvulnerableUntil)return false;damage=Mathf.Max(1,Mathf.RoundToInt(damage*SurvivalBuild.ArmorMultiplier(Armor)));LastDamage=Mathf.Min(Health,damage);Health=Mathf.Max(0,Health-damage);Damaged?.Invoke(direction);return true;}
 public void RestoreHealth(int health){Health=Mathf.Clamp(health,0,maxHealth);}
 public void SetMaximum(int maximum){int previous=maxHealth;maxHealth=Mathf.Max(1,maximum);if(Health>0)Health=Mathf.Clamp(Health+maxHealth-previous,1,maxHealth);}
 public void ResetHealth(int maximum){maxHealth=Mathf.Max(1,maximum);Health=maxHealth;InvulnerableUntil=0;}
 public void Heal(int amount){Health=Mathf.Min(maxHealth,Health+Mathf.Max(0,amount));}
}
}
