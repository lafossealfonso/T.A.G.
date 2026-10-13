using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Cycle: Initial wait -> gravity ON (random direction) -> gravity OFF -> gravity ON
/// (a different random direction) -> repeat. All phases are shown on a timer text.
/// Starts when GameManager.GameStarted fires.
/// </summary>
public class GravityShiftObstacle : MonoBehaviour
{
    [Header("References")]
    
    [SerializeField] private TextMeshProUGUI[] timerTexts;
    [SerializeField] private bool inMenu = false;

    [Header("Timing (seconds)")]
    [SerializeField] private float initialWait = 10f;
    [SerializeField] private float gravityDuration = 5f;
    [SerializeField] private float gravityOffDuration = 4f;

    [Header("Gravity")]
    [Tooltip("The directions gravity can be set to. Each activation picks one at random, never the same as the last.")]
    [SerializeField]
    private Vector2[] possibleDirections =
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };
    [SerializeField] private float gravityStrength = 9.81f;
    [Tooltip("gravityScale given to every dynamic Rigidbody2D while gravity is on.")]
    [SerializeField] private float gravityScaleWhileActive = 1f;
    [Tooltip("World gravity while the obstacle is inactive. Leave at (0, 0) for no gravity.")]
    [SerializeField] private Vector2 gravityWhileInactive = Vector2.zero;
    [Tooltip("How much of its speed each body keeps when gravity turns off. 0 = stops dead, 1 = keeps all of it.")]
    [Range(0f, 1f)]
    [SerializeField] private float velocityKeptWhenGravityEnds = 0f;

    [Header("Timer Text Labels")]
    [SerializeField] private string initialWaitLabel = "GRAVITY IN";
    [Tooltip("{0} is replaced with the direction name, e.g. LEFT.")]
    [SerializeField] private string gravityOnLabel = "GRAVITY {0} ENDS IN";
    [SerializeField] private string gravityOffLabel = "GRAVITY RETURNS IN";

    [Header("Background")]
    [Tooltip("Leave empty to use the Main Camera.")]
    [SerializeField] private Camera backgroundCamera;
    [SerializeField] private Color gravityBackgroundColor = new Color(0.25f, 0.05f, 0.05f);
    [SerializeField] private float backgroundFadeTime = 0.5f;

    [Header("Direction Indicator")]
    [Tooltip("The object (arrow sprite or UI image) that points to where gravity will pull next.")]
    [SerializeField] private Transform directionIndicator;
    [Tooltip("Angle correction for your sprite. 0 if it points right by default, -90 if it points up.")]
    [SerializeField] private float indicatorAngleOffset = -90f;
    [SerializeField] private float indicatorRotateDuration = 1f;
    [Tooltip("Higher = the bounce dies out faster / overshoots less.")]
    [SerializeField] private float bounceDamping = 6f;
    [Tooltip("How many wobbles happen during the rotation.")]
    [SerializeField] private float bounceFrequency = 2f;

    private Coroutine indicatorRoutine;

    private Color originalBackgroundColor;
    private Coroutine backgroundRoutine;

    // Remembered so we can put everything back exactly how we found it.
    private Vector2 originalGravity;
    private readonly Dictionary<Rigidbody2D, float> originalGravityScales = new();
    private Coroutine cycleRoutine;

    // Index into possibleDirections of the last direction used (-1 = none yet).
    private int lastDirectionIndex = -1;

    private void OnEnable()
    {
        originalGravity = Physics2D.gravity;

        if (backgroundCamera == null) backgroundCamera = Camera.main;
        if (backgroundCamera != null)
            originalBackgroundColor = backgroundCamera.backgroundColor;

        GameManager.GameStarted += SetActive;

        if(inMenu == true)
        {
            if (cycleRoutine != null) return;

            lastDirectionIndex = -1;
            cycleRoutine = StartCoroutine(GravityCycle());
        }
    }

    private void OnDisable()
    {
        GameManager.GameStarted -= SetActive;

        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }

        if (backgroundRoutine != null)
        {
            StopCoroutine(backgroundRoutine);
            backgroundRoutine = null;
        }

        // Snap straight back (no fade), because a disabled object can't run a coroutine.
        if (backgroundCamera != null)
            backgroundCamera.backgroundColor = originalBackgroundColor;

        DeactivateGravity();
        Physics2D.gravity = originalGravity;

        if (indicatorRoutine != null)
        {
            StopCoroutine(indicatorRoutine);
            indicatorRoutine = null;
        }
    }

    private void SetActive(bool active)
    {
        // Guard against the event firing twice and starting two cycles at once.
        if (cycleRoutine != null) return;

        lastDirectionIndex = -1;
        cycleRoutine = StartCoroutine(GravityCycle());
    }

    private IEnumerator GravityCycle()
    {
        // 1. Initial wait, with no gravity. Pick and show the first direction now.
        Physics2D.gravity = gravityWhileInactive;

        int nextIndex = PickNewDirectionIndex();
        RotateIndicatorTowards(possibleDirections[nextIndex]);
        yield return RunPhase(initialWait, initialWaitLabel);

        while (true)
        {
            // 2 + 3. Gravity on, using the direction the indicator has been showing
            Vector2 activeDirection = possibleDirections[nextIndex];

            ActivateGravity(activeDirection);
            FadeBackgroundTo(gravityBackgroundColor);
            yield return RunPhase(gravityDuration, string.Format(gravityOnLabel, DirectionName(activeDirection)));

            // 4. Gravity off: choose the NEXT direction and swing the indicator to it
            DeactivateGravity(true);
            FadeBackgroundTo(originalBackgroundColor);

            nextIndex = PickNewDirectionIndex();
            RotateIndicatorTowards(possibleDirections[nextIndex]);
            yield return RunPhase(gravityOffDuration, gravityOffLabel);

            // Loop back round -> gravity activates in the direction that was just shown
        }
    }

    /// <summary>
    /// Picks a random index that is never the same as the previous one.
    /// </summary>
    private int PickNewDirectionIndex()
    {
        int count = possibleDirections.Length;

        // Nothing to choose between if there are fewer than two directions.
        if (count <= 1)
        {
            lastDirectionIndex = 0;
            return 0;
        }

        int index;

        if (lastDirectionIndex < 0)
        {
            // First time: anything goes.
            index = Random.Range(0, count);
        }
        else
        {
            // Pick from the (count - 1) directions that aren't the last one.
            // Anything at or past the last index gets shifted up by one to skip over it.
            index = Random.Range(0, count - 1);
            if (index >= lastDirectionIndex) index++;
        }

        lastDirectionIndex = index;
        return index;
    }

    private string DirectionName(Vector2 direction)
    {
        if (direction == Vector2.up) return "UP";
        if (direction == Vector2.down) return "DOWN";
        if (direction == Vector2.left) return "LEFT";
        if (direction == Vector2.right) return "RIGHT";
        return "SHIFTS"; // for custom directions, e.g. diagonals
    }

    /// <summary>
    /// Counts down for 'duration' seconds, updating the timer text every frame.
    /// </summary>
    private IEnumerator RunPhase(float duration, string label)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            SetTimerText(label, duration - elapsed);

            yield return null;
            elapsed += Time.deltaTime;
        }
    }

    private void SetTimerText(string label, float secondsRemaining)
    {
        if (timerTexts == null) return;

        // Build the string once, then hand the same one to every text field.
        string text = label + Mathf.Max(0f, secondsRemaining).ToString("0.0");

        foreach (TextMeshProUGUI timerText in timerTexts)
        {
            if (timerText != null) timerText.text = text;
        }
    }

    private void FadeBackgroundTo(Color target)
    {
        if (backgroundCamera == null) return;

        // Stop any fade already running so two fades don't fight each other.
        if (backgroundRoutine != null) StopCoroutine(backgroundRoutine);
        backgroundRoutine = StartCoroutine(FadeBackgroundRoutine(target));
    }

    private IEnumerator FadeBackgroundRoutine(Color target)
    {
        Color start = backgroundCamera.backgroundColor;
        float elapsed = 0f;

        while (elapsed < backgroundFadeTime)
        {
            elapsed += Time.deltaTime;
            backgroundCamera.backgroundColor =
                Color.Lerp(start, target, elapsed / backgroundFadeTime);
            yield return null;
        }

        backgroundCamera.backgroundColor = target;
    }

    private void ActivateGravity(Vector2 direction)
    {
        Physics2D.gravity = direction.normalized * gravityStrength;

        // Re-scan every time so players who joined after Start are included.
        Rigidbody2D[] bodies = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);

        foreach (Rigidbody2D body in bodies)
        {
            // Static/Kinematic bodies ignore gravity anyway.
            if (body.bodyType != RigidbodyType2D.Dynamic) continue;

            if (!originalGravityScales.ContainsKey(body))
                originalGravityScales[body] = body.gravityScale;

            body.gravityScale = gravityScaleWhileActive;
        }
    }

    private void DeactivateGravity(bool applyVelocityLoss = false)
    {
        foreach (KeyValuePair<Rigidbody2D, float> pair in originalGravityScales)
        {
            // Unity's "== null" also catches objects that have been destroyed.
            if (pair.Key == null) continue;

            pair.Key.gravityScale = pair.Value;

            if (applyVelocityLoss)
            {
                pair.Key.linearVelocity *= velocityKeptWhenGravityEnds;
                pair.Key.angularVelocity *= velocityKeptWhenGravityEnds;
            }
        }

        originalGravityScales.Clear();
        Physics2D.gravity = gravityWhileInactive;
    }
    private void RotateIndicatorTowards(Vector2 direction)
    {
        if (directionIndicator == null) return;

        // Atan2 turns a direction (x, y) into an angle in radians, so convert to degrees.
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + indicatorAngleOffset;

        // Stop any rotation already running so two don't fight each other.
        if (indicatorRoutine != null) StopCoroutine(indicatorRoutine);
        indicatorRoutine = StartCoroutine(RotateIndicatorRoutine(targetAngle));
    }

    private IEnumerator RotateIndicatorRoutine(float targetAngle)
    {
        float startAngle = directionIndicator.eulerAngles.z;

        // DeltaAngle gives the shortest signed turn from start to target
        // (e.g. 350 -> 10 is +20, not -340).
        float totalRotation = Mathf.DeltaAngle(startAngle, targetAngle);

        float elapsed = 0f;

        while (elapsed < indicatorRotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / indicatorRotateDuration);

            // Spring curve: 0 at the start, swings past 1, then settles on 1.
            float progress = 1f - Mathf.Exp(-bounceDamping * t) *
                                  Mathf.Cos(bounceFrequency * 2f * Mathf.PI * t);

            float angle = startAngle + totalRotation * progress;
            directionIndicator.rotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        directionIndicator.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }


}