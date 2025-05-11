using UnityEngine;
using Unity.Netcode;

public class ToolSync : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform grapple;

    [SerializeField] private Vector3 localOffset;
    [SerializeField] private Quaternion localRotation = Quaternion.identity;

    private NetworkVariable<Vector3> syncedPosition = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<Quaternion> syncedRotation = new(writePerm: NetworkVariableWritePermission.Owner);

    void LateUpdate()
    {
        if (IsOwner)
        {
            // Local player updates their own tool position
            Vector3 desiredPosition = cameraTransform.TransformPoint(localOffset);
            Quaternion desiredRotation = cameraTransform.rotation * localRotation;

            gun.position = desiredPosition;
            gun.rotation = desiredRotation;

            grapple.position = desiredPosition;
            grapple.rotation = desiredRotation;

            // Update synced values for other clients
            syncedPosition.Value = desiredPosition;
            syncedRotation.Value = desiredRotation;
        }
        else
        {
            // Remote clients display synced tool transform
            gun.position = syncedPosition.Value;
            gun.rotation = syncedRotation.Value;

            grapple.position = syncedPosition.Value;
            grapple.rotation = syncedRotation.Value;
        }
    }
}
