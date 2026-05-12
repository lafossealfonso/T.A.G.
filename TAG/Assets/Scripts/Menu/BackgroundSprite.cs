using UnityEngine;
using UnityEngine.InputSystem;

public class BackgroundSprite : MonoBehaviour
{
    [SerializeField] private float maxDistance = 250f;
    [SerializeField] private float scaleMultiplier = 1.5f;
    [SerializeField] private AnimationCurve scaleCurve;

    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Camera uiCamera;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;

        Canvas canvas = GetComponentInParent<Canvas>();

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCamera = canvas.worldCamera;
    }

    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Vector2 screenPos =
            RectTransformUtility.WorldToScreenPoint(
                uiCamera,
                rectTransform.position);

        float distance = Vector2.Distance(mousePos, screenPos);

        float normalized =
            Mathf.Clamp01(1f - (distance / maxDistance));

        float curveValue = scaleCurve.Evaluate(normalized);

        float scale =
    Mathf.Lerp(scaleMultiplier, 1f, curveValue);

        rectTransform.localScale =
            originalScale * scale;
    }
}
