using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WoollyArena.Editor
{
    [InitializeOnLoad]
    public static class DodgeReview
    {
        const string Active = "Woolly.DodgeReview.Active";
        static DodgeReview()
        {
            if (SessionState.GetBool(Active, false)) EditorApplication.update += StartProbe;
        }
        [MenuItem("Woolly/Review Dash")]
        public static void Run()
        {
            EditorSceneManager.OpenScene("Assets/Woolly/Scenes/TrainingArena.unity");
            Object.FindAnyObjectByType<EnemySpawnDirector>().survivalMode = false;
            EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
            SessionState.SetBool(Active, true);
            EditorApplication.update -= StartProbe;
            EditorApplication.update += StartProbe;
            EditorApplication.EnterPlaymode();
        }
        static void StartProbe()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling) return;
            var player = Object.FindAnyObjectByType<ArenaPlayer>();
            if (!player) return;
            SessionState.SetBool(Active, false);
            EditorApplication.update -= StartProbe;
            player.gameObject.AddComponent<DodgeProbe>();
        }
    }
}
