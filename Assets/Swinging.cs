using UnityEngine;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask grappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 30f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;

    [Header("Air Movement")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;
    
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (lr.material == null) {
            lr.material = new Material(Shader.Find("Unlit/Color"));
            lr.material.color = Color.red;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(swingKey)) {
            StartSwing();
        }
        if (Input.GetKeyUp(swingKey)) {
            StopSwing();
        }

        if (joint != null) {
            AirMovement();
        }
    }

    void LateUpdate()
    {
        DrawRope();
    }

    private void StartSwing() {
        pm.swinging = true;
        
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, grappleable)) {
            Debug.Log("Grapple point hit: " + hit.point);
            
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
        else {
            Debug.Log("Raycast did not hit anything.");
        }
    }

    void StopSwing() {
        pm.swinging = false;
        
        lr.positionCount = 0;
        Destroy(joint);
    }

    void DrawRope() {
        if (!joint) {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        Debug.Log("Current Grapple Position: " + currentGrapplePosition);
        
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
        
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        lr.startColor = Color.red;
        lr.endColor = Color.red;
    }

    private void AirMovement() {
        // Right
        if (Input.GetKey(KeyCode.D)) {
            rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        // Left
        if (Input.GetKey(KeyCode.A)) {
            rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        }
        // Forward
        if (Input.GetKey(KeyCode.W)) {
            rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime);
        }

        // Shorten Cable
        if (Input.GetKey(KeyCode.Space)) {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        // Extend Cable
        if (Input.GetKey(KeyCode.S)) {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }
}
