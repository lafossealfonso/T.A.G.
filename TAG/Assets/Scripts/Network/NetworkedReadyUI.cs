using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedReadyUI : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI playerNameText;
    
    [SerializeField] private Image readyTick;
    [SerializeField] private Image cycleColourImage;
    [SerializeField] private TextMeshProUGUI cycleColourText;
    [SerializeField] private Image southButton;
    [SerializeField] private GameObject readyUpParent;
    [SerializeField] private GameObject waitingToJoin;

    [SerializeField] private TextMeshProUGUI readyUpText; 

    private NetworkedPlayerData boundPlayerData;

    [Header("Feedback")]
    [SerializeField] private MMF_Player nudgeFeedbackPlayer; // same as PlayerMenuCard.thisFeedbackPlayer
    [SerializeField] private MMF_Player joinedFeedback;

    public void PlayNudgeFeedback(int channel)
    {
        if (nudgeFeedbackPlayer == null) return;
        nudgeFeedbackPlayer.MMF_Channel = channel;
        nudgeFeedbackPlayer.PlayFeedbacks();
    }

    public void PlayJoinFeedback(int channel)
    {
        joinedFeedback.MMF_Channel = channel;
        joinedFeedback.PlayFeedbacks();
    }

    public void BindToPlayer(NetworkedPlayerData playerData, PlayerProfile profile)
    {
        Unbind();

        boundPlayerData = playerData;
        boundPlayerData.isReady.OnValueChanged += HandleReadyChanged;

        if (profile != null)
        {

            if (profile != null && readyUpParent != null)
            {
                readyUpParent.SetActive(true);
                if (waitingToJoin != null) waitingToJoin.SetActive(false);

                if (readyUpText != null) readyUpText.color = profile.playerColor;
                if (readyTick != null) readyTick.color = profile.playerColor;
                if (cycleColourImage != null) cycleColourImage.color = profile.playerColor;
                if (cycleColourText != null) cycleColourText.color = profile.playerColor;
                if(southButton != null) southButton.color = profile.playerColor;
            }

        }

        HandleReadyChanged(false, boundPlayerData.isReady.Value);
    }

    private void HandleReadyChanged(bool previousValue, bool newValue)
    {
        if (readyTick != null) readyTick.gameObject.SetActive(newValue);
        if(southButton != null) southButton.gameObject.SetActive(!newValue);
        if (cycleColourImage != null) cycleColourImage.gameObject.SetActive(!newValue);
        joinedFeedback.PlayFeedbacks();
    }
    private void Unbind()
    {
        if (boundPlayerData != null)
        {
            boundPlayerData.isReady.OnValueChanged -= HandleReadyChanged;
            boundPlayerData = null;
        }
    }

    private void OnDestroy()
    {
        Unbind();
    }
}
