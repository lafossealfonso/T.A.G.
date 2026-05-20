using MoreMountains.Feedbacks;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float forceAmount;
    [SerializeField] MMF_Player boostFeedback;
    private void OnTriggerEnter2D(Collider2D other)
    {
        boostFeedback.PlayFeedbacks();
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
