using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkedReadyUI : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image tickFrame;
    [SerializeField] private GameObject readyTick;

    private NetworkedPlayerData boundPlayerData;

    [Header("Feedback")]
    [SerializeField] private MMF_Player nudgeFeedbackPlayer; // same as PlayerMenuCard.thisFeedbackPlayer

    public void PlayNudgeFeedback(int channel)
    {
        if (nudgeFeedbackPlayer == null) return;
        nudgeFeedbackPlayer.MMF_Channel = channel;
        nudgeFeedbackPlayer.PlayFeedbacks();
    }

    public void BindToPlayer(NetworkedPlayerData playerData, PlayerProfile profile)
    {
        Unbind();

        boundPlayerData = playerData;
        boundPlayerData.isReady.OnValueChanged += HandleReadyChanged;

        if (profile != null)
        {

            if (tickFrame != null) tickFrame.gameObject.SetActive(true); tickFrame.color = profile.playerColor;
        }

        HandleReadyChanged(false, boundPlayerData.isReady.Value);
    }

    private void HandleReadyChanged(bool previousValue, bool newValue)
    {
        if (readyTick != null) readyTick.SetActive(newValue);
        Debug.Log(newValue);
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
