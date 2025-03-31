using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float gravity = -9.8f;
    public float jumpSpeed = 10f;
    CharacterController characterController;

    public int maxJumps = 1;
    public int jumpsLeft = 1;
    public float maxDashes = 3f;
    public float dashesLeft = 3f;

    public Transform groundCheckTransform;
    public LayerMask terrainLayers;

    [Header("Audio")]
    public AudioSource audioSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        ApplyGravityWithCC();
    }

    void Awake() {
        characterController = GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider other)
    {
        
    }

    public void MoveWithCC(Vector3 direction) {
        characterController.Move(direction * speed * Time.deltaTime);
        transform.LookAt(transform.position + direction);
    }

    Vector3 gravityVelocity = Vector3.zero;
    public void ApplyGravityWithCC() {
        if (characterController.isGrounded && gravityVelocity.y < 0) {
            gravityVelocity = Vector3.zero;
            return;
        }

        gravityVelocity.y += gravity * Time.deltaTime;

        characterController.Move(gravityVelocity * Time.deltaTime);
    }

    public void Jump() {
        if (CreatureOnGround()) {
            jumpsLeft = maxJumps;
        }else if (jumpsLeft < 1) {
            return;
        }
        jumpsLeft--;
        if (gravityVelocity.y < 0) {
            gravityVelocity.y = 0;
        }
        gravityVelocity.y += jumpSpeed;
    }

    public void Dash() {
        if (dashesLeft <= 0) {
            return;
        }
        
    }

    public bool CreatureOnGround() {
        return Physics.OverlapSphere(groundCheckTransform.position, 0.75f, terrainLayers).Length > 0;
    }
}
