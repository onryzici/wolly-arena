using UnityEngine;
using UnityEngine.SceneManagement;
namespace WoollyArena {public sealed class ArenaLobbyReturn:MonoBehaviour {public void Return(){var run=Object.FindAnyObjectByType<SurvivalRun>();if(run){run.LeaveToLobby();return;}Time.timeScale=1;SceneManager.LoadSceneAsync("Lobby");}}}
