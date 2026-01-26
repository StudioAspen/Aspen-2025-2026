using CharonsCorner.Runtime;
using UnityEditor;

public static class FlagManagerEditor
{
    [MenuItem("Charons Corner/Reset All Progress")]
    private static void ResetAllFlags()
    {
        if (!EditorUtility.DisplayDialog(
                "Reset All Flags",
                "Are you sure you want to reset ALL progress flags?\nThis cannot be undone.",
                "Reset",
                "Cancel"))
            return;

        FlagManager.ResetAll();

        UnityEngine.Debug.Log("All progress flags have been reset.");
    }
}