using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // The prefab to instantiate
    public GameObject prefab;

    // The minimum and maximum force to apply to each object
    public float minForce = 5f;
    public float maxForce = 15f;

    // The number of objects to instantiate
    public int objectCount = 10;

    void Start()
    {
        for (int i = 0; i < objectCount; i++)
        {
            // Instantiate a new object from the prefab
            GameObject obj = Instantiate(prefab, transform.position, Quaternion.identity);

            // Add a 2D rigidbody component to the object
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

            // Apply a random force to the object
            Vector2 force = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            float magnitude = Random.Range(minForce, maxForce);
            rb.AddForce(force * magnitude, ForceMode2D.Impulse);
        }
    }
}
