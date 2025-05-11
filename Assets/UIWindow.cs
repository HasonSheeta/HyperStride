using UnityEngine;

public class UIWindow : MonoBehaviour
{
    public void OpenWindow() {
        Debug.Log("Opening Window");
        GetComponent<Canvas>().enabled = true;
    }

    public void CloseWindow() {
        Debug.Log("Closing Window");
        GetComponent<Canvas>().enabled = false;
    }
}
