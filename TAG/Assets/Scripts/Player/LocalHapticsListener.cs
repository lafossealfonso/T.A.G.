using UnityEngine;
using UnityEngine.InputSystem;

public class LocalHapticsListener : MonoBehaviour
{
    private void OnEnable() { GameManager.OnPlayerTagged += HandleTagged; }
    private void OnDisable() { GameManager.OnPlayerTagged -= HandleTagged; }

    private void HandleTagged(GameObject tagger, GameObject tagged)
    {
        PlayFor(tagger, HapticType.BecameIt);
        PlayFor(tagged, HapticType.GotTagged);
    }

    private void PlayFor(GameObject player, HapticType type)
    {
        PlayerInput input = player.GetComponentInParent<PlayerInput>();
        if (input == null) return;

        // Each PlayerInput knows which device(s) it's paired with.
        foreach (InputDevice device in input.devices)
        {
            if (device is Gamepad pad)
            {
                HapticsManager.Instance.Play(pad, type);
                break;
            }
        }
    }
}