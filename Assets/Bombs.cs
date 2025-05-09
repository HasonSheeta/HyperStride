using UnityEngine;
using Unity.Netcode;

public class Bombs : NetworkBehaviour
{
    public Rigidbody rb;
    public GameObject explosion;
    public LayerMask whatIsEnemies;

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
    // private NetworkVariable<int> maxCollisions = new NetworkVariable<int>(2);
    // private NetworkVariable<float> lastCollisionTime = new NetworkVariable<float>(-1f);
    // private NetworkVariable<float> collisionCooldown = new NetworkVariable<float>(0.1f);
    // private NetworkVariable<float> maxLifetime = new NetworkVariable<float>(4f);
    public bool explodeOnTouch = true;

    int collisions;
    PhysicsMaterial physicMat;

    public void Init(Vector3 dir)
    {
        direction.Value = dir;
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

    private void Explode() {
        if (explosion != null) {
            Instantiate(explosion, transform.position, Quaternion.identity);
        }

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        foreach (var enemyCollider in enemies)
        {
            Player player = enemyCollider.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(explosionDamage);
            }

            Rigidbody rb = enemyCollider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRange);
            }
        }

        Invoke("Delay", 0.05f);
    }

    private void Delay() {
        //Destroy(gameObject);

        
        RequestDestroyServerRpc();
        

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRange);
    }

    [ServerRpc]
    void RequestDestroyServerRpc()
    {
        if (IsServer)
        {
            GetComponent<NetworkObject>().Despawn(true); // Or Destroy(gameObject)
        }
    }

}
