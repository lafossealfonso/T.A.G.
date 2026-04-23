using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SlowTrap : MonoBehaviour
{
    private float originalSpeed;
    public float slowedSpeed = 2f; // Speed to set when player enters the trigger zone

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterController>() != null)
        {
            originalSpeed = collision.gameObject.GetComponent<CharacterController>().moveSpeed; // Store the player's original speed
            collision.gameObject.GetComponent<CharacterController>().moveSpeed = slowedSpeed; // Slow down the player's speed
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<CharacterController>() != null)
        {
            collision.gameObject.GetComponent<CharacterController>().moveSpeed = originalSpeed;  // Restore the player's original speed
        }
    }
}
