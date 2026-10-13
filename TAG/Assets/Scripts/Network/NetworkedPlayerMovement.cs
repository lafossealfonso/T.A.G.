using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(NetworkedPlayerData))]
public class NetworkedPlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Input")]
    [Tooltip("Drag the 'Move' action from Move.inputactions here.")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Ready Input")]
    [Tooltip("Drag the 'StartGame' or 'Hold' action from Move.inputactions here.")]
    [SerializeField] private InputActionReference readyAction;

    private Rigidbody2D rb;
    private NetworkedPlayerData playerData;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerData = GetComponent<NetworkedPlayerData>();
        NetworkedReadyUI readyUI = NetworkedPlayerManager.Instance.GetReadyUISlot(playerData.slotIndex.Value);
        if (readyUI != null) readyUI.PlayJoinFeedback(playerData.slotIndex.Value);
    }

    public override void OnNetworkSpawn()
    {
        
        if (IsOwner)
        {
            moveAction.action.Enable();
            readyAction.action.Enable();
            readyAction.action.performed += OnReadyPerformed;
            moveAction.action.performed += OnMovePerformed;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            moveAction.action.Disable();
            readyAction.action.Disable();
            readyAction.action.performed -= OnReadyPerformed;
            moveAction.action.performed -= OnMovePerformed;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (isRidingBeam)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        moveInput = moveAction.action.ReadValue<Vector2>();
        HandleProfileCycleInput();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!playerData.canMove.Value)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Move();
        RotateDirection();
    }

    private void Move()
    {
        float drag = playerData.isIt.Value ? 0.88f : 0.9f;

        rb.linearVelocity = new Vector2(
            moveInput.x * moveSpeed,
            moveInput.y * moveSpeed
        ) + rb.linearVelocity * drag;
    }

    private void RotateDirection()
    {
        if (moveInput == Vector2.zero) return;

        float angle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(
            transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void OnReadyPerformed(InputAction.CallbackContext context)
    {
        playerData.RequestReadyServerRpc();
    }

    // --- Portal teleport ---------------------------------------------------
    [Header("Portal Teleport")]
    [SerializeField] private float exitSnapDuration = 0.1f;
    private bool isRidingBeam;

    public bool IsTeleporting
    {
        get { return isRidingBeam || playerData.isTeleporting.Value; }
    }

    public void TeleportAlongLine(Vector2 lineStart, Vector2 lineEnd, Vector2 exitPosition, float duration)
    {
        if (!IsOwner || isRidingBeam) return;
        StartCoroutine(TeleportAlongLineRoutine(lineStart, lineEnd, exitPosition, duration));
    }

    private IEnumerator TeleportAlongLineRoutine(Vector2 lineStart, Vector2 lineEnd,
                                                 Vector2 exitPosition, float duration)
    {
        isRidingBeam = true;
        rb.linearVelocity = Vector2.zero;

        rb.MovePosition(lineStart);                                 // snap onto the portal
        yield return LerpAlong(lineStart, lineEnd, duration);       // ride the beam
        yield return LerpAlong(lineEnd, exitPosition, exitSnapDuration); // settle at the exit

        isRidingBeam = false;
        playerData.TeleportFinishedServerRpc();
    }

    private IEnumerator LerpAlong(Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rb.MovePosition(Vector2.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        rb.MovePosition(to);
    }

    // --- Tagging ---------------------------------------------------------
    //Client detects locally first

    [Header("Tagging")]
    [SerializeField] private float localTagRequestCooldown = 0.5f;
    private float nextTagRequestTime;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsOwner) return;
        if (Time.time < nextTagRequestTime) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        NetworkObject otherNetworkObject = collision.gameObject.GetComponentInParent<NetworkObject>();
        if (otherNetworkObject == null) return;

        nextTagRequestTime = Time.time + localTagRequestCooldown;
        playerData.RequestTagServerRpc(otherNetworkObject.OwnerClientId);
    }

    // --- Lobby input -----------------------------------------------------
    private float nextProfileCycleTime;
    private const float ProfileCycleCooldown = 0.25f; // same as local

    private bool IsInLobby()
    {
        return NetworkedGameManager.Instance != null &&
               !NetworkedGameManager.Instance.gameStarted.Value;
    }

    private void HandleProfileCycleInput()
    {
        if (moveInput != Vector2.zero)
            Debug.Log($"[Cycle] input={moveInput} inLobby={IsInLobby()} " +
                      $"ready={playerData.isReady.Value} " +
                      $"managerNull={NetworkedGameManager.Instance == null}");

        if (!IsInLobby() || playerData.isReady.Value) return;
        if (Time.time < nextProfileCycleTime) return;

        if (moveInput.x > 0.5f)
        {
            Debug.Log("[Cycle] sending +1");
            playerData.RequestCycleProfileServerRpc(1);
            nextProfileCycleTime = Time.time + ProfileCycleCooldown;
        }
        else if (moveInput.x < -0.5f)
        {
            Debug.Log("[Cycle] sending -1");
            playerData.RequestCycleProfileServerRpc(-1);
            nextProfileCycleTime = Time.time + ProfileCycleCooldown;
        }
    }

    // Mirrors PlayerMovement.OnMove: moving while frozen plays the card feedback.
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (playerData.canMove.Value || !IsInLobby()) return;
        if (NetworkedPlayerManager.Instance == null) return;

        NetworkedReadyUI readyUI = NetworkedPlayerManager.Instance.GetReadyUISlot(playerData.slotIndex.Value);
        if (readyUI != null) readyUI.PlayNudgeFeedback(playerData.slotIndex.Value);
    }
}
