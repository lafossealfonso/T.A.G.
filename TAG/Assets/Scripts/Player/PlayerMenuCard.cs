using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class PlayerMenuCard : MonoBehaviour
{
    public TextMeshProUGUI menuText;
    public MMF_Player thisFeedbackPlayer;
    public GameObject optionsButton;

    public void ReadyUp(Color playerColor)
    {
        menuText.text = "READY";
        optionsButton.SetActive(false);
        menuText.fontStyle = (FontStyles)FontStyle.Bold;
        menuText.color = playerColor;

    }
    public void PlayFeedback(int playerIndex)
    {
        thisFeedbackPlayer.MMF_Channel = playerIndex;
        thisFeedbackPlayer.PlayFeedbacks();
    }

}
