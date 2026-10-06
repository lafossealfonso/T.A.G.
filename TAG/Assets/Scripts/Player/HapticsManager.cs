using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum HapticType { BecameIt, GotTagged }

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance;

    [System.Serializable]
    public class HapticPattern
    {
        [Range(0f, 1f)] public float lowFrequency;
        [Range(0f, 1f)] public float highFrequency;
        public float duration = 0.2f;
        // 1 = full strength, 0 = off. Curve is sampled across the duration.
        public AnimationCurve fade = AnimationCurve.Linear(0, 1, 1, 0);
    }

    [SerializeField]
    private HapticPattern becameIt = new HapticPattern
    { lowFrequency = 0.1f, highFrequency = 1f, duration = 0.15f };
    [SerializeField]
    private HapticPattern gotTagged = new HapticPattern
    { lowFrequency = 1f, highFrequency = 0.3f, duration = 0.5f };

    // Remembers the running vibration per controller so a new one replaces the old.
    private readonly Dictionary<Gamepad, Coroutine> running = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Play(Gamepad pad, HapticType type)
    {
        if (pad == null) return; // keyboard players have no gamepad

        if (running.TryGetValue(pad, out Coroutine old) && old != null)
            StopCoroutine(old);

        HapticPattern p = type == HapticType.BecameIt ? becameIt : gotTagged;
        running[pad] = StartCoroutine(Rumble(pad, p));
    }

    private IEnumerator Rumble(Gamepad pad, HapticPattern p)
    {
        float t = 0f;
        while (t < p.duration)
        {
            float strength = p.fade.Evaluate(t / p.duration);
            pad.SetMotorSpeeds(p.lowFrequency * strength, p.highFrequency * strength);
            t += Time.deltaTime;
            yield return null;
        }
        pad.SetMotorSpeeds(0f, 0f);
        running.Remove(pad);
    }

    private void OnDisable()
    {
        foreach (Gamepad pad in running.Keys) pad?.ResetHaptics();
        running.Clear();
    }
}
