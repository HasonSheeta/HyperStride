using UnityEngine;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [SerializeField] private List<Transform> spawnPoints;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points set in SpawnManager!");
            return null;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
}
