using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Player player;
    public Transform cameraTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(cameraTransform.forward);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;

        Vector3 cameraRight = cameraTransform.right;
        cameraRight.y = 0;

        Vector3 finalMovement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) {
            finalMovement += cameraForward;
        }
        if (Input.GetKey(KeyCode.S)) {
            finalMovement -= cameraForward;
        }
        if (Input.GetKey(KeyCode.A)) {
            finalMovement -= cameraRight;
        }
        if (Input.GetKey(KeyCode.D)) {
            finalMovement += cameraRight;
        }
        
        finalMovement.Normalize();

        player.MoveWithCC(finalMovement);

        if (Input.GetKeyDown(KeyCode.Space)) {
            player.Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            player.Dash();
        }
    }
}
