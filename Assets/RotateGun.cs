using UnityEngine;
using Unity.Netcode;

public class RotateGun : NetworkBehaviour
{
    public Swinging swinging;
    
    private Quaternion desiredRotation;
    private float rotationSpeed = 5f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) {
        //     return;
        // }
        
        if (!swinging.IsSwinging()) {
            desiredRotation = transform.parent.rotation;
        }
        else {
            desiredRotation = Quaternion.LookRotation(swinging.GetSwingPoint() - transform.position);
        }
        transform.rotation = Quaternion.Lerp(transform.rotation, desiredRotation, Time.deltaTime * rotationSpeed);;
    }
}
