using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteOnCollision : MonoBehaviour
{
    public GameObject objectToDisable;

    // The prefab to instantiate
    public List<GameObject> prefabList;

    // The minimum and maximum force to apply to each object
    public float minForce = 5f;
    public float maxForce = 15f;

    

    // The number of objects to instantiate
    public int objectCount = 10;

    public float destroyDelay = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Turn off the game object
            objectToDisable.SetActive(false);

            

            for (int i = 0; i < objectCount; i++)
            {
                int n = Random.Range(0, prefabList.Count);
                // Instantiate a new object from the prefab
                GameObject obj = Instantiate(prefabList[n], transform.position, Quaternion.identity);

                // Add a 2D rigidbody component to the object
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

                // Apply a random force to the object
                Vector2 force = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)).normalized;
                float magnitude = Random.Range(minForce, maxForce);
                rb.AddForce(force * magnitude, ForceMode2D.Impulse);


                Destroy(obj, destroyDelay);
            }
        }
    }
}
