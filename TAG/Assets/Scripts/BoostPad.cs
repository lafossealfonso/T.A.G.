using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float boostForce = 10f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (collision.gameObject.GetComponent<CharacterController>() != null)
            {
                StartCoroutine(collision.GetComponent<CharacterController>().BoostDelay());
            }
            Vector2 boostDirection = rb.velocity.normalized;
            rb.AddForce(boostDirection * boostForce, ForceMode2D.Impulse);
        }
    }
}




