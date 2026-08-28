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
        // Only enable input reading on the machine that owns this object.
        // Every other client just sees this object's transform sync in via
        // NetworkTransform -- they never read input for it.
        if (IsOwner)
        {
            moveAction.action.Enable();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            moveAction.action.Disable();
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

    // --- Tagging ---------------------------------------------------------
    //Client detects locally first

    [Header("Tagging")]
    [SerializeField] private float localTagRequestCooldown = 0.5f;
    private float nextTagRequestTime;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision Detected");
        if (!IsOwner) return;
        Debug.Log("Is Owner");
        if (Time.time < nextTagRequestTime) return;
        Debug.Log("Next Tag not available");
        if (!collision.gameObject.CompareTag("Player")) return;
        Debug.Log("IsPLayerCollision");

        NetworkObject otherNetworkObject = collision.gameObject.GetComponent<NetworkObject>();
        NetworkedPlayerData otherPlayerData = collision.gameObject.GetComponent<NetworkedPlayerData>();
        if (otherNetworkObject == null || otherPlayerData == null) return;

        if (!otherPlayerData.isIt.Value || playerData.isIt.Value) return;

        nextTagRequestTime = Time.time + localTagRequestCooldown;

        playerData.RequestTagServerRpc(otherNetworkObject.OwnerClientId);
    }
}
