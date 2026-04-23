using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarBehaviour : MonoBehaviour
{
    public List<Transform> playerTransform;
    public GameObject[] Players;
    public float triggerDistance = 5.0f;
    public float fleeDistance = 10.0f;
    public float speed = 5.0f;
    public float obstacleAvoidanceRadius = 2.0f;

    private Vector2 escapeDirection;

    void Start()
    {
        // Set the player transform to the player object in the scene
        Players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i <= 1; i++)
        {
            playerTransform.Add(Players[i].GetComponent<Transform>());
        }
    }

    void Update()
    {
        //Make a dictionary
        Dictionary<Transform, float> dict = new Dictionary<Transform, float>();
        KeyValuePair<Transform, float> C = new KeyValuePair<Transform, float>(null, 100000f);

        //Adding player transforms to a dictionary
        foreach (Transform player in playerTransform)
            dict.Add(player, Vector2.Distance(transform.position, player.position));

        //for each item in dictionary, find closest player
        foreach (KeyValuePair<Transform, float> entry in dict)
        {
            if (entry.Value < C.Value)
            {
                C = entry;
            }
        }

        //Detect if the star is in the middle
        bool middle;
        float player1Distance = dict[playerTransform[0]], player2Distance = dict[playerTransform[1]];
        if (player1Distance >= player2Distance - 50 && player1Distance <= player2Distance + 50)
        {
            middle = true;
        }
        else
        {
            middle = false;
        }

        //if in middle shoot off in random direction
        if (middle)
        {
            float r = Random.Range(-1, 1);
            Vector3 p1dir = playerTransform[0].transform.forward;
            Vector3 p2dir = playerTransform[1].transform.forward;
            Vector3 cross = Vector3.Cross(p1dir, p2dir).normalized;
            escapeDirection = (cross - (transform.position * r)).normalized;
        }
        else   //else run from closest player
        {
            escapeDirection = (transform.position - C.Key.transform.position).normalized;
        }

        // Move the enemy in the escape direction
        transform.position += (Vector3)escapeDirection * speed * Time.deltaTime;
    }
}




