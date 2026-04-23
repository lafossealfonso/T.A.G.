using UnityEngine;

public class BoostPad : MonoBehaviour
{
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

                rb.AddForce(moveDirection * 35f, ForceMode2D.Impulse);
            }
        }

        
    }
}
