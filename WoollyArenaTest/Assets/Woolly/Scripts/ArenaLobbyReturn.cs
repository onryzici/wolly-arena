using UnityEngine;
using UnityEngine.SceneManagement;
namespace WoollyArena {public sealed class ArenaLobbyReturn:MonoBehaviour {public void Return(){Time.timeScale=1;SceneManager.LoadSceneAsync("Lobby");}}}
