using CharonsCorner.Runtime;
using UnityEditor;
using UnityEngine;

public class ProgressFlagViewerWindow : EditorWindow
{
    private Vector2 _scroll;

    [MenuItem("Charons Corner/Progression Flags")]
    public static void Open()
    {
        GetWindow<ProgressFlagViewerWindow>("Game Flags");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Progress Flags", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("Open Save Data File Location"))
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
        
        if (GUILayout.Button("Reset All Flags"))
        {
            if (EditorUtility.DisplayDialog(
                    "Reset All Flags",
                    "Are you sure you want to reset ALL progress flags?",
                    "Reset",
                    "Cancel"))
            {
                FlagManager.ResetAll();
            }
        }

        EditorGUILayout.Space();
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        foreach (ProgressFlag flag in System.Enum.GetValues(typeof(ProgressFlag)))
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(flag.ToString(), GUILayout.Width(200));

            int currentValue = FlagManager.Get(flag);
            int newValue = EditorGUILayout.IntField(currentValue);

            if (newValue != currentValue)
            {
                FlagManager.Set(flag, newValue);
            }

            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                FlagManager.Set(flag, 0);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}