using DG.Tweening.Core.Easing;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicGameMode : MonoBehaviour
{
    public PlayerManager playerManager;
    public List<Transform> spawnPoints;
    public CinemachineCamera winnerCamera;
    [SerializeField] TextMeshProUGUI winnerDisplayName;
    private void OnEnable()
    {
        GameManager.OnPlayerTagged += HandlePlayerTagged;
        GameManager.OnWinnerChosen += WinnerSequence;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerTagged -= HandlePlayerTagged;
        GameManager.OnWinnerChosen -= WinnerSequence;
    }

    void HandlePlayerTagged(GameObject taggingPlayer, GameObject taggedPlayer)
    {
        taggingPlayer.GetComponent<PlayerMovement>().setIsIt(true);
        taggedPlayer.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].position;

        
    }

    void WinnerSequence(GameObject player)
    {
        PlayerManager.Instance.DisablePlayerMovement();
        winnerCamera.Follow = player.gameObject.transform;
        winnerCamera.gameObject.SetActive(true);
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        Basic_PlayerScoreCard playerCard = playerMovement.thisPlayerScoreCard;
        winnerDisplayName.text = playerCard.playerName;
        winnerDisplayName.color = playerCard.playerColor;
        winnerDisplayName.gameObject.transform.parent.gameObject.SetActive(true);
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
