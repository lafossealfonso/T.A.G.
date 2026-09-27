using UnityEngine;
using TMPro;

public class PlayerLabelUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI iconText; // your second TMP text - e.g. a symbol/outline layer

    [Header("Follow Settings")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f); // height above the player's head

    private RectTransform rectTransform;
    private Transform targetPlayer;
    private Camera worldCamera;    // the camera that actually renders your gameplay (main/Cinemachine brain cam)
    private RectTransform canvasRect;
    private Camera canvasCamera;   // null for Overlay canvases

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Call this once when the label is created/placed, so it knows how to do the math
    public void Setup(Camera gameplayCamera, Canvas parentCanvas)
    {
        worldCamera = gameplayCamera;
        canvasRect = parentCanvas.GetComponent<RectTransform>();
        canvasCamera = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : parentCanvas.worldCamera;
    }

    public void AssignPlayer(Transform player, Color color, string playerName)
    {
        targetPlayer = player;

        iconText.text = playerName;
        nameText.text = playerName;
        nameText.color = color;

        

        gameObject.SetActive(true);
    }

    public void ClearPlayer()
    {
        targetPlayer = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (targetPlayer == null || worldCamera == null) return;

        Vector3 worldPosition = targetPlayer.position + worldOffset;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPoint, canvasCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }
}
