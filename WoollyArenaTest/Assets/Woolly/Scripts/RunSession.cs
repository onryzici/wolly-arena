using System;
using System.IO;
using UnityEngine;

namespace WoollyArena
{
    public static class RunSession
    {
        public static bool ResumeRequested;
        public static string SavePath => Path.Combine(Application.persistentDataPath, "survival-run-v1.sav");
        public static bool HasCheckpoint => RunCheckpointStore.TryLoad(SavePath, out _);
        public static bool SoundEnabled => PlayerPrefs.GetInt("Woolly.Sound", 1) != 0;
        public static void ApplySound() => AudioListener.volume = SoundEnabled ? 1 : 0;
        public static void ToggleSound() { PlayerPrefs.SetInt("Woolly.Sound", SoundEnabled ? 0 : 1); PlayerPrefs.Save(); ApplySound(); }
        public static string ClearCheckpoint()
        {
            try { RunCheckpointStore.Clear(SavePath); return null; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { return "Kayıt silinemedi. Cihazın depolama alanını kontrol et."; }
        }
    }
}
