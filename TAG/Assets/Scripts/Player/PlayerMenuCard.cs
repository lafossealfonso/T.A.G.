using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;
using UnityEngine.UI;

public class PlayerMenuCard : MonoBehaviour
{
    public TextMeshProUGUI menuText;
    public MMF_Player thisFeedbackPlayer;
    public GameObject optionsButton;
    public Image cycleColourIcon;
    public TextMeshProUGUI cycleIconText;

    public void ReadyUp(Color playerColor)
    {
        menuText.text = "READY";
        optionsButton.SetActive(false);
        menuText.fontStyle = (FontStyles)FontStyle.Bold;
        menuText.color = playerColor;
        cycleColourIcon.color = playerColor;
        cycleIconText.color = playerColor;
        cycleColourIcon.gameObject.SetActive(true);
        

    }
    public void PlayFeedback(int playerIndex)
    {
        thisFeedbackPlayer.MMF_Channel = playerIndex;
        thisFeedbackPlayer.PlayFeedbacks();
    }

}
