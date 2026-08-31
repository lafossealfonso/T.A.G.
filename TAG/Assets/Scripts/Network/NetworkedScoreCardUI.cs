using TMPro;
using UnityEngine;
using UnityEngine.UI;
 
public class NetworkedScoreCardUI : MonoBehaviour
{
    [Header("UI References (mirrors Basic_PlayerScoreCard fields)")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Slider scoreSlider;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private Image fillImage;

    private OnlineBasic_PlayerScoreCard boundScoreCard;

    public void BindToPlayer(OnlineBasic_PlayerScoreCard scoreCard, PlayerProfile profile)
    {
        // Unbind from whoever was here before -- guards against double
        // subscription if this ever gets called twice for the same slot.
        Unbind();

        boundScoreCard = scoreCard;
        boundScoreCard.scoreValue.OnValueChanged += HandleScoreChanged;

        if (profile != null)
        {
            if (playerNameText != null)
            {
                playerNameText.text = profile.playerName;
                playerNameText.color = profile.playerColor;
            }
            if (fillImage != null) fillImage.color = profile.playerColor;
            if (percentageText != null) percentageText.color = profile.playerColor;
        }

        HandleScoreChanged(0f, boundScoreCard.scoreValue.Value);
    }

    private void HandleScoreChanged(float previousValue, float newValue)
    {
        if (scoreSlider != null) scoreSlider.value = newValue;
        if (percentageText != null) percentageText.text = Mathf.RoundToInt(newValue) + "%";
    }

    private void Unbind()
    {
        if (boundScoreCard != null)
        {
            boundScoreCard.scoreValue.OnValueChanged -= HandleScoreChanged;
            boundScoreCard = null;
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
