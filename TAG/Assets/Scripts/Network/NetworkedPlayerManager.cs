using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class NetworkedPlayerManager : MonoBehaviour
{
    public static NetworkedPlayerManager instance;

    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private List<PlayerProfile> playerProfiles;

    private readonly Dictionary<ulong, int> clientProfileIndex = new();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    private void HandleClientConnected(ulong clientId)
    {
        //may not run on non host clients
        if (!NetworkManager.Singleton.IsServer) return;

        int profileIndex = GetFirstAvailableProfile();
        clientProfileIndex[clientId] = profileIndex;

        PositionPlayerAtSpawn(clientId);
        // TODO once NetworkedPlayerData exists:
        // - tell that client's player object which profile it has
        //   (a NetworkVariable<int> the server sets, or a ClientRpc)
        // - register with score cards / BasicGameMode, same as old RegisterPlayer()

        Debug.Log($"Client {clientId} connected, assigned profile {profileIndex}");
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if(!NetworkManager.Singleton.IsServer) return;

        clientProfileIndex.Remove(clientId);

        Debug.Log($"Client {clientId} disconnected");
    }

    private void PositionPlayerAtSpawn(ulong clientId)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject != null) return;

        int spawnIndex = clientProfileIndex.Count - 1;
        if(spawnIndex>= 0 && spawnIndex < spawnPoints.Count)
        {
            playerObject.transform.position = spawnPoints[spawnIndex].position;
        }
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
