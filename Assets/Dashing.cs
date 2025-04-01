using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dashing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerCam;
    private Rigidbody rb;
    private CharacterController pm;

    [Header("Dashing")]
    public float dashForce;
    public float dashUpwardForce;
    public float dashDuration;

    [Header("Cooldown")]
    public float dashCd;
    private float dashCdTimer;

    [Header("Input")]
    public KeyCode dashKey = KeyCode.E;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(dashKey)) {
            Dash();
        }
    }

    private void Dash() {
        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpwardForce;
    
        rb.AddForce(forceToApply, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash() {

    }
}
