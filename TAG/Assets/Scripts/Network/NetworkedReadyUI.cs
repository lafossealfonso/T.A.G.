using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NetworkedReadyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image colorIcon;
    [SerializeField] private GameObject readyTick;

    private NetworkedPlayerData boundPlayerData;

    public void BindToPlayer(NetworkedPlayerData playerData, PlayerProfile profile)
    {
        Unbind();

        boundPlayerData = playerData;
        boundPlayerData.isReady.OnValueChanged += HandleReadyChanged;

        if (profile != null)
        {
            if (playerNameText != null)
            {
                playerNameText.text = profile.playerName;
                playerNameText.color = profile.playerColor;
            }

            if (colorIcon != null) colorIcon.color = profile.playerColor;
        }

        HandleReadyChanged(false, boundPlayerData.isReady.Value);
    }

    private void HandleReadyChanged(bool previousValue, bool newValue)
    {
        if (readyTick != null) readyTick.SetActive(newValue);
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
