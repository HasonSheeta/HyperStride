using UnityEngine;

public class AutoDestroyParticle : MonoBehaviour
{
    private ParticleSystem ps;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ps && !ps.IsAlive()) {
            Destroy(gameObject);
        }
    }
}
