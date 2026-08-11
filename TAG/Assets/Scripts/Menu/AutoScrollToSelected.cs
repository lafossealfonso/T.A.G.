using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AutoScrollToSelected : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;
    [SerializeField] private float scrollMultiplier = 0.3f;
    [SerializeField] private float scrollLerpSpeed = 12f;

    private Vector2 targetPosition;
    private GameObject lastSelected;

    private void Awake()
{
    scrollRect = GetComponent<ScrollRect>();
    viewport = scrollRect.viewport;
    content = scrollRect.content;

    targetPosition = content.anchoredPosition;
}
    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();

        if (scrollRect != null)
        {
            viewport = scrollRect.viewport;
            content = scrollRect.content;
        }
    }

    private void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null || selected == lastSelected)
            return;

        RectTransform selectedRect = selected.GetComponent<RectTransform>();

        if (selectedRect == null || !selectedRect.IsChildOf(content))
            return;

        lastSelected = selected;

        ScrollToSelected(selectedRect);
        content.anchoredPosition = Vector2.Lerp(
    content.anchoredPosition,
    targetPosition,
    Time.deltaTime * scrollLerpSpeed
);
    }

    private void ScrollToSelected(RectTransform selectedRect)
    {
        Canvas.ForceUpdateCanvases();

        Bounds selectedBounds =
            RectTransformUtility.CalculateRelativeRectTransformBounds(
                viewport,
                selectedRect
            );

        Rect viewportRect = viewport.rect;

        float moveAmount = 0f;

        // Selected item is above the visible viewport.
        if (selectedBounds.max.y > viewportRect.yMax)
        {
            moveAmount = selectedBounds.max.y - viewportRect.yMax;
        }
        // Selected item is below the visible viewport.
        else if (selectedBounds.min.y < viewportRect.yMin)
        {
            moveAmount = selectedBounds.min.y - viewportRect.yMin;
        }

        if (Mathf.Approximately(moveAmount, 0f))
            return;

        targetPosition.y -= moveAmount * scrollMultiplier;
    }
}