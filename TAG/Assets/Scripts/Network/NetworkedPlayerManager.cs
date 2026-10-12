using MoreMountains.Feedbacks;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkedPlayerManager : MonoBehaviour
{
    public static NetworkedPlayerManager Instance;

    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerProfile> playerProfiles;

    [SerializeField] private List<NetworkedScoreCardUI> scoreCardSlots;
    [SerializeField] private List<NetworkedReadyUI> readyUISlots;
    [SerializeField] private int minPlayersToStart = 2;

    [Header("Player Labels")]
    [SerializeField] private List<PlayerLabelUI> playerLabels;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Canvas labelCanvas;

    [Header("Feedback")]
    [SerializeField] private MMF_Player lobbyShakeFeedback; // online startSliderFeedback
    [SerializeField] private GameObject menuParent;

    private readonly Dictionary<ulong, int> clientProfileIndex = new();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        NetworkedGameManager.OnlineGameStarted += HandleOnlineGameStarted;
    }

    private void OnDisable()
    {
        NetworkedGameManager.OnlineGameStarted -= HandleOnlineGameStarted;
    }

    // Online version of the first lines of local StartGame().
    // Runs on every machine, because gameStarted syncs to everyone.
    private void HandleOnlineGameStarted(bool started)
    {
        if (!started || menuParent == null) return;
        menuParent.SetActive(false); 
        //add MMF feedback
        //MMF_Player menuParentFeedbackPlayer = menuParent.GetComponent<MMF_Player>();
        //if (menuParentFeedbackPlayer != null) menuParentFeedbackPlayer.PlayFeedbacks();
    }

    public void ShakeUI()
    {
        if (lobbyShakeFeedback != null) lobbyShakeFeedback.PlayFeedbacks();
    }

    public void ServerCycleProfile(ulong clientId, int direction)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // Never trust values from a client: only +1 or -1 is valid.
        if (direction != 1 && direction != -1) return;

        if (NetworkedGameManager.Instance != null &&
            NetworkedGameManager.Instance.gameStarted.Value) return;

        if (!clientProfileIndex.TryGetValue(clientId, out int current)) return;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) return;
        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        NetworkedPlayerData playerData = playerObject.GetComponent<NetworkedPlayerData>();
        if (playerData == null || playerData.isReady.Value) return; // ready locks the profile

        int count = playerProfiles.Count;

        // Same search as local CycleProfile: walk in the requested direction
        // until we find a profile nobody else has.
        for (int i = 1; i <= count; i++)
        {
            int next = (current + direction * i + count) % count;

            if (!clientProfileIndex.ContainsValue(next))
            {
                clientProfileIndex[clientId] = next;
                playerData.ServerSetProfileIndex(next);
                return;
            }
        }

        Debug.Log($"[Cycle][Server] request from {clientId}, direction {direction}");
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

        foreach (PlayerLabelUI label in playerLabels)
        {
            label.Setup(gameplayCamera, labelCanvas);
        }

        for (int i = 0; i < scoreCardSlots.Count; i++)
        {
            if (scoreCardSlots[i] != null)
                scoreCardSlots[i].gameObject.SetActive(i < minPlayersToStart);
        }
    }

    public void ReleaseScoreCardSlot(int index, OnlineBasic_PlayerScoreCard owner)
    {
        NetworkedScoreCardUI slot = GetScoreCardSlot(index);
        if (slot == null || !slot.IsBoundTo(owner)) return; // slot may already belong to someone else

        slot.ClearPlayer();
        slot.gameObject.SetActive(index < minPlayersToStart);
    }

    public PlayerLabelUI GetLabel(int index)
    {
        if (index < 0 || index >= playerLabels.Count) return null;
        return playerLabels[index];
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        // On the host this callback fires for every client, including the
        // host's own local client. On a pure client build it only ever fires
        // for itself. Everything below is server-authority logic, so it must
        // not run on non-host clients.
        if (!NetworkManager.Singleton.IsServer) return;

        int profileIndex = GetFirstAvailableProfile();
        clientProfileIndex[clientId] = profileIndex;

        // Same index used for spawn point, score card slot, and profile
        // priority -- all driven by connection order, same as the old system.
        int connectionOrderIndex = clientProfileIndex.Count - 1;

        PositionPlayerAtSpawn(clientId, connectionOrderIndex);
        ApplyProfileToPlayer(clientId, profileIndex);
        ApplySlotIndexToPlayer(clientId, connectionOrderIndex);

        // TODO still remaining:
        // - hook into BasicGameMode.PlayerHasJoined() equivalent once that's converted

        Debug.Log($"Client {clientId} connected, assigned profile {profileIndex}, slot {connectionOrderIndex}");
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        clientProfileIndex.Remove(clientId);

        Debug.Log($"Client {clientId} disconnected");
    }

    private void PositionPlayerAtSpawn(ulong clientId, int spawnIndex)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return; // Player Prefab not assigned yet in NetworkManager Inspector

        if (spawnIndex >= 0 && spawnIndex < spawnPoints.Count)
        {
            playerObject.transform.position = spawnPoints[spawnIndex].position;
        }
    }

    private void ApplySlotIndexToPlayer(ulong clientId, int index)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        NetworkedPlayerData playerData = playerObject.GetComponent<NetworkedPlayerData>();
        if (playerData == null) return;

        playerData.ServerSetSlotIndex(index);
    }

    public NetworkedScoreCardUI GetScoreCardSlot(int index)
    {
        if (index < 0 || index >= scoreCardSlots.Count) return null;
        return scoreCardSlots[index];
    }

    public PlayerProfile GetProfile(int index)
    {
        if (index < 0 || index >= playerProfiles.Count) return null;
        return playerProfiles[index];
    }

    private void ApplyProfileToPlayer(ulong clientId, int profileIndex)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        NetworkedPlayerData playerData = playerObject.GetComponent<NetworkedPlayerData>();
        if (playerData == null)
        {
            Debug.LogWarning("Player prefab is missing NetworkedPlayerData component.");
            return;
        }

        playerData.ServerSetProfileIndex(profileIndex);
        playerData.ServerSetCanMove(false); // players can't move until StartGame() equivalent
    }
    private int GetFirstAvailableProfile()
    {
        for (int i = 0; i < playerProfiles.Count; i++)
        {
            if (!clientProfileIndex.ContainsValue(i)) return i;
        }

        return 0;
    }

    public NetworkedReadyUI GetReadyUISlot(int index)
    {
        if (index < 0 || index >= readyUISlots.Count) return null;
        return readyUISlots[index];
    }

    public void ServerNotifyPlayerReady(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        if (NetworkManager.Singleton.ConnectedClients.Count < minPlayersToStart) return;

        // Everyone must be ready before we start.
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            NetworkObject obj = kvp.Value.PlayerObject;
            if (obj == null) return;

            NetworkedPlayerData data = obj.GetComponent<NetworkedPlayerData>();
            if (data == null || !data.isReady.Value) return;
        }

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            kvp.Value.PlayerObject.GetComponent<NetworkedPlayerData>().ServerSetCanMove(true);
        }

        if (NetworkedGameManager.Instance != null)
            NetworkedGameManager.Instance.ServerStartGame();
    }




}
