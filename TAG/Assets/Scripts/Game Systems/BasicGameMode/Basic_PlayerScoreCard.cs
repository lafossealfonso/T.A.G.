using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Basic_PlayerScoreCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI PlayerName;
    [SerializeField] Slider scoreSlider;
    [SerializeField] Image fillSlideRenderer;
    [SerializeField] public float fillSpeed;
    private GameObject assignedPlayer;
    private PlayerMovement playerMovementScript;
    bool playerIsAssigned = false;
    public string playerName;
    public Color playerColor;
    [SerializeField] MMF_Player feedbackPlayer;
    bool feedbackStarted = false;
    [SerializeField] private AnimationCurve intensityCurve;


    public void AssignPlayer(GameObject player, Color color, string name)
    {
        PlayerName.gameObject.SetActive(true);
        scoreSlider.gameObject.SetActive(true);
        scoreSlider.value = 0;
        //PlayerCounterText.gameObject.SetActive(true);
        assignedPlayer = player;
        //scoreCounter = 0;
        SetUIColor(color);
        playerColor = color;
        playerMovementScript = assignedPlayer.gameObject.GetComponent<PlayerMovement>();
        playerMovementScript.thisPlayerScoreCard = this;
        playerIsAssigned = true;
        playerName = name;
        PlayerName.text = name;
        UpdateUI();
    }

    public void UpdateProfileCard(Color colour, string name)
    {
        PlayerName.text = name;
        playerName = name;
        playerColor = colour;
        SetUIColor(colour);
        UpdateUI();

    }

    private void Start()
    {
        PlayerName.gameObject.SetActive(false);
        scoreSlider.gameObject.SetActive(false);
        feedbackPlayer.FeedbacksIntensity = 0f;
    }

    private void Update()
    {
        float normalizedTime = scoreSlider.value / scoreSlider.maxValue;
        float curveValue = intensityCurve.Evaluate(normalizedTime);

        // START
        if (curveValue > 0f && !feedbackStarted)
        {
            feedbackPlayer.PlayFeedbacks();
            feedbackStarted = true;
        }

        // STOP
        if (curveValue <= 0f && feedbackStarted)
        {
            feedbackPlayer.StopFeedbacks();
            feedbackStarted = false;
        }

        // UPDATE
        if (feedbackStarted)
        {
            feedbackPlayer.FeedbacksIntensity = curveValue;
        }

        if (playerIsAssigned)
        {
            if (playerMovementScript.isIt)
            {
                
                scoreSlider.value += fillSpeed * Time.deltaTime;

                if (scoreSlider.value >= scoreSlider.maxValue)
                {
                    GameManager.Instance.WinnerEvent(assignedPlayer);

                }
            }

            else if (playerMovementScript.isIt == false)
            {
                scoreSlider.value -= fillSpeed * 0.3f * Time.deltaTime;
            }
        }
        
    }



    public bool IsAssignedTo(GameObject player)
    {
        return assignedPlayer == player;
    }

    public void AddScore(int amount)
    {
        //scoreCounter += amount;
        UpdateUI();
    }
    private void UpdateUI()
    {
        //PlayerCounterText.text = scoreCounter.ToString();
    }

    public void SetUIColor(Color color)
    {
        PlayerName.color = color;
        fillSlideRenderer.color = color;
        //PlayerCounterText.color = color;
    }

}
