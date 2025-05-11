using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform), typeof(NetworkObject), typeof(Animator))]
public class NetworkMovingPlatform : NetworkBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnNetworkSpawn()
    {
        // This runs only when the object is fully initialized on the network
        if (!IsServer && animator != null)
        {
            animator.enabled = false;
        }
    }

    private void Update()
    {
        if (!IsServer || animator == null)
            return;

        // Control animation only from server
        animator.SetBool("IsMoving", true); // or use your own animation triggers
    }
}
