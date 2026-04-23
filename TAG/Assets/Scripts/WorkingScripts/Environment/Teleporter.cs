using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject[] Exits;
    public bool teleportable;

    private void Start()
    {
        teleportable = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform collidingTransform = collision.gameObject.transform;
        if (collidingTransform != null && teleportable == true)
        {
            if (collision.gameObject.tag == "ChildCollider")
            {
                collidingTransform.parent.position = GetRandomExit().transform.position;
                StartCoroutine(TeleportDelay());
            }
            else
            {
                collidingTransform.position = GetRandomExit().transform.position;
                StartCoroutine(TeleportDelay());
            }
        }


    }

    public GameObject GetRandomExit()
    {
        int randomIndex = Random.Range(0, Exits.Length);
        StartCoroutine(Exits[randomIndex].gameObject.GetComponentInParent<Teleporter>().TeleportDelay());
        return Exits[randomIndex];
    }
    private IEnumerator TeleportDelay()
    {
        teleportable = false;
        yield return new WaitForSeconds(1f);
        teleportable = true;
    }

}
