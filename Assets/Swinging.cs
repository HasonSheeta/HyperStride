using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;

public class Swinging : NetworkBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask grappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 15f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Vector3 currentGrapplePosition;
    private Transform swingAnchorTransform; // test
    private Vector3 localSwingAnchorPoint; // test
    private NetworkVariable<Vector3> netGrappleStart = new NetworkVariable<Vector3>();
    private NetworkVariable<Vector3> netGrappleEnd = new NetworkVariable<Vector3>();
    private NetworkVariable<bool> isSwinging = new NetworkVariable<bool>();

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
        // if (!IsOwner) {
        //     return;
        // }
        
        if (joint != null && swingAnchorTransform != null) {
            swingPoint = swingAnchorTransform.TransformPoint(localSwingAnchorPoint);
        }
        
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

    private void StartSwing()
    {
        if (!IsOwner) return;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, grappleable))
        {
            // Run local logic for rope and physics (ONLY for the owner)
            SetupLocalSwing(hit.point, hit.transform);

            // Tell server to update shared swing point
            StartSwingServerRpc(hit.point, hit.transform.GetComponent<NetworkObject>()?.NetworkObjectId ?? 0);
        }
    }

    void StopSwing() {
        if (!IsOwner) {
            return;
        }

        pm.swinging = false;
        lr.positionCount = 0;

        if (joint != null) {
            Destroy(joint);
            joint = null;
        }

        StopSwingServerRpc();
    }

    void DrawRope() {
        if (IsOwner) {
            if (joint == null) return;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }
        else {
            if (!isSwinging.Value) {
                lr.positionCount = 0;
                return;
            }

            lr.positionCount = 2;
            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, netGrappleEnd.Value, Time.deltaTime * 8f);

            //Debug.Log("Current Grapple Position: " + currentGrapplePosition);
        
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        
            lr.startWidth = 0.1f;
            lr.endWidth = 0.1f;

            lr.startColor = Color.red;
            lr.endColor = Color.red;
        }
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
        if (Input.GetKey(KeyCode.S)) {
            rb.AddForce(-orientation.forward * forwardThrustForce * Time.deltaTime);
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
        if (Input.GetKey(KeyCode.C)) {
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendCableSpeed;

            joint.maxDistance = extendedDistanceFromPoint * 0.8f;
            joint.minDistance = extendedDistanceFromPoint * 0.25f;
        }
    }

    public bool IsSwinging() {
        return joint != null;
    }

    public Vector3 GetSwingPoint() {
        return swingPoint;
    }

    private void SetupLocalSwing(Vector3 hitPoint, Transform anchorTransform)
    {
        if (joint != null) {
            Destroy(joint);
        }
        
        swingAnchorTransform = anchorTransform;
        localSwingAnchorPoint = anchorTransform.InverseTransformPoint(hitPoint);
        swingPoint = hitPoint;

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
        pm.swinging = true;
    }

    [ServerRpc]
    private void StartSwingServerRpc(Vector3 swingPosition, ulong anchorObjectId)
    {
        netGrappleStart.Value = gunTip.position;
        netGrappleEnd.Value = swingPosition;
        isSwinging.Value = true;
    }

    [ServerRpc]
    private void StopSwingServerRpc() {
        pm.swinging = false;

        isSwinging.Value = false;
        netGrappleStart.Value = Vector3.zero;
        netGrappleEnd.Value = Vector3.zero;
        
        lr.positionCount = 0;
        Destroy(joint);
    }
}
