using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class PlayerMenuCard : MonoBehaviour
{
    public TextMeshProUGUI menuText;
    public MMF_Player thisFeedbackPlayer;

    public void PlayFeedback(int playerIndex)
    {
        thisFeedbackPlayer.MMF_Channel = playerIndex;
        thisFeedbackPlayer.PlayFeedbacks();
    }

}
