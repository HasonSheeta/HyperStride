using UnityEngine;

public class CursorHandler : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotSpot = Vector2.zero;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        Cursor.visible = true;
    }
}
