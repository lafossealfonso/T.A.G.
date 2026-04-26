using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BasicGameMode : MonoBehaviour
{
    public PlayerManager playerManager;
    public List<Transform> spawnPoints;
    private void OnEnable()
    {
        GameManager.OnPlayerTagged += HandlePlayerTagged;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerTagged -= HandlePlayerTagged;
    }

    void HandlePlayerTagged(GameObject taggingPlayer, GameObject taggedPlayer)
    {
        taggedPlayer.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].position;

        foreach (Basic_PlayerScoreCard card in playerManager.scoreCards)
        {
            if (card.IsAssignedTo(taggingPlayer))
            {
                card.AddScore(1);
                break;
            }
        }
    }
}
