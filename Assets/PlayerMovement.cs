using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float swingSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState {
        walking,
        sprinting,
        crouching,
        sliding,
        swinging,
        air
    }
    
    public bool sliding;
    public bool swinging;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) {
            return;
        }
        
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        MyInput();
        SpeedControl();
        StateHandler();

        if (grounded) {
            rb.linearDamping = groundDrag;
        } else {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded) {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(crouchKey)) {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler() {
        if (swinging) {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;
        }
        else if (sliding) {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f) {
                desiredMoveSpeed = slideSpeed;
            }
            else {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        else if (Input.GetKey(crouchKey)) {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey)) {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded) {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else {
            state = MovementState.air;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0) {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed() {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference) {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time/difference);
            
            if (OnSlope()) {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else {
                time += Time.deltaTime * speedIncreaseMultiplier;
            }

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer() {
        if (swinging) {
            return;
        }
        
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0) {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        else if (grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        } else if (!grounded) {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        if (OnSlope() && !exitingSlope) {
            if (rb.linearVelocity.magnitude > moveSpeed) {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
            }
        }
        else {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > moveSpeed) {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
        
    }

    private void Jump() {
        exitingSlope = true;
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope() {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction) {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
