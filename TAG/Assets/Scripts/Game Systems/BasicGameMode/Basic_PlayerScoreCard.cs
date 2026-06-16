using MoreMountains.Feedbacks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Basic_PlayerScoreCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI PlayerName;
    [Header("Sliders")]
    [SerializeField] Slider scoreSlider;
    [SerializeField] Slider reverseScoreSlider;
    [SerializeField] Slider decorSlider;
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float duration = 0.8f;
    private float timer;
    bool decorFill = false;

    [SerializeField] Image fillSlideRenderer;
    
    
    [SerializeField] public float fillSpeed;
    [Header("Percentage")]
    [SerializeField] TextMeshProUGUI percentageText;
    [SerializeField] Image percentageImage;
    [Header("Turn Off Objects")]
    [SerializeField] List<GameObject> turnOffObjectList;
    [Header("Decor Items")]
    [SerializeField] List<Image> recolorImageList;
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
        foreach (GameObject item in  turnOffObjectList)
        {
            item.SetActive(true);
        }

        decorSlider.value = 0;
        timer = 0f;
        decorFill = true;

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
        percentageText.color = color;
        foreach (Image image in recolorImageList) 
        {
            image.color = color;
        }
        UpdateUI();
    }

    public void UpdateProfileCard(Color colour, string name)
    {

        

        PlayerName.text = name;
        playerName = name;
        playerColor = colour;
        SetUIColor(colour);
        UpdateUI();

        timer = 0f;
        decorSlider.value = 0;
        decorFill = true;
        percentageText.color = colour;
        foreach (Image image in recolorImageList) 
        {
            image.color = colour;
        }

        

    }

    private void Start()
    {
        foreach (GameObject item in turnOffObjectList)
        {
            item.SetActive(false);
        }
        feedbackPlayer.FeedbacksIntensity = 0f;
        decorSlider.value = 0f;
        decorFill = false;
    }
    private void DecorFillSlider()
    {
        if (!decorFill) return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);

        // evaluate curve (controls speed)
        float curved = fillCurve.Evaluate(t);

        decorSlider.value = curved * 100f;

        if (t >= 1f)
        {
            decorFill = false;
            timer = 0f;
        }
    }


    private void Update()
    {

        DecorFillSlider();
        percentageText.text = Mathf.RoundToInt(scoreSlider.value).ToString() + "%";

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

                reverseScoreSlider.value -= fillSpeed * Time.deltaTime;
            }

            else if (playerMovementScript.isIt == false)
            {
                scoreSlider.value -= fillSpeed * 0.3f * Time.deltaTime;
                reverseScoreSlider.value += fillSpeed * 0.3f * Time.deltaTime;
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
