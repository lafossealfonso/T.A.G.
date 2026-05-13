using MoreMountains.Feedbacks;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    public List<Transform> linkedPortals;
    public float teleportOffset;
    [SerializeField] MMF_Player portalFeedback;

    private void OnTriggerEnter2D(Collider2D other)
    {
        portalFeedback.PlayFeedbacks();
        int randomIndex = Random.Range(0, linkedPortals.Count);
        Transform targetTransform = linkedPortals[randomIndex];

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        Vector2 direction = rb.linearVelocity.normalized;

        Vector2 offset = direction * teleportOffset;

        other.transform.position = (Vector2)targetTransform.position + offset;
    }
}
