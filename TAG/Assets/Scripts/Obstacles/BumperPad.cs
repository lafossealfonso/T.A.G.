using MoreMountains.Feedbacks;
using UnityEngine;

public class BumperPad : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("Fixed speed added on top of incoming speed. This is what actually determines the 'kick' strength.")]
    [SerializeField] private float bumpKickAmount = 6f;

    [Tooltip("Floor for exit speed, in case something taps the bumper slowly.")]
    [SerializeField] private float minimumExitSpeed = 8f;

    [Tooltip("Hard ceiling -- exit speed can never exceed this, no matter how many bumpers chain together.")]
    [SerializeField] private float maximumExitSpeed = 25f;

    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float debugLineLength = 0.2f;
    [SerializeField] private MMF_Player bumperPadFeedback;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        // Fixed, not read from the collision -- this bumper always faces
        // one direction since it's mounted flat against a wall. This avoids
        // the inconsistent per-contact normals you get from box colliders
        // (corners especially can report a different contact than the flat face).
        Vector2 normal = transform.up;

        Vector2 incomingVelocity = collision.relativeVelocity;
        if (incomingVelocity.sqrMagnitude < 0.01f) return;

        if (Vector2.Dot(incomingVelocity, normal) > 0f)
            incomingVelocity = -incomingVelocity;

        Vector2 reflectedDirection = Vector2.Reflect(incomingVelocity.normalized, normal);

        // Additive rather than multiplicative -- each bumper adds a fixed kick
        // instead of scaling up whatever speed the player already had, so
        // chaining through multiple bumpers grows linearly instead of exponentially.
        float rawExitSpeed = incomingVelocity.magnitude + bumpKickAmount;

        // Clamp handles both ends: never weaker than minimumExitSpeed,
        // never stronger than maximumExitSpeed.
        float exitSpeed = Mathf.Clamp(rawExitSpeed, minimumExitSpeed, maximumExitSpeed);

        Vector2 impulseForce = reflectedDirection * exitSpeed * rb.mass;
        PlayerMovement playerMovement = collision.gameObject.GetComponentInParent<PlayerMovement>();

        if (playerMovement != null)
        {
            playerMovement.Bumped(impulseForce, knockbackDuration);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(impulseForce, ForceMode2D.Impulse);
        }

        bumperPadFeedback.PlayFeedbacks();
    }

    // Draws an arrow in the Scene view so you can see exactly which way
    // the bumper thinks it's facing while you're placing/rotating it.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(Vector2)transform.up * debugLineLength);
    }
}