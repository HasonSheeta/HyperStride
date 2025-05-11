using UnityEngine;
using Unity.Netcode;

public class Bombs : NetworkBehaviour
{
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;
    private SphereCollider sphereCollider;
    private Collider shooterCollider;
    private float colliderEnableDelay = 0.05f; // Ignore collision for this duration
    private float spawnTime;

    //stats
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;
    public float speed;
    private NetworkVariable<Vector3> direction = new NetworkVariable<Vector3>(writePerm: NetworkVariableWritePermission.Server);

    //damage
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;

    //lifetime
    public int maxCollisions;
    private float lastCollisionTime = -1f;
    public float collisionCooldown = 0.1f;
    public float maxLifetime;
    public bool explodeOnTouch = true;

    int collisions;
    PhysicsMaterial physicMat;

    [SerializeField] private AudioClip explosionSound;
    public AudioSource audioSource;
    private bool hasPlayedSound = false;

    public void Init(Vector3 dir, Collider shooterCol)
    {
        direction.Value = dir;
        spawnTime = Time.time;
        shooterCollider = shooterCol;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Setup();

        if (IsServer) {
            rb.isKinematic = false;
            rb.AddForce(direction.Value * speed, ForceMode.Impulse);
        }
        else {
            rb.isKinematic = true;
        }

        sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider != null)
        {
            sphereCollider.enabled = false;
            Invoke(nameof(EnableCollider), colliderEnableDelay);
        }
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Setup();
        //GetComponent<Rigidbody>().AddForce(direction * speed, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        // if (!IsOwner) {
        //     return;
        // }
        
        if (collisions > maxCollisions) {
            Explode();
        }

        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0) {
            Explode();
        }
    }

    private void Setup() {
        physicMat = new PhysicsMaterial();
        physicMat.bounciness = bounciness;
        physicMat.frictionCombine = PhysicsMaterialCombine.Minimum;
        physicMat.bounceCombine = PhysicsMaterialCombine.Maximum;

        GetComponent<SphereCollider>().material = physicMat;

        rb.useGravity = useGravity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && explodeOnTouch) {
            Explode();
            return;
        }

        if (Time.time - lastCollisionTime > collisionCooldown) {
            collisions++;
            lastCollisionTime = Time.time;
        }
    }

    private void EnableCollider()
    {
        if (sphereCollider != null)
        {
            sphereCollider.enabled = true;

            if (shooterCollider != null)
            {
                Physics.IgnoreCollision(sphereCollider, shooterCollider, true);
                Invoke(nameof(ReenableShooterCollision), colliderEnableDelay); // Delay can be tweaked
            }
        }
    }

    private void ReenableShooterCollision()
    {
        if (shooterCollider != null && sphereCollider != null)
        {
            Physics.IgnoreCollision(sphereCollider, shooterCollider, false);
        }
    }

    private void Explode() {
        PlayExplosionSound();
        
        if (IsServer) { //&& explosion != null
            // GameObject explosionInstance = Instantiate(explosion, transform.position, Quaternion.identity);
            // //explosionInstance.GetComponent<NetworkObject>().Spawn();
            // Destroy(explosionInstance, explosion.GetComponentInChildren<ParticleSystem>().main.duration);

            SpawnExplosionClientRpc(transform.position);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        if (IsServer) {
            foreach (var enemyCollider in enemies)
            {
                Player player = enemyCollider.GetComponent<Player>();
                if (player != null)
                {
                    player.TakeDamageFromExplosion(explosionDamage, OwnerClientId);
                }

                Rigidbody rb = enemyCollider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRange);
                }
            }
        }

        Invoke("Delay", 0.05f);
    }

    private void Delay() {
        RequestDestroyServerRpc();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    private void PlayExplosionSound() {
        PlayExplosionSoundClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestDestroyServerRpc()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true); // Or Destroy(gameObject)
        }
    }

    [ClientRpc]
    public void PlayExplosionSoundClientRpc()
    {
        if (hasPlayedSound) return;
        hasPlayedSound = true;
        
        if (explosionSound == null) return;

        GameObject soundObj = new GameObject("ExplosionSound");
        soundObj.transform.position = transform.position;

        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = explosionSound;
        source.spatialBlend = 1f; // 3D sound
        source.Play();

        Destroy(soundObj, explosionSound.length);
    }

    [ClientRpc]
    private void SpawnExplosionClientRpc(Vector3 position)
    {
        if (explosion == null) return;

        GameObject explosionInstance = Instantiate(explosion, position, Quaternion.identity);
        Destroy(explosionInstance, explosion.GetComponentInChildren<ParticleSystem>().main.duration);
    }

}
