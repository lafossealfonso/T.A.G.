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

    // Which pre-placed UI score card slot this player has been assigned,
    // in connection order -- the networked replacement for old
    // PlayerManager's scoreCards[index] assignment. -1 means "not assigned yet".
    public NetworkVariable<int> slotIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Visual References (mirrors old PlayerMovement fields)")]
    [SerializeField] private GameObject itIndicator;

    public override void OnNetworkSpawn()
    {
        isIt.OnValueChanged += HandleIsItChanged;
        profileIndex.OnValueChanged += HandleProfileChanged;
        slotIndex.OnValueChanged += HandleSlotIndexChanged;

        HandleIsItChanged(false, isIt.Value);
        HandleProfileChanged(-1, profileIndex.Value);
        HandleSlotIndexChanged(-1, slotIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        isIt.OnValueChanged -= HandleIsItChanged;
        profileIndex.OnValueChanged -= HandleProfileChanged;
    }

    private void HandleIsItChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Tag][Visual] isIt changed on {gameObject.name}: {previousValue} -> {newValue}. itIndicator assigned: {itIndicator != null}");
        if (itIndicator != null)
        {
            itIndicator.SetActive(newValue);
        }
    }

    private void HandleProfileChanged(int previousValue, int newValue)
    {
        // TODO: once PlayerProfile assignment is ported over, apply
        // color/sprite here -- same job old PlayerManager.ApplyProfile() did.
    }

    private void HandleSlotIndexChanged(int previousValue, int newValue)
    {
        TryBindScoreCardSlot();
    }

    private void TryBindScoreCardSlot()
    {
        if (slotIndex.Value < 0) return;
        if (NetworkedPlayerManager.Instance == null) return;

        NetworkedScoreCardUI slot = NetworkedPlayerManager.Instance.GetScoreCardSlot(slotIndex.Value);
        if (slot == null) return;

        OnlineBasic_PlayerScoreCard scoreCard = GetComponent<OnlineBasic_PlayerScoreCard>();
        if (scoreCard == null) return;

        PlayerProfile profile = NetworkedPlayerManager.Instance.GetProfile(profileIndex.Value);

        slot.BindToPlayer(scoreCard, profile);
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

    public void ServerSetSlotIndex(int index)
    {
        if (!IsServer) return;
        slotIndex.Value = index;
    }

    // --- Tagging -----------------------------------------------------------

    [ServerRpc]
    public void RequestTagServerRpc(ulong taggedClientId)
    {

        Debug.Log($"[Tag][Server] Received request. Tagger OwnerClientId={OwnerClientId}, targeting clientId={taggedClientId}, tagger isIt={isIt.Value}");

        if (!ServerCanTag())
        {
            Debug.Log("[Tag][Server] REJECTED: tagger is still on cooldown.");
            return;
        }

        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(taggedClientId, out var taggedClient))
        {
            Debug.Log($"[Tag][Server] REJECTED: clientId {taggedClientId} not found in ConnectedClients.");
            return;
        }

        NetworkObject taggedObject = taggedClient.PlayerObject;
        if (taggedObject == null)
        {
            Debug.Log("[Tag][Server] REJECTED: taggedClient.PlayerObject is null.");
            return;
        }

        NetworkedPlayerData taggedPlayerData = taggedObject.GetComponent<NetworkedPlayerData>();
        if (taggedPlayerData == null)
        {
            Debug.Log("[Tag][Server] REJECTED: target has no NetworkedPlayerData component.");
            return;
        }

        if (!taggedPlayerData.ServerCanTag())
        {
            Debug.Log("[Tag][Server] REJECTED: target is still on cooldown.");
            return;
        }

        Debug.Log($"[Tag][Server] Validation check: target isIt={taggedPlayerData.isIt.Value}, tagger isIt={isIt.Value}");

        if (!taggedPlayerData.isIt.Value || isIt.Value)
        {
            Debug.Log("[Tag][Server] REJECTED: target is not It, or tagger already is It.");
            return;
        }

        taggedPlayerData.ServerSetIsIt(false);
        ServerSetIsIt(true);

        ServerStartTagCooldown();
        taggedPlayerData.ServerStartTagCooldown();

        Debug.Log($"[Tag][Server] SUCCESS: tag applied. Tagger isIt is now {isIt.Value}, target isIt is now {taggedPlayerData.isIt.Value}");
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
