using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Basic_PlayerScoreCard : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI PlayerName;
    [SerializeField] TextMeshProUGUI PlayerCounterText;
    [SerializeField] int scoreCounter;
    private GameObject assignedPlayer;

    public void AssignPlayer(GameObject player, Color color)
    {
        assignedPlayer = player;
        PlayerName.text = "Player";
        scoreCounter = 0;
        UpdateUI();
        SetUIColor(color);
    }

    public bool IsAssignedTo(GameObject player)
    {
        return assignedPlayer == player;
    }

    public void AddScore(int amount)
    {
        scoreCounter += amount;
        UpdateUI();
    }
    private void UpdateUI()
    {
        PlayerCounterText.text = scoreCounter.ToString();
    }

    public void SetUIColor(Color color)
    {
        PlayerName.color = color;
        PlayerCounterText.color = color;
    }

}
