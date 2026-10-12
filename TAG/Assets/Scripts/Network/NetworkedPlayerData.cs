using MoreMountains.Feedbacks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

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

    public NetworkVariable<bool> isReady = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("Visual References")]
    [SerializeField] private GameObject itIndicator;

    [SerializeField] private SpriteRenderer playerVisual;
    [SerializeField] private SpriteRenderer directionIndicator;
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private HardLight2D lightObject;

    
    private Sprite defaultSprite;

    private void Awake()
    {
        if (playerVisual != null) defaultSprite = playerVisual.sprite;
    }


    public override void OnNetworkSpawn()
    {
        isIt.OnValueChanged += HandleIsItChanged;
        profileIndex.OnValueChanged += HandleProfileChanged;
        slotIndex.OnValueChanged += HandleSlotIndexChanged;
        isReady.OnValueChanged += HandleReadyChanged;

        HandleIsItChanged(false, isIt.Value);
        HandleProfileChanged(-1, profileIndex.Value);
        HandleSlotIndexChanged(-1, slotIndex.Value);
    }

    public override void OnNetworkDespawn()
    {
        isIt.OnValueChanged -= HandleIsItChanged;
        profileIndex.OnValueChanged -= HandleProfileChanged;
        slotIndex.OnValueChanged -= HandleSlotIndexChanged;
        isReady.OnValueChanged -= HandleReadyChanged;
        if (NetworkedPlayerManager.Instance != null && slotIndex.Value >= 0)
        {
            PlayerLabelUI label = NetworkedPlayerManager.Instance.GetLabel(slotIndex.Value);
            if (label != null) label.ClearPlayer();
        }

        if (NetworkedPlayerManager.Instance != null && slotIndex.Value >= 0)
        {
            NetworkedPlayerManager.Instance.ReleaseScoreCardSlot(
                slotIndex.Value, GetComponent<OnlineBasic_PlayerScoreCard>());
        }
    }

    private void HandleIsItChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Tag][Visual] isIt changed on {gameObject.name}: {previousValue} -> {newValue}. itIndicator assigned: {itIndicator != null}");
        if (itIndicator != null)
        {
            itIndicator.SetActive(newValue);
        }

        if (newValue && slotIndex.Value >= 0 && NetworkedPlayerManager.Instance != null)
        {
            NetworkedScoreCardUI slot = NetworkedPlayerManager.Instance.GetScoreCardSlot(slotIndex.Value);
            if (slot != null) slot.PlayIsItFeedback();
        }
    }



    private void HandleProfileChanged(int previousValue, int newValue)
    {
        ApplyProfileVisuals(newValue);
        TryBindScoreCardSlot();
    }

    private void ApplyProfileVisuals(int index)
    {
        if (NetworkedPlayerManager.Instance == null) return;

        PlayerProfile profile = NetworkedPlayerManager.Instance.GetProfile(index);
        if (profile == null) return; // -1 = not assigned yet

        Color color = profile.playerColor;

        if (lightObject != null) lightObject.Color = color;

        if (playerVisual != null)
        {
            if (profile.isSprite)
            {
                playerVisual.sprite = profile.playerSprite;
                playerVisual.color = Color.white;
            }
            else
            {
                playerVisual.sprite = defaultSprite;
                playerVisual.color = color;
            }
        }

        if (directionIndicator != null)
            directionIndicator.color = Color.Lerp(color, Color.white, 0.6f);

        if (trail != null)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                new GradientColorKey(color, 0f),
                new GradientColorKey(Color.white, 1f)
                },
                new GradientAlphaKey[]
                {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
                });
            trail.colorGradient = gradient;
        }

        // Controller light bar: only the player who owns this object has this
        // machine's controller, so only they should set it.
        if (IsOwner && Gamepad.current is DualShockGamepad dualShock)
            dualShock.SetLightBarColor(color);
    }

    private void HandleReadyChanged(bool previousValue, bool newValue)
    {
        if (newValue && NetworkedPlayerManager.Instance != null)
            NetworkedPlayerManager.Instance.ShakeUI();
    }

    private void HandleSlotIndexChanged(int previousValue, int newValue)
    {
        TryBindScoreCardSlot();

        if (previousValue < 0 && newValue >= 0 && NetworkedPlayerManager.Instance != null)
            NetworkedPlayerManager.Instance.ShakeUI();
    }

    private void TryBindScoreCardSlot()
    {
        if (slotIndex.Value < 0) return;
        if (NetworkedPlayerManager.Instance == null) return;

        PlayerProfile profile = NetworkedPlayerManager.Instance.GetProfile(profileIndex.Value);

        NetworkedScoreCardUI scoreSlot = NetworkedPlayerManager.Instance.GetScoreCardSlot(slotIndex.Value);
        if (scoreSlot != null)
        {
            OnlineBasic_PlayerScoreCard scoreCard = GetComponent<OnlineBasic_PlayerScoreCard>();
            if (scoreCard != null) scoreSlot.BindToPlayer(scoreCard, profile);
        }

        NetworkedReadyUI readySlot = NetworkedPlayerManager.Instance.GetReadyUISlot(slotIndex.Value);
        if (readySlot != null)
        {
            readySlot.BindToPlayer(this, profile);
        }

        PlayerLabelUI label = NetworkedPlayerManager.Instance.GetLabel(slotIndex.Value);
        if (label != null && profile != null)
        {
            label.AssignPlayer(transform, profile.playerColor, profile.playerName);
        }
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

    public void ServerSetReady(bool value)
    {
        if(!IsServer) return;
        isReady.Value = value;
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

    [ServerRpc]
    public void RequestReadyServerRpc()
    {
        if (isReady.Value) return;

        ServerSetReady(true);

        if (NetworkedPlayerManager.Instance != null)
            NetworkedPlayerManager.Instance.ServerNotifyPlayerReady(OwnerClientId);
    }

    [ServerRpc]
    public void RequestCycleProfileServerRpc(int direction)
    {
        if (NetworkedPlayerManager.Instance != null)
            NetworkedPlayerManager.Instance.ServerCycleProfile(OwnerClientId, direction);
    }
}
