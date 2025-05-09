using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(3);
    private bool isDead;
    private float lastCollisionTime = -1f;
    public float collisionCooldown = 0.1f;
    [SerializeField] private GameObject camera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            //Debug.Log("IsOwner is FALSE");
            DisableRemotePlayerControls();
            camera.tag = "Untagged";
        }
        else
        {
            //Debug.Log("IsOwner is TRUE");
            camera.tag = "MainCamera";
            camera.gameObject.SetActive(true);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("NetworkObject spawned: " + GetComponent<NetworkObject>().IsSpawned);
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage) {
        if (Time.time - lastCollisionTime > collisionCooldown) {
            health.Value -= damage;
            lastCollisionTime = Time.time;
        }

        Debug.Log("A player took damage\nCurrent health: " + health.Value);

        if (health.Value <= 0) {
            isDead = true;
        }
    }

    private void DisableRemotePlayerControls()
    {
        if (TryGetComponent(out PlayerCam cam)) cam.enabled = false;
        if (TryGetComponent(out PlayerMovement move)) move.enabled = false;

        // Disable scripts on children
        foreach (var swinging in GetComponentsInChildren<Swinging>()) swinging.enabled = false;
        foreach (var rot in GetComponentsInChildren<RotateGun>()) rot.enabled = false;
        foreach (var gun in GetComponentsInChildren<GunProjectile>()) gun.enabled = false;

        camera.SetActive(false);
    }

}
