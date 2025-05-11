using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private float roundDuration = 300f;
    [SerializeField] private GameObject winCanvas;
    [SerializeField] private TMP_Text winText;
    private NetworkVariable<float> timeRemaining = new(writePerm: NetworkVariableWritePermission.Server);

    public float TimeRemaining => timeRemaining.Value;

    public static bool RoundActive { get; private set; } = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            timeRemaining.Value = roundDuration;
            RoundActive = true;
        }
    }

    private void Start() {
        if (winCanvas != null) {
            winCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsServer || !RoundActive) return;

        timeRemaining.Value -= Time.deltaTime;

        if (timeRemaining.Value <= 0)
        {
            timeRemaining.Value = 0;
            HandleTieRoundServerRpc();
        }
    }

    public float GetTimeRemaining() {
        return timeRemaining.Value;
    }

    public void TriggerRoundEnd(ulong winningPlayerId) {
        if (IsServer && RoundActive == true) {
            RoundActive = false;
            //Debug.Log("In TriggerRoundEnd");
            StartCoroutine(SlowMoAndRespawn());
        }
    }

    private void TriggerGameOver(ulong winnerId) {
        StartCoroutine(GameOverSequence(winnerId));
    }

    private IEnumerator SlowMoAndRespawn() {
        // Slow down time
        SetTimeScaleClientRpc(0.3f);

        yield return new WaitForSecondsRealtime(2f); // Wait real seconds, not scaled time

        // Reset time
        SetTimeScaleClientRpc(1f);

        Debug.Log("Reset Time Scale");

        // Respawn all players
        foreach (var player in FindObjectsOfType<Player>())
        {
            Debug.Log($"Respawning player: {player.OwnerClientId}");
            player.ResetPlayerServerRpc();
        }
        Debug.Log("Respawn players");

        // Restart round
        timeRemaining.Value = roundDuration;
        RoundActive = true;      
    }

    private IEnumerator GameOverSequence(ulong winnerId) {
        SetTimeScaleClientRpc(0.3f);

        yield return new WaitForSecondsRealtime(0.5f);

        ShowWinCanvasClientRpc(winnerId);

        yield return new WaitForSecondsRealtime(2f);

        SetTimeScaleClientRpc(1f);

        ReturnClientsToMenuClientRpc();

        yield return new WaitForSecondsRealtime(1.5f);

        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    [ClientRpc]
    private void SetTimeScaleClientRpc(float scale) {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerDiedServerRpc(ulong deadPlayerId)
    {
        ulong winnerId = GetOpponentId(deadPlayerId);
        Player winner = GetPlayerByClientId(winnerId);
        if (winner != null)
        {
            winner.roundWins.Value++;

            if (winner.roundWins.Value >= 5) {
                TriggerGameOver(winnerId);
            }
            else {
                EndRoundServerRpc($"Player {winnerId} wins", winnerId);
            }
        }

        EndRoundServerRpc($"Player {winnerId} wins", winnerId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EndRoundServerRpc(string reason, ulong winningPlayerId)
    {
        //RoundActive = true;
        Debug.Log("Round ended: " + reason);
        TriggerRoundEnd(winningPlayerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void HandleTieRoundServerRpc() {
        RoundActive = true;
        Debug.Log("Round ended in a tie");
        NotifyClientsOfTieClientRpc();
    }

    [ClientRpc]
    private void NotifyClientsOfTieClientRpc()
    {
        Debug.Log("Round ended in a tie (Client-side).");
        // You could show a message in the player's UI here
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.IsOwner)
            {
                player.ShowTieMessage(); // implement this method in Player
            }
        }
    }

    [ClientRpc]
    private void ShowWinCanvasClientRpc(ulong winnerId) {
        if (winCanvas != null && winText != null)
        {  
            winCanvas.SetActive(true);
            string result = winnerId == 0 ? "Host Wins!\nReturning to Main Menu" : "Client Wins!\nReturning to Main Menu";
            winText.text = result;
        }
    }

    [ClientRpc]
    private void ReturnClientsToMenuClientRpc()
    {
        if (!IsHost) // only clients should execute this
        {
            NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    private ulong GetOpponentId(ulong playerId) {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != playerId)
                return client.ClientId;
        }
        return 0;
    }

    private Player GetPlayerByClientId(ulong clientId) {
        foreach (var player in FindObjectsOfType<Player>())
        {
            if (player.OwnerClientId == clientId)
                return player;
        }
        return null;
    }
}
