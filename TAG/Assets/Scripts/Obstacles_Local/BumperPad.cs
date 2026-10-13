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

        bool isPlayer = collision.gameObject.CompareTag("Player");
        bool isObstacle = collision.gameObject.CompareTag("Obstacle");

        // Only react to things we explicitly want the bumper to affect --
        // anything else (world geometry, decorations with a stray Rigidbody2D)
        // gets ignored entirely.
        if (!isPlayer && !isObstacle) return;

        Vector2 normal = transform.up;

        Vector2 incomingVelocity = collision.relativeVelocity;
        if (incomingVelocity.sqrMagnitude < 0.01f) return;

        if (Vector2.Dot(incomingVelocity, normal) > 0f)
            incomingVelocity = -incomingVelocity;

        Vector2 reflectedDirection = Vector2.Reflect(incomingVelocity.normalized, normal);

        float rawExitSpeed = incomingVelocity.magnitude + bumpKickAmount;
        float exitSpeed = Mathf.Clamp(rawExitSpeed, minimumExitSpeed, maximumExitSpeed);

        Vector2 impulseForce = reflectedDirection * exitSpeed * rb.mass;

        if (isPlayer)
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponentInParent<PlayerMovement>();

            if (playerMovement != null)
            {
                playerMovement.Bumped(impulseForce, knockbackDuration);
                return;
            }
        }

        // Obstacle-tagged objects (or a Player without a PlayerMovement
        // component for some reason) just get a plain physics impulse --
        // no input-lockout coroutine needed since they don't read input anyway.
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(impulseForce, ForceMode2D.Impulse);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)(Vector2)transform.up * debugLineLength);
    }
}