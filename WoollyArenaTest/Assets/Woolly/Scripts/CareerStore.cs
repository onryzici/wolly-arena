using System;
namespace WoollyArena {
 public static class CareerStore {
  const string Key="Woolly.Career.v1";
  public static long Today=>DateTime.UtcNow.Ticks/TimeSpan.TicksPerDay;
  public static CareerProgress Load(){try{return UnityEngine.JsonUtility.FromJson<CareerProgress>(UnityEngine.PlayerPrefs.GetString(Key,""))??new CareerProgress();}catch(ArgumentException){return new CareerProgress();}}
  public static void Save(CareerProgress data){UnityEngine.PlayerPrefs.SetString(Key,UnityEngine.JsonUtility.ToJson(data));UnityEngine.PlayerPrefs.Save();}
 }
}
