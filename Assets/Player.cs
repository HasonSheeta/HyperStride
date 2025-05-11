using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;

public class Player : NetworkBehaviour
{
    public NetworkVariable<int> health = new NetworkVariable<int>(10);
    private bool isDead;
    private float lastCollisionTime = -1f;
    public float collisionCooldown = 0.1f;
    [SerializeField] private GameObject camera;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text hostWinsText;
    [SerializeField] private TMP_Text clientWinsText;
    private int localRoundWins = 0;
    public NetworkVariable<int> roundWins = new NetworkVariable<int>(0);
    [SerializeField] private AudioClip gotHitSound;      
    [SerializeField] private AudioClip scoreHitSound; 
    [SerializeField] private AudioSource audioSource;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            //Debug.Log("IsOwner is FALSE");
            DisableRemotePlayerControls();
            camera.tag = "Untagged";

            // Ensure AudioListener is disabled for remote players
            AudioListener audioListener = camera.GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }
        }
        else
        {
            Transform spawnPoint = SpawnManager.Instance.GetRandomSpawnPoint();
            if (spawnPoint != null)
            {
                transform.position = spawnPoint.position;
                transform.rotation = spawnPoint.rotation;
            }

            //Debug.Log("IsOwner is TRUE");
            camera.tag = "MainCamera";
            camera.gameObject.SetActive(true);

            // Enable AudioListener only for the local player
            AudioListener audioListener = camera.GetComponentInChildren<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = true;  // Enable AudioListener for the local player
            }

        }

        foreach (var player in FindObjectsOfType<Player>())
        {
            player.roundWins.OnValueChanged += (oldVal, newVal) =>
            {
                UpdateAllWinsUI();
            };
        }

        UpdateAllWinsUI();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("NetworkObject spawned: " + GetComponent<NetworkObject>().IsSpawned);
    }


    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || GameManager.Instance == null) {
            return;
        }

        float time = GameManager.Instance.TimeRemaining;
        timerText.text = Mathf.CeilToInt(time).ToString();
    }

    public void TakeDamage(int damage) {
        if (GameManager.RoundActive.Equals(true) && Time.time - lastCollisionTime > collisionCooldown) {
            health.Value -= damage;
            lastCollisionTime = Time.time;
        }

        Debug.Log("A player took damage\nCurrent health: " + health.Value);

        if (isDead == false && health.Value <= 0) {
            isDead = true;
            HandleDeath();
        }
    }

    private void HandleDeath() {
        Debug.Log("Player is dead");

        if (IsServer)
        {
            GameManager.Instance.PlayerDiedServerRpc(OwnerClientId);
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

    public void TakeDamageFromExplosion(int damage, ulong sourceClientId) {
        if (GameManager.RoundActive.Equals(false)) {
            return;
        }

        TakeDamage(damage);

        if (IsOwner) {
        // This is the player who got hit
            PlayGotHitSound();
        }

        if (NetworkManager.Singleton.LocalClientId == sourceClientId)
        {
            // This is the player who scored the hit
            PlayScoreHitSound();
        }

        Debug.Log($"Player hit by client {sourceClientId}");
    }

    private void PlayGotHitSound()
    {
        if (gotHitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gotHitSound);
        }
    }

    private void PlayScoreHitSound()
    {
        if (scoreHitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreHitSound);
        }
    }

    private void UpdateAllWinsUI()
    {
        var players = FindObjectsOfType<Player>();
        int hostWins = 0, clientWins = 0;

        foreach (var player in players)
        {
            if (player.OwnerClientId == 0)
                hostWins = player.roundWins.Value;
            else
                clientWins = player.roundWins.Value;
        }

        // Update UI for each player
        foreach (var player in players)
        {
            if (player.IsOwner)
            {
                player.hostWinsText.text = $"{hostWins}";
                player.clientWinsText.text = $"{clientWins}";
            }
        }
    }

    public void ShowTieMessage()
    {
        // Optionally fade in or display a TMP text overlay
        Debug.Log("Tie round! No winners.");
        // You could instantiate a prefab or enable a Canvas here
    }

    [ClientRpc]
    void RespawnClientRpc(Vector3 pos, Quaternion rot)
    {
        Debug.Log("RespawnClientRpc");
        if (IsOwner)
        {
            Debug.Log("Transform pos for respawn");
            transform.position = pos;
            transform.rotation = rot;
            collisionCooldown = 0.1f;
        }
    }

    // [ClientRpc]
    // public void IncrementRoundWinsClientRpc(ulong winnerClientId) {
    //     if (NetworkManager.Singleton.LocalClientId != winnerClientId) {
    //         return;
    //     }

    //     localRoundWins++;
    //     hostWinsText.text = $"{localRoundWins}";
    // }

    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerServerRpc()
    {
        health.Value = 3;
        //

        foreach (var player in FindObjectsOfType<Player>())
        {
            Debug.Log($"Health of Player {player.OwnerClientId}: " + health.Value);
        }

        //
        isDead = false;

        Transform spawnPoint = SpawnManager.Instance.GetRandomSpawnPoint();
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }

        RespawnClientRpc(spawnPoint.position, spawnPoint.rotation);
    }
}
