using UnityEngine;

public class Vacuum2D : MonoBehaviour
{
    [SerializeField] private float vacuumForce = 20f;

    private void OnTriggerStay2D(Collider2D other)
    {
        Rigidbody2D rb = other.attachedRigidbody;

        if (rb == null)
            return;

        Vector2 direction = (Vector2)transform.position - rb.position;

        rb.AddForce(direction.normalized * vacuumForce, ForceMode2D.Force);
    }
}
