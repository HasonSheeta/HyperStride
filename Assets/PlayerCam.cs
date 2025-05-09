using UnityEngine;
using Unity.Netcode;

public class PlayerCam : NetworkBehaviour
{
    public float sensX;
    public float sensY;
    
    public Transform orientation;

    float xRotation;
    float yRotation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) {
        //     Debug.Log("Exiting Camera Update");
        //     return;
        // }

        //Debug.Log($"Mouse X: {Input.GetAxisRaw("Mouse X")}, Mouse Y: {Input.GetAxisRaw("Mouse Y")}");
        
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
