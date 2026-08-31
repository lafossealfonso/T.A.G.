using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class NetworkedPlayerManager : MonoBehaviour
{
    public static NetworkedPlayerManager Instance;

    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerProfile> playerProfiles;

    [SerializeField] private List<NetworkedScoreCardUI> scoreCardSlots;

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

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
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
}
