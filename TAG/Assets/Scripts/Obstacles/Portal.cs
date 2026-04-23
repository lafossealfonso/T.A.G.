using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    public List<Transform> linkedPortals;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.name + "entered portal trigger");
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            int randomIndex = Random.Range(0, linkedPortals.Count);
            other.gameObject.transform.position = linkedPortals[randomIndex].position;
            playerMovement.PlayerTeleported();
        }
    }
}
