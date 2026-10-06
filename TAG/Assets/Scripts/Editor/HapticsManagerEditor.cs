using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(HapticsManager))]
public class HapticsManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the normal Inspector (your pattern fields) first.
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);

        // Buttons only work while the game is running.
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            HapticsManager manager = (HapticsManager)target;

            if (GUILayout.Button("Play: Became It"))
                manager.Play(Gamepad.current, HapticType.BecameIt);

            if (GUILayout.Button("Play: Got Tagged"))
                manager.Play(Gamepad.current, HapticType.GotTagged);
        }

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("Enter Play mode to test vibrations.", MessageType.Info);
    }
}