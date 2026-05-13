using DG.Tweening.Core.Easing;
using MoreMountains.Feedbacks;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BasicGameMode : MonoBehaviour
{
    [SerializeField] MMF_Player startFadePlayer;
    public PlayerManager playerManager;
    public List<Transform> spawnPoints;
    public CinemachineCamera winnerCamera;
    [SerializeField] TextMeshProUGUI winnerDisplayName;
    bool gameEnded;
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

    private void Start()
    {
        startFadePlayer.PlayFeedbacks();
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
        gameEnded = true;
    }

    void Update()
    {
        if (gameEnded == false) return;

        bool spacePressed = UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;

        bool controllerPressed =
            UnityEngine.InputSystem.Gamepad.current != null &&
            UnityEngine.InputSystem.Gamepad.current.buttonNorth.wasPressedThisFrame;

        if (spacePressed || controllerPressed)
        {
            Reload();
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
