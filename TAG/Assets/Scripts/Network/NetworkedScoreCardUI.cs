using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedScoreCardUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI percentageText;

    [Header("Sliders")]
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private Slider reverseScoreSlider;
    [SerializeField] private Slider decorSlider;
    [SerializeField] private Image fillImage;
    [Tooltip("Higher = follows the networked score more tightly.")]
    [SerializeField] private float displaySmoothing = 12f;

    [Header("Decor Fill (replays on every profile change)")]
    [SerializeField] private AnimationCurve fillCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float duration = 0.8f;

    [Header("Objects shown only once a player is bound")]
    [SerializeField] private List<GameObject> turnOffObjectList;

    [Header("Decor Items")]
    [SerializeField] private List<Image> recolorImageList;

    [Header("Scale Growth")]
    [SerializeField] private Vector3 maxScale = Vector3.one * 1.3f;
    [SerializeField] private float scaleLerpSpeed = 6f;

    [Header("Feedback")]
    [SerializeField] private MMF_Player isItFeedbackPlayer;

    private OnlineBasic_PlayerScoreCard boundScoreCard;
    private PlayerProfile boundProfile;

    private Vector3 baseScale;
    private float decorTimer;
    private bool decorFill;
    private int lastPercent = -1;

    private void Awake()
    {
        baseScale = transform.localScale;

        // Default "waiting for a player" state, same as local before AssignPlayer.
        SetTurnOffObjects(false);
        if (decorSlider != null) decorSlider.value = 0f;
        decorFill = false;
    }

    public void BindToPlayer(OnlineBasic_PlayerScoreCard scoreCard, PlayerProfile profile)
    {
        // Slots beyond the min player count start hidden; a join turns them on.
        gameObject.SetActive(true);

        bool newPlayer = boundScoreCard != scoreCard;
        boundScoreCard = scoreCard;

        // Jump straight to the current score for a new player (e.g. joining mid-game).
        if (newPlayer && scoreCard != null) SnapSliders(scoreCard.scoreValue.Value);

        // Slot can arrive before the profile does. The profile change triggers another bind.
        if (profile == null) return;

        SetTurnOffObjects(true);
        ApplyColour(profile.playerColor);
        if (playerNameText != null) playerNameText.text = profile.playerName;

        if (profile != boundProfile)
        {
            boundProfile = profile;
            StartDecorFill();
        }
    }

    public bool IsBoundTo(OnlineBasic_PlayerScoreCard scoreCard)
    {
        return boundScoreCard == scoreCard;
    }

    public void ClearPlayer()
    {
        boundScoreCard = null;
        boundProfile = null;
        decorFill = false;
        if (decorSlider != null) decorSlider.value = 0f;
        SetTurnOffObjects(false);
        ResetSliders();
    }

    public void ResetSliders()
    {
        SnapSliders(0f);
    }

    public void PlayIsItFeedback()
    {
        if (isItFeedbackPlayer != null) isItFeedbackPlayer.PlayFeedbacks();
    }

    private void Update()
    {
        DecorFillSlider();

        if (boundScoreCard != null)
        {
            float target = boundScoreCard.scoreValue.Value;
            float t = 1f - Mathf.Exp(-displaySmoothing * Time.deltaTime);
            scoreSlider.value = Mathf.Lerp(scoreSlider.value, target, t);

            if (reverseScoreSlider != null)
                reverseScoreSlider.value = reverseScoreSlider.maxValue - scoreSlider.value;
        }

        int percent = Mathf.RoundToInt(scoreSlider.value);
        if (percent != lastPercent && percentageText != null)
        {
            lastPercent = percent;
            percentageText.text = percent + "%";
        }

        float normalized = scoreSlider.value / scoreSlider.maxValue;
        Vector3 targetScale = Vector3.Lerp(baseScale, maxScale, normalized);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleLerpSpeed * Time.deltaTime);
    }

    private void StartDecorFill()
    {
        decorTimer = 0f;
        decorFill = true;
        if (decorSlider != null) decorSlider.value = 0f;
    }

    private void DecorFillSlider()
    {
        if (!decorFill || decorSlider == null) return;

        decorTimer += Time.deltaTime;
        float t = Mathf.Clamp01(decorTimer / duration);
        decorSlider.value = fillCurve.Evaluate(t) * 100f;

        if (t >= 1f)
        {
            decorFill = false;
            decorTimer = 0f;
        }
    }

    private void ApplyColour(Color color)
    {
        if (playerNameText != null) playerNameText.color = color;
        if (fillImage != null) fillImage.color = color;
        if (percentageText != null) percentageText.color = color;

        foreach (Image image in recolorImageList)
            if (image != null) image.color = color;
    }

    private void SetTurnOffObjects(bool active)
    {
        foreach (GameObject item in turnOffObjectList)
            if (item != null) item.SetActive(active);
    }

    private void SnapSliders(float value)
    {
        if (scoreSlider != null) scoreSlider.value = value;
        if (reverseScoreSlider != null)
            reverseScoreSlider.value = reverseScoreSlider.maxValue - value;
    }
}