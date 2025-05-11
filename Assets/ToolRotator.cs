using UnityEngine;
using Unity.Netcode;

public class ToolRotator : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform grapple;

    [SerializeField] private Vector3 localOffset; // local position in camera space
    [SerializeField] private Quaternion localRotation = Quaternion.identity;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        if (!IsOwner) return;
        
        if (cameraTransform == null) return;

        // Update position and rotation relative to the camera
        Vector3 desiredPosition = cameraTransform.TransformPoint(localOffset);
        Quaternion desiredRotation = cameraTransform.rotation * localRotation;

        gun.position = desiredPosition;
        gun.rotation = desiredRotation;

        grapple.position = desiredPosition;
        grapple.rotation = desiredRotation;
    }
}
