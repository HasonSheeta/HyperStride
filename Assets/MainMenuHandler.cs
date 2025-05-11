using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    public static bool IsHost = false;
    public static bool IsClient = false;
    
    public void HostGame() {
        IsHost = true;
        SceneManager.LoadScene("Game");
    }

    public void JoinGame() {
        IsClient = true;
        SceneManager.LoadScene("Game");
    }

    public void QuitGame() {
        Application.Quit();
    }
}