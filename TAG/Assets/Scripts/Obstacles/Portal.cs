using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public List<Transform> linkedPortals;
    public float teleportOffset;
    public float teleportDuration = 0.5f;
    [SerializeField] MMF_Player portalFeedback;

    [Header("Exit Override")]
    [Tooltip("If set, players arriving at THIS portal are placed here instead of using the calculated offset.")]
    [SerializeField] private Transform overrideExitPoint;

    [Header("Connection Visuals")]
    [SerializeField] private bool isSpecial = false;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private LineRenderer specialLinePrefab;
    [SerializeField] private Color lineRendererColor;
    private readonly List<LineRenderer> spawnedLines = new();

    private void Start()
    {
        DrawPortalLines();
    }

    private void DrawPortalLines()
    {
        foreach (Transform linkedPortal in linkedPortals)
        {
            Portal linkedPortalScript = linkedPortal.GetComponent<Portal>();

            if (linkedPortalScript != null &&
                linkedPortalScript.linkedPortals.Contains(transform) &&
                linkedPortalScript.GetInstanceID() < GetInstanceID())
            {
                continue;
            }

            if (!isSpecial)
            {
                LineRenderer line = Instantiate(linePrefab, transform.position, Quaternion.identity, transform);
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, linkedPortal.position);

                spawnedLines.Add(line);
            }

            else
            {
                LineRenderer line = Instantiate(specialLinePrefab, transform.position, Quaternion.identity, transform);
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.SetPosition(0, transform.position);
                line.SetPosition(1, linkedPortal.position);

                spawnedLines.Add(line);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
            return;

        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
        if (playerMovement == null)
            return;

        portalFeedback.PlayFeedbacks();

        int randomIndex = Random.Range(0, linkedPortals.Count);
        Transform targetTransform = linkedPortals[randomIndex];

        Vector2 lineStart = transform.position;
        Vector2 lineEnd = targetTransform.position;

        Vector2 exitPosition;

        Portal targetPortalScript = targetTransform.GetComponent<Portal>();

        if (targetPortalScript != null && targetPortalScript.overrideExitPoint != null)
        {
            exitPosition = targetPortalScript.overrideExitPoint.position;
        }
        else
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            Vector2 direction = rb.linearVelocity.normalized;
            Vector2 offset = direction * teleportOffset;
            exitPosition = lineEnd + offset;
        }

        playerMovement.TeleportAlongLine(lineStart, lineEnd, exitPosition, teleportDuration, lineRendererColor);
    }
}