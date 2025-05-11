using UnityEngine;
using Unity.Netcode;

public class Sliding : NetworkBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObject;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObject.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) {
            return;
        }
        
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0)) {
            StartSlide();
        }
        
        if (Input.GetKeyUp(slideKey) && pm.sliding) {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if (pm.sliding) {
            SlidingMovement();
        }
    }

    private void StartSlide() {
        pm.sliding = true;

        playerObject.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlideTime;
    }

    private void SlidingMovement() {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
    
        if (!pm.OnSlope() || rb.linearVelocity.y > -0.1f) {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            slideTimer -= Time.deltaTime;
        }
        else {
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0) {
            StopSlide();
        }
    }

    private void StopSlide() {
        pm.sliding = false;
        playerObject.localScale = new Vector3(playerObject.localScale.x, startYScale, playerObject.localScale.z);
    }
}
