using MoreMountains.Feedbacks;
using Unity.Netcode;
using Unity.Netcode.Components;
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

    public NetworkVariable<Color> teleportColor = new NetworkVariable<Color>(
    Color.white,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isTeleporting = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private SpriteRenderer teleportingVisual;
    [SerializeField] private Collider2D playerCollider; // falls back to GetComponent in Awake

    private float teleportDeadline;


    private Sprite defaultSprite;

    private void Awake()
    {
        if (playerVisual != null) defaultSprite = playerVisual.sprite;
        if (playerCollider == null) playerCollider = GetComponent<Collider2D>();
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

        RegisterWithScene();

        isTeleporting.OnValueChanged += HandleTeleportingChanged;
        if (isTeleporting.Value) HandleTeleportingChanged(false, true); // joined mid-teleport
        
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

        if (NetworkedPlayerManager.Instance != null)
            NetworkedPlayerManager.Instance.RemoveFromTargetGroup(transform);

        isTeleporting.OnValueChanged -= HandleTeleportingChanged;
        if (NetworkedPlayerManager.Instance != null)
            NetworkedPlayerManager.Instance.PlayerDespawned(this);

    }

    private void HandleIsItChanged(bool previousValue, bool newValue)
    {
        Debug.Log($"[Tag][Visual] isIt changed on {gameObject.name}: {previousValue} -> {newValue}. itIndicator assigned: {itIndicator != null}");

        if (itIndicator != null) itIndicator.SetActive(newValue && !isTeleporting.Value);

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

    private void HandleTeleportingChanged(bool previous, bool current)
    {
        SetTeleportVisual(current, teleportColor.Value);
    }

    // Online version of PlayerMovement.SetTeleportVisual, run on every machine.
    private void SetTeleportVisual(bool teleporting, Color color)
    {
        if (playerVisual != null) playerVisual.enabled = !teleporting;
        if (directionIndicator != null) directionIndicator.enabled = !teleporting;
        if (playerCollider != null) playerCollider.enabled = !teleporting;
        if (lightObject != null) lightObject.gameObject.SetActive(teleporting);

        if (teleportingVisual != null)
        {
            teleportingVisual.color = color;
            teleportingVisual.enabled = teleporting;
        }

        if (itIndicator != null) itIndicator.SetActive(!teleporting && isIt.Value);
    }

    public void ServerSetTeleporting(bool value, Color color, float expectedDuration)
    {
        if (!IsServer) return;
        teleportColor.Value = color;
        isTeleporting.Value = value;
        teleportDeadline = Time.time + expectedDuration + 1.5f;
    }

    [ServerRpc]
    public void TeleportFinishedServerRpc()
    {
        if (isTeleporting.Value) isTeleporting.Value = false;
    }

    // Safety net: if the owner's "finished" message is ever lost, don't leave
    // a player invisible and untouchable forever.
    private void Update()
    {
        if (IsServer && isTeleporting.Value && Time.time > teleportDeadline)
            isTeleporting.Value = false;
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

        if (isTeleporting.Value || taggedPlayerData.isTeleporting.Value)
        {
            Debug.Log("[Tag][Server] REJECTED: someone is mid-teleport.");
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
        ServerBroadcastTagEffects(OwnerClientId, taggedClientId);
        taggedPlayerData.ServerRespawnOwner();
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

    // --- Tag effects -----------------------------------------------------

    public const ulong NoClient = ulong.MaxValue; // "nobody", used for star pickups

    // Server calls this; the RPC then runs on all clients.
    public void ServerBroadcastTagEffects(ulong taggerClientId, ulong taggedClientId)
    {
        if (!IsServer) return;
        TagEffectsClientRpc(taggerClientId, taggedClientId);
    }

    [ClientRpc]
    private void TagEffectsClientRpc(ulong taggerClientId, ulong taggedClientId)
    {
        if (NetworkedGameMode.Instance != null)
            NetworkedGameMode.Instance.PlayTagEffects();

        // Haptics are about THIS machine's controller, so each client checks
        // whether it is the tagger or the tagged player.
        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (localId == taggerClientId) PlayLocalHaptic(HapticType.BecameIt);
        else if (localId == taggedClientId) PlayLocalHaptic(HapticType.GotTagged);
    }

    private void PlayLocalHaptic(HapticType type)
    {
        if (HapticsManager.Instance == null) return;
        HapticsManager.Instance.Play(Gamepad.current, type); // null (keyboard) is ignored by Play
    }

    // Called on the TAGGED player's object. Sends only to that player's owner.
    public void ServerRespawnOwner()
    {
        if (!IsServer) return;

        Vector3 target = NetworkedGameMode.Instance != null
            ? NetworkedGameMode.Instance.RespawnPosition
            : transform.position;

        ServerTeleportOwnerTo(target);
    }

    public void ServerTeleportOwnerTo(Vector3 position)
    {
        if (!IsServer) return;

        ClientRpcParams ownerOnly = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        };

        TeleportToClientRpc(position, ownerOnly);
    }

    [ClientRpc]
    private void TeleportToClientRpc(Vector3 target, ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        NetworkTransform netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
            netTransform.Teleport(target, transform.rotation, transform.localScale);
        else
            transform.position = target;

        if (rb != null) rb.position = target;
    }

    [ClientRpc]
    private void RespawnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return; // only the owner may move this player

        Vector3 target = NetworkedGameMode.Instance != null
            ? NetworkedGameMode.Instance.RespawnPosition
            : transform.position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Teleport tells NetworkTransform "this is a jump, don't smooth it".
        NetworkTransform netTransform = GetComponent<NetworkTransform>();
        if (netTransform != null)
            netTransform.Teleport(target, transform.rotation, transform.localScale);
        else
            transform.position = target;

        if (rb != null) rb.position = target;
    }

    public NetworkVariable<int> roundWins = new NetworkVariable<int>(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    public void ServerAddRoundWin()
    {
        if (!IsServer) return;
        roundWins.Value += 1;
    }

    public void ServerResetForNewMatch()
    {
        if (!IsServer) return;
        isReady.Value = false;
        canMove.Value = false;
        isIt.Value = false;
        roundWins.Value = 0;
        isTeleporting.Value = false;
    }

    // Everything that connects this player to the CURRENT scene's objects. Runs on spawn,
    // and again after a reload, because players persist but scene objects don't.
    public void RegisterWithScene()
    {
        if (NetworkedPlayerManager.Instance == null) return;

        NetworkedPlayerManager.Instance.PlayerSpawned(this);
        NetworkedPlayerManager.Instance.AddToTargetGroup(transform);
        TryBindScoreCardSlot();
    }
}
