using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{

    public List<Transform> linkedPortals;

    private void OnTriggerEnter2D(Collider2D other)
    {

        

        if (other.gameObject.CompareTag("Player"))
        {
            int randomIndex = Random.Range(0, linkedPortals.Count);
            other.transform.position = linkedPortals[randomIndex].position;
            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            playerMovement.PlayerTeleported();
        }
    }
}
