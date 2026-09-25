using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;


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
    }

    public override void OnNetworkSpawn()
    {
        
        if (IsOwner)
        {
            moveAction.action.Enable();
            readyAction.action.Enable();
            readyAction.action.performed += OnReadyPerformed;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            moveAction.action.Disable();
            readyAction.action.Disable();
            readyAction.action.performed -= OnReadyPerformed;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        moveInput = moveAction.action.ReadValue<Vector2>();
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
}
