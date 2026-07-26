using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineModelFactoryLine))]
public class SplineModelFactoryLineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SplineModelFactoryLine factory = (SplineModelFactoryLine)target;

        EditorGUILayout.Space();
        
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Pulse"))
        {
            factory.Pulse();
        }
        GUI.enabled = true;
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Pulse can only be tested in Play Mode.", MessageType.Info);
        }
    }
}
