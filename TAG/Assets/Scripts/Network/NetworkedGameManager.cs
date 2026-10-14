using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkedGameManager : NetworkBehaviour
{
    public static NetworkedGameManager Instance { get; private set; }

    // Online equivalent of GameManager.GameStarted. Networked obstacles
    // will subscribe to this later, same OnEnable/OnDisable pattern.
    public static event Action<bool> OnlineGameStarted;

    public NetworkVariable<bool> gameStarted = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Round Reset")]
    [SerializeField] private NetworkedStar star;
    private int roundsPlayed;
    private bool roundResolving;

    private bool gameIsOver;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        gameStarted.OnValueChanged += HandleGameStartedChanged;

        // If we joined mid-game, fire the event for the current value too.
        if (gameStarted.Value) HandleGameStartedChanged(false, true);
    }

    public override void OnNetworkDespawn()
    {
        gameStarted.OnValueChanged -= HandleGameStartedChanged;
    }

    private void HandleGameStartedChanged(bool previous, bool current)
    {
        OnlineGameStarted?.Invoke(current);
    }

    public void ServerStartGame()
    {
        if (!IsServer) return;
        gameStarted.Value = true;
    }

    public void ServerPlayerReachedMaxScore(ulong winnerClientId)
    {
        if (!IsServer || roundResolving) return; // also stops two "wins" in one tick
        if (NetworkedGameMode.Instance == null || NetworkedPlayerManager.Instance == null) return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(winnerClientId, out var client)) return;
        NetworkObject winnerObject = client.PlayerObject;
        if (winnerObject == null) return;

        NetworkedPlayerData winnerData = winnerObject.GetComponent<NetworkedPlayerData>();
        if (winnerData == null) return;

        roundResolving = true;

        NetworkedPlayerManager.Instance.ServerSetAllCanMove(false);
        winnerData.ServerSetIsIt(false);
        winnerData.ServerAddRoundWin();
        roundsPlayed++;

        // Same maths as local: wins >= limit / 2 (float division).
        bool gameOver = winnerData.roundWins.Value >= NetworkedGameMode.Instance.RoundsToWin;

        Debug.Log($"[Round] winner client {winnerClientId}, wins {winnerData.roundWins.Value}, " +
                  $"round {roundsPlayed}, gameOver {gameOver}");

        RoundWonClientRpc(winnerData.profileIndex.Value, roundsPlayed, gameOver);

        // On game over, roundResolving stays true so no further round can start.
        if (gameOver) gameIsOver = true;
        else StartCoroutine(ServerRoundResetRoutine());
    }

    [ClientRpc]
    private void RoundWonClientRpc(int winnerProfileIndex, int roundNumber, bool gameOver)
    {
        if (NetworkedGameMode.Instance != null)
            NetworkedGameMode.Instance.ShowRoundWon(winnerProfileIndex, roundNumber, gameOver);
    }

    // Replaces local's MidCallbackRoutine + RoundSplashEnded.
    private IEnumerator ServerRoundResetRoutine()
    {
        float splash = NetworkedGameMode.Instance.RoundSplashDuration;
        if (splash <= 0f) splash = 2f; // fallback if the MMF reports no duration

        yield return new WaitForSeconds(splash * 0.5f);   // splash covers the screen here

        NetworkedPlayerManager.Instance.ServerResetPlayersForNewRound();
        if (star != null) star.ServerResetStar();

        yield return new WaitForSeconds(splash * 0.5f);

        NetworkedPlayerManager.Instance.ServerSetAllCanMove(true);
        roundResolving = false;
    }

    // Any player on the end screen can start a rematch. The server decides whether it's valid.
    [ServerRpc(RequireOwnership = false)]
    public void RequestReplayServerRpc()
    {
        if (!gameIsOver) return; // only valid on the end screen; also ignores repeat presses
        gameIsOver = false;

        NetworkedPlayerManager.Instance.ServerResetPlayersForNewMatch();

        // Reloads the scene on every machine. Players persist; scene objects are rebuilt.
        NetworkManager.Singleton.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}