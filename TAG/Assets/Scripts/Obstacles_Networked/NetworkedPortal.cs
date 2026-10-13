using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Unity.Netcode;
using UnityEngine;

public class NetworkedPortal : NetworkBehaviour
{
    public List<NetworkedPortal> linkedPortals;
    public float teleportOffset;
    public float teleportDuration = 0.5f;
    public bool lineRenderersOn = false;
    [SerializeField] private MMF_Player portalFeedback;

    [Header("Exit Override")]
    [SerializeField] private Transform overrideExitPoint;

    [Header("Connection Visuals")]
    [SerializeField] private bool isSpecial = false;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private LineRenderer specialLinePrefab;
    [SerializeField] private Color lineRendererColor;
    private readonly List<LineRenderer> spawnedLines = new();

    // Cosmetic only, so every machine just draws its own lines from scene data.
    private void Start()
    {
        if (lineRenderersOn) DrawPortalLines();
    }

    private void DrawPortalLines()
    {
        foreach (NetworkedPortal linked in linkedPortals)
        {
            if (linked == null) continue;

            // Draw one line per linked pair, not two.
            if (linked.linkedPortals.Contains(this) &&
                linked.GetInstanceID() < GetInstanceID())
            {
                continue;
            }

            LineRenderer prefab = isSpecial ? specialLinePrefab : linePrefab;
            LineRenderer line = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.SetPosition(0, transform.position);
            line.SetPosition(1, linked.transform.position);
            spawnedLines.Add(line);
        }
    }

    // Runs on every machine for every player, so only the owner acts on it.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkedPlayerMovement movement = other.GetComponentInParent<NetworkedPlayerMovement>();
        if (movement == null || !movement.IsOwner) return;
        if (movement.IsTeleporting) return;

        RequestTeleportServerRpc();

        // TODO (networked obstacles step): the local Portal also moves
        // "Obstacle"-tagged objects such as cannonballs through the portal.
    }

    // RequireOwnership = false because the portal is owned by the server,
    // but any client may poke it (same pattern as NetworkedStar).
    [ServerRpc(RequireOwnership = false)]
    private void RequestTeleportServerRpc(ServerRpcParams rpcParams = default)
    {
        if (linkedPortals == null || linkedPortals.Count == 0) return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) return;

        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null) return;

        NetworkedPlayerData playerData = playerObject.GetComponent<NetworkedPlayerData>();
        if (playerData == null || playerData.isTeleporting.Value) return; // also stops double triggers

        int targetIndex = Random.Range(0, linkedPortals.Count);

        playerData.ServerSetTeleporting(true, lineRendererColor, teleportDuration);
        TeleportClientRpc(clientId, targetIndex);
    }

    [ClientRpc]
    private void TeleportClientRpc(ulong teleportingClientId, int targetIndex)
    {
        // Everyone sees and hears the portal.
        if (portalFeedback != null) portalFeedback.PlayFeedbacks();

        // Only the teleporting player's own machine moves them.
        if (NetworkManager.Singleton.LocalClientId != teleportingClientId) return;

        NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObject == null) return;

        NetworkedPlayerMovement movement = playerObject.GetComponent<NetworkedPlayerMovement>();
        NetworkedPlayerData playerData = playerObject.GetComponent<NetworkedPlayerData>();

        if (targetIndex < 0 || targetIndex >= linkedPortals.Count || linkedPortals[targetIndex] == null)
        {
            playerData.TeleportFinishedServerRpc(); // never leave the player stuck mid-beam
            return;
        }

        NetworkedPortal target = linkedPortals[targetIndex];
        Vector2 exitPosition = GetExitPosition(target, playerObject.GetComponent<Rigidbody2D>());

        movement.TeleportAlongLine(transform.position, target.transform.position,
                                   exitPosition, teleportDuration);
    }

    // Same rules as the local Portal.
    private Vector2 GetExitPosition(NetworkedPortal target, Rigidbody2D rb)
    {
        if (target.overrideExitPoint != null)
            return target.overrideExitPoint.position;

        Vector2 direction = rb != null ? rb.linearVelocity.normalized : Vector2.zero;
        return (Vector2)target.transform.position + direction * teleportOffset;
    }
}