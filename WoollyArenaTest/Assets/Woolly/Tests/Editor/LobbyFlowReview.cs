using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace WoollyArena.Editor
{
    [InitializeOnLoad]
    public static class LobbyFlowReview
    {
        const string Active = "Woolly.LobbyFlowReview.Active";
        const string Result = "Logs/lobby-flow-review.txt";
        static int step;
        static double started;
        static double next;

        static LobbyFlowReview()
        {
            if (SessionState.GetBool(Active, false))
            {
                started = EditorApplication.timeSinceStartup;
                next = started + 3;
                EditorApplication.update += Tick;
            }
        }

        public static void Run()
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Result, "Lobby interaction review\n");
            EditorSceneManager.OpenScene("Assets/Woolly/Scenes/Lobby.unity");
            var gameView = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
            EditorWindow.GetWindow(gameView).Focus();
            SessionState.SetBool(Active, true);
            started = EditorApplication.timeSinceStartup;
            next = started + 3;
            step = 0;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            EditorApplication.EnterPlaymode();
        }

        static void Assert(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
            File.AppendAllText(Result, "PASS " + message + "\n");
        }

        static Button Button(LobbyScreen lobby, string name)
        {
            return lobby.GetComponentsInChildren<Button>().First(b => b.name == name);
        }

        static void Tick()
        {
            if (!SessionState.GetBool(Active, false)) { EditorApplication.update -= Tick; return; }
            try
            {
                if (EditorApplication.timeSinceStartup - started > 60) throw new TimeoutException("Lobby review timed out");
                if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
                var lobby = Object.FindAnyObjectByType<LobbyScreen>();
                switch (step)
                {
                    case 0:
                        Assert(lobby && lobby.hero && lobby.presentationCamera, "Lobby and hero loaded");
                        Button(lobby, "Settings").onClick.Invoke();
                        Assert(lobby.transform.Find("Modal/Dialog"), "Settings opens a dialog");
                        float volume = AudioListener.volume;
                        Button(lobby, "Sound").onClick.Invoke();
                        Assert(AudioListener.volume != volume, "Sound toggle changes audio");
                        Button(lobby, "Sound").onClick.Invoke();
                        Assert(AudioListener.volume == volume, "Sound toggle restores audio");
                        ScreenCapture.CaptureScreenshot("Logs/lobby-settings-live.png");
                        step++; next = EditorApplication.timeSinceStartup + 2;
                        break;
                    case 1:
                        Button(lobby, "Close").onClick.Invoke();
                        Assert(!lobby.transform.Find("Modal").gameObject.activeSelf, "Close dismisses modal");
                        Button(lobby, "Quests").onClick.Invoke();
                        Assert(lobby.GetComponentsInChildren<TMPro.TMP_Text>().Any(t => t.text == "GÖREVLER"), "Quest navigation opens the correct panel");
                        lobby.ClosePopup();
                        step++; next = EditorApplication.timeSinceStartup + 1;
                        break;
                    case 2:
                        Button(lobby, "Play").onClick.Invoke();
                        Assert(!Button(lobby, "Play").interactable, "Play blocks duplicate loading");
                        step++; next = EditorApplication.timeSinceStartup + 4;
                        break;
                    case 3:
                        Assert(SceneManager.GetActiveScene().name == "TrainingArena", "Play opens the existing arena");
                        var director = Object.FindAnyObjectByType<EnemySpawnDirector>();
                        Assert(director && director.Enemies.Count == 4, "All four arena enemies spawn");
                        var back = Object.FindAnyObjectByType<ArenaLobbyReturn>();
                        Assert(back != null, "Arena has a lobby return button");
                        back.GetComponent<Button>().onClick.Invoke();
                        step++; next = EditorApplication.timeSinceStartup + 4;
                        break;
                    case 4:
                        Assert(SceneManager.GetActiveScene().name == "Lobby" && lobby, "Arena return opens the redesigned lobby");
                        Assert(Button(lobby, "Play").interactable, "Play is available after returning");
                        ScreenCapture.CaptureScreenshot("Logs/lobby-live.png");
                        File.AppendAllText(Result, "COMPLETE: left in the redesigned lobby in Play mode.\n");
                        Debug.Log("LOBBY_FLOW_REVIEW_OK");
                        Finish();
                        break;
                }
            }
            catch (Exception error)
            {
                File.AppendAllText(Result, "FAIL " + error + "\n");
                Debug.LogException(error);
                Finish();
            }
        }

        static void Finish()
        {
            SessionState.SetBool(Active, false);
            EditorApplication.update -= Tick;
        }
    }
}
