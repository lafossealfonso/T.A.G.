using DG.Tweening.Core.Easing;
using MoreMountains.Feedbacks;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BasicGameMode : MonoBehaviour
{
    [SerializeField] MMF_Player startFadePlayer;
    [Header("Round Transition Fade Settings")]
    [SerializeField] MMF_Player roundFadePlayer;
    [SerializeField] TextMeshProUGUI roundFadePlayerLabel;
    [SerializeField] TextMeshProUGUI roundFadeCounter;
    [SerializeField] Image roundFadeImage;
    public PlayerManager playerManager;
    public List<Transform> startPoints;
    public Transform starTransform;
    [Header("Score Settings")]
    public float roundNumberLimit;
    public int numberOfRoundsPlayed;
    public List<Image> scoreKeepers;
    public Transform spawnPoint;
    public CinemachineCamera winnerCamera;
    [SerializeField] TextMeshProUGUI winnerDisplayName;
    [Header("Game Manager Set-Up")]
    [SerializeField] MMF_Player feedbackPlayerTagged;
    bool gameEnded;
    [SerializeField] bool funnyOn = false;
    [SerializeField] FunnyScript funnyScript;
    private void OnEnable()
    {
        GameManager.OnPlayerTagged += HandlePlayerTagged;
        GameManager.OnWinnerChosen += WinnerSequence;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerTagged -= HandlePlayerTagged;
        GameManager.OnWinnerChosen -= WinnerSequence;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }



    private void Start()
    {
        startFadePlayer.PlayFeedbacks();
        GameManager.Instance.playerTaggedEffectPlayer = feedbackPlayerTagged;
    }

    void HandlePlayerTagged(GameObject taggingPlayer, GameObject taggedPlayer)
    {
        taggingPlayer.GetComponent<PlayerMovement>().setIsIt(true);
        taggedPlayer.transform.position = spawnPoint.position;

        if(funnyOn == true)
        {
            funnyScript.PlayFunny();
        }

        
    }

    void WinnerSequence(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        Basic_PlayerScoreCard playerCard = playerMovement.thisPlayerScoreCard;

        playerCard.playerScore += 1;
        numberOfRoundsPlayed += 1;
        scoreKeepers[numberOfRoundsPlayed - 1 ].color = playerCard.playerColor;

        ResetLevelForNextRound();

        if (playerCard.playerScore >= roundNumberLimit / 2)
        {
            PlayerManager.Instance.DisablePlayerMovement();
            winnerCamera.Follow = player.gameObject.transform;
            winnerCamera.gameObject.SetActive(true);

            winnerDisplayName.text = playerCard.playerName;
            winnerDisplayName.color = playerCard.playerColor;
            winnerDisplayName.gameObject.transform.parent.gameObject.SetActive(true);
            gameEnded = true;                    
        }
    }



    void ResetLevelForNextRound()
    {
        roundFadePlayer.PlayFeedbacks();
        playerManager.ResetPlayerPositions();
        starTransform.gameObject.SetActive(true);
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
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene("Menu");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!this || gameObject == null) return;

        StartCoroutine(OnSceneReady());
    }
    private IEnumerator OnSceneReady()
    {
        yield return new WaitForEndOfFrame();
        yield return null;

        startFadePlayer.PlayFeedbacks();
    }
}
