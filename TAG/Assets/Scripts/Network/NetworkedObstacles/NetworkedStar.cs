using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NetworkedStar : NetworkBehaviour
{
    public NetworkVariable<bool> isAvailable = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        isAvailable.OnValueChanged += HandleAvailabilityChanged;
        HandleAvailabilityChanged(true, isAvailable.Value); // existing line; now also adds it to the camera
    }

    public override void OnNetworkDespawn()
    {
        isAvailable.OnValueChanged -= HandleAvailabilityChanged;
        UpdateCameraMembership(false);
    }

    private void HandleAvailabilityChanged(bool previousValue, bool newValue)
    {
        gameObject.SetActive(newValue);
        UpdateCameraMembership(newValue);
    }

    private void UpdateCameraMembership(bool inGroup)
    {
        if (NetworkedPlayerManager.Instance == null) return;

        if (inGroup) NetworkedPlayerManager.Instance.AddToTargetGroup(transform);
        else NetworkedPlayerManager.Instance.RemoveFromTargetGroup(transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkedPlayerMovement playerMovement = other.GetComponent<NetworkedPlayerMovement>();
        if (playerMovement == null || !playerMovement.IsOwner) return;

        if (!isAvailable.Value) return;

        RequestPickupServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        // Server re-checks from scratch. This is also what resolves the
        // "two players reach it the same frame" race -- whichever RPC the
        // server processes first wins, and the second one fails this check.
        if (!isAvailable.Value) return;

        ulong requestingClientId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(requestingClientId, out var client))
            return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        NetworkedPlayerData playerData = playerObject.GetComponent<NetworkedPlayerData>();
        if (playerData == null) return;

        if (playerData.isTeleporting.Value) return;

        isAvailable.Value = false;
        playerData.ServerSetIsIt(true);

        playerData.ServerBroadcastTagEffects(requestingClientId, NetworkedPlayerData.NoClient);
    }

    // TODO: once BasicGameMode is converted, call this from the server-side
    // equivalent of ResetLevelForNextRound() to bring the star back.
    public void ServerResetStar()
    {
        if (!IsServer) return;
        isAvailable.Value = true;
    }


}
