using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float forceAmount;
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            playerMovement.BoostPad();
            return;
        }

        else
        {
            Rigidbody2D rb = other.attachedRigidbody;

            if(rb != null)
            {

                Vector2 moveDirection = rb.linearVelocity.normalized;

                rb.AddForce(moveDirection * forceAmount, ForceMode2D.Impulse);
            }
        }

        
    }
}
