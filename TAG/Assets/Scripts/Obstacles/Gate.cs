using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;

public class Gate : MonoBehaviour
{
    [Header("Gate References")]
    [SerializeField] Transform leftGate;
    [SerializeField] Transform rightGate;

    [Header("Positions")]
    [SerializeField] Vector3 leftClosedPos;
    [SerializeField] Vector3 leftOpenPos;

    [SerializeField] Vector3 rightClosedPos;
    [SerializeField] Vector3 rightOpenPos;

    [Header("Movement")]
    [SerializeField] float openDuration = 1f;
    [SerializeField] float closeDuration = 1f;

    [SerializeField]
    AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Timing")]
    [SerializeField] bool useRandomWait = false;

    [SerializeField] float waitTime = 2f;

    [SerializeField] float minWaitTime = 1f;
    [SerializeField] float maxWaitTime = 4f;

    bool isPlaying = false;

    bool isOpen = false;

    void Start()
    {
        leftGate.localPosition = leftClosedPos;
        rightGate.localPosition = rightClosedPos;

        StartCoroutine(GateLoop());
        
        
    }

    Coroutine gateLoopCoroutine;
    private void OnEnable()
    {
        if (gateLoopCoroutine == null)
        {
            gateLoopCoroutine = StartCoroutine(GateLoop());
        }
    }

    private void OnDisable()
    {
        if (gateLoopCoroutine != null)
        {
            StopCoroutine(gateLoopCoroutine);
            gateLoopCoroutine = null;
        }
    }

    IEnumerator GateLoop()
    {
        while (true)
        {
            yield return MoveGates(
                leftClosedPos,
                leftOpenPos,
                rightClosedPos,
                rightOpenPos,
                openDuration
            );

            isOpen = true;

            yield return new WaitForSeconds(GetWaitTime());

            yield return MoveGates(
                leftOpenPos,
                leftClosedPos,
                rightOpenPos,
                rightClosedPos,
                closeDuration
            );

            isOpen = false;

            yield return new WaitForSeconds(GetWaitTime());
        }
    }

    IEnumerator MoveGates(
        Vector3 leftStart,
        Vector3 leftEnd,
        Vector3 rightStart,
        Vector3 rightEnd,
        float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);

            float curveValue = movementCurve.Evaluate(t);

            leftGate.localPosition =
                Vector3.Lerp(leftStart, leftEnd, curveValue);

            rightGate.localPosition =
                Vector3.Lerp(rightStart, rightEnd, curveValue);

            yield return null;
        }

        leftGate.localPosition = leftEnd;
        rightGate.localPosition = rightEnd;
    }

    float GetWaitTime()
    {
        if (useRandomWait)
        {
            return Random.Range(minWaitTime, maxWaitTime);
        }

        return waitTime;
    }
}
