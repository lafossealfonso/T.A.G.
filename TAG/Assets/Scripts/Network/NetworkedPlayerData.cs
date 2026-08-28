using UnityEngine;
using Unity.Netcode;

public class NetworkedPlayerData : NetworkBehaviour
{
    // Write permission = Server means only server-side code can set .Value.
    // Clients can still freely READ .Value, they just can't assign to it --
    // this is what stops a client from setting their own isIt = true.
    public NetworkVariable<bool> isIt = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> canMove = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> profileIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Visual References (mirrors old PlayerMovement fields)")]
    [SerializeField] private GameObject itIndicator;

    public override void OnNetworkSpawn()
    {
        isIt.OnValueChanged += HandleIsItChanged;
        profileIndex.OnValueChanged += HandleProfileChanged;

        HandleIsItChanged(false, isIt.Value);
        HandleProfileChanged(-1, profileIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        isIt.OnValueChanged -= HandleIsItChanged;
        profileIndex.OnValueChanged -= HandleProfileChanged;
    }

    private void HandleIsItChanged(bool previousValue, bool newValue)
    {
        if(itIndicator != null)
        {
            itIndicator.SetActive(newValue);
        }
    }

    private void HandleProfileChanged(int previousValue, int newValue)
    {
        // TODO: once PlayerProfile assignment is ported over, apply
        // color/sprite here -- same job old PlayerManager.ApplyProfile() did.
    }

    // --- Server-only setters -------------------------------------------

    public void ServerSetIsIt(bool value)
    {
        if (!IsServer) return;
        isIt.Value = value;
    }

    public void ServerSetCanMove(bool value)
    {
        if (!IsServer) return;
        canMove.Value = value;
    }

    public void ServerSetProfileIndex(int index)
    {
        if (!IsServer) return;
        profileIndex.Value = index;
    }

    // --- Tagging -----------------------------------------------------------

    [ServerRpc]
    public void RequestTagServerRpc(ulong taggedClientId)
    {
        if (!ServerCanTag()) return;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(taggedClientId, out var taggedClient)) return;

        NetworkObject taggedObject = taggedClient.PlayerObject;
        if (taggedObject == null) return;

        NetworkedPlayerData taggedPlayerData = taggedObject.GetComponent<NetworkedPlayerData>();
        if (taggedPlayerData == null || !taggedPlayerData.ServerCanTag()) return;

        // Mirrors the old PlayerMovement.OnCollisionEnter2D check: only a
        // valid tag if the OTHER player currently is It, and I currently am not.
        if (!taggedPlayerData.isIt.Value || isIt.Value) return;

        taggedPlayerData.ServerSetIsIt(false);
        ServerSetIsIt(true);

        ServerStartTagCooldown();
        taggedPlayerData.ServerStartTagCooldown();

        // TODO: once BasicGameMode is converted, broadcast the tagged-feedback
        // (VFX/SFX) here -- likely a ClientRpc, mirroring old GameManager.PlayerTagged.
    }

    private const float TagCooldownDuration = 0.5f;
    private float tagCooldownEndTime;

    public bool ServerCanTag()
    {
        return Time.time >= tagCooldownEndTime;
    }

    public void ServerStartTagCooldown()
    {
        tagCooldownEndTime = Time.time + TagCooldownDuration;
    }
}
