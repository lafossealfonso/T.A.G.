using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class AutoScrollToSelected : MonoBehaviour
{
    [Tooltip("Pixels to move per press. Use item height + Vertical Layout Group spacing.")]
    [SerializeField] private float scrollStep = 100f;
    [SerializeField] private float scrollLerpSpeed = 12f;

    private ScrollRect scrollRect;
    private RectTransform content;
    private int lastIndex = -1;
    private float targetNormalized;
    private bool isScrolling;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
    }

    private void Update()
    {
        CheckSelection();

        if (!isScrolling) return;

        float t = 1f - Mathf.Exp(-scrollLerpSpeed * Time.unscaledDeltaTime);
        scrollRect.verticalNormalizedPosition =
            Mathf.Lerp(scrollRect.verticalNormalizedPosition, targetNormalized, t);

        if (Mathf.Abs(scrollRect.verticalNormalizedPosition - targetNormalized) < 0.001f)
        {
            scrollRect.verticalNormalizedPosition = targetNormalized;
            isScrolling = false;
        }
    }

    private void CheckSelection()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected == null || !selected.transform.IsChildOf(content))
            return;

        // Find which direct child of the content is selected (its position in the list).
        Transform item = selected.transform;
        while (item.parent != content) item = item.parent;
        int index = item.GetSiblingIndex();

        if (index == lastIndex) return;

        int previous = lastIndex;
        lastIndex = index;

        if (previous == -1) return; // first selection, nothing to scroll yet

        Scroll(index > previous ? -1 : 1); // moved down -> scroll down, moved up -> scroll up
    }

    private void Scroll(int direction)
    {
        float scrollableHeight = content.rect.height - scrollRect.viewport.rect.height;
        if (scrollableHeight <= 0f) return; // everything already fits on screen

        // Normalized position: 1 = very top of the list, 0 = very bottom.
        float stepNormalized = scrollStep / scrollableHeight;

        if (!isScrolling)
            targetNormalized = scrollRect.verticalNormalizedPosition;

        targetNormalized = Mathf.Clamp01(targetNormalized + direction * stepNormalized);
        isScrolling = true;
    }
}