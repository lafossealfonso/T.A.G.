using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedGameMode : MonoBehaviour
{
    public static NetworkedGameMode Instance { get; private set; }

    [Header("Tag Settings")]
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private MMF_Player feedbackPlayerTagged;

    [Header("Round Settings (same as BasicGameMode)")]
    [SerializeField] private int roundsToWin = 2;
    public int RoundsToWin { get { return roundsToWin; } }
    [SerializeField] private MMF_Player startFadePlayer;

    [Header("Round Feedback Settings")]
    [SerializeField] private MMF_Player roundScoreFeedbackPlayer;
    [SerializeField] private TextMeshProUGUI roundScorePlayerText;
    [SerializeField] private TextMeshProUGUI roundScoreNumberText;

    [Header("Winning Transition Fade Settings")]
    [SerializeField] private MMF_Player roundFadePlayer;
    [SerializeField] private TextMeshProUGUI roundFadePlayerLabel;
    [SerializeField] private Image roundFadeIconSML;
    [SerializeField] private Image roundFadeIconMED;
    [SerializeField] private Image roundFadeIconLRG;

    [Header("Scorekeepers")]
    [SerializeField] private GameObject scoreKeeperPrefab;
    [SerializeField] private GameObject scoreKeeperParentA;
    [SerializeField] private GameObject scoreKeeperParentB;
    [SerializeField] private List<Image> scoreKeepers = new List<Image>();

    private static readonly string[] RoundNames = { "ONE", "TWO", "THREE", "FOUR", "FIVE" };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (startFadePlayer != null) startFadePlayer.PlayFeedbacks();

        int players = NetworkedPlayerManager.Instance != null
            ? NetworkedPlayerManager.Instance.PlayerCount : 0;
        RefreshScoreKeepers(players);
    }

    public void RefreshScoreKeepers(int playerCount)
    {
        if (scoreKeeperPrefab == null) return;

        // Once the game has started the set is fixed, even if someone disconnects.
        if (NetworkedGameManager.Instance != null &&
            NetworkedGameManager.Instance.gameStarted.Value) return;

        int target = playerCount * (roundsToWin - 1) + 1;

        while (scoreKeepers.Count < target) AddScoreKeeper();
        while (scoreKeepers.Count > target) RemoveScoreKeeper();
    }

    private void AddScoreKeeper()
    {
        GameObject parent = (scoreKeeperParentA.transform.childCount >= 3)
            ? scoreKeeperParentB : scoreKeeperParentA;

        GameObject keeper = Instantiate(scoreKeeperPrefab, parent.transform);
        scoreKeepers.Add(keeper.GetComponent<Image>());
    }

    private void RemoveScoreKeeper()
    {
        int last = scoreKeepers.Count - 1;
        if (scoreKeepers[last] != null) Destroy(scoreKeepers[last].gameObject);
        scoreKeepers.RemoveAt(last);
    }

    

    public Vector3 RespawnPosition
    {
        get { return respawnPoint != null ? respawnPoint.position : Vector3.zero; }
    }

    // The server uses this as its round clock (the host has this scene object too).
    public float RoundSplashDuration
    {
        get { return roundScoreFeedbackPlayer != null ? roundScoreFeedbackPlayer.TotalDuration : 0f; }
    }

    // Runs on EVERY machine when a tag happens.
    public void PlayTagEffects()
    {
        if (feedbackPlayerTagged != null) feedbackPlayerTagged.PlayFeedbacks();
    }

    // Local mode adds one scorekeeper per joining player. Rounds don't depend on
    // joins, so online creates one per possible round up front.
    

    // Online version of the visual half of BasicGameMode.WinnerSequence.
    public void ShowRoundWon(int winnerProfileIndex, int roundNumber, bool gameOver)
    {
        if (NetworkedPlayerManager.Instance == null) return;

        PlayerProfile profile = NetworkedPlayerManager.Instance.GetProfile(winnerProfileIndex);
        if (profile == null) return;

        int keeperIndex = roundNumber - 1;
        if (keeperIndex >= 0 && keeperIndex < scoreKeepers.Count && scoreKeepers[keeperIndex] != null)
        {
            MMF_Player scorePlayer = scoreKeepers[keeperIndex].GetComponent<MMF_Player>();
            if (scorePlayer != null) scorePlayer.PlayFeedbacks();
            scoreKeepers[keeperIndex].color = profile.playerColor;
        }

        if (roundFadePlayerLabel != null) roundFadePlayerLabel.text = profile.playerName;

        if (gameOver)
        {
            NetworkedPlayerManager.Instance.TurnOffPlayerLabels(false);

            if (roundFadePlayerLabel != null) roundFadePlayerLabel.color = profile.playerColor;
            if (roundFadeIconSML != null) roundFadeIconSML.color = profile.playerColor * 0.55f;
            if (roundFadeIconMED != null) roundFadeIconMED.color = profile.playerColor * 0.9f;
            if (roundFadeIconLRG != null) roundFadeIconLRG.color = profile.playerColor * 0.7f;

            if (roundFadePlayer != null) roundFadePlayer.PlayFeedbacks();
        }
        else
        {
            if (roundScorePlayerText != null)
            {
                roundScorePlayerText.color = profile.playerColor;
                roundScorePlayerText.text = profile.playerName;
            }

            if (roundScoreNumberText != null)
            {
                roundScoreNumberText.text = (roundNumber >= 1 && roundNumber <= RoundNames.Length)
                    ? RoundNames[roundNumber - 1] : "NULL";
            }

            if (roundScoreFeedbackPlayer != null) roundScoreFeedbackPlayer.PlayFeedbacks();
        }
    }
}