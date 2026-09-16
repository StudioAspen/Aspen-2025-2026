using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutlineMaterialApplier : EditorWindow
{
    private Material outlineMaterial;

    [MenuItem("Tools/Outline/Give Every Model An Outline")]
    public static void ShowWindow()
    {
        GetWindow<OutlineMaterialApplier>("Outline Applier");
    }

    private void OnGUI()
    {
        GUILayout.Label("Apply Outline Material", EditorStyles.boldLabel);
        
        outlineMaterial = (Material)EditorGUILayout.ObjectField("Outline Material", outlineMaterial, typeof(Material), false);

        if (GUILayout.Button("Apply to all MeshRenderers in Scene"))
        {
            if (outlineMaterial == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign an Outline Material first.", "OK");
                return;
            }

            ApplyOutline();
        }
    }

    private void ApplyOutline()
    {
        MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        int modifiedCount = 0;

        Undo.RecordObjects(renderers, "Apply Outline Material");

        foreach (var renderer in renderers)
        {
            Material[] sharedMaterials = renderer.sharedMaterials;

            // Check if it has at least one material
            if (sharedMaterials.Length == 0) continue;

            // If it only has element 0
            if (sharedMaterials.Length == 1)
            {
                Material[] newMaterials = new Material[2];
                newMaterials[0] = sharedMaterials[0];
                newMaterials[1] = outlineMaterial;
                renderer.sharedMaterials = newMaterials;
                modifiedCount++;
            }
            // If it has element 1 but it's null
            else if (sharedMaterials.Length >= 2 && sharedMaterials[1] == null)
            {
                sharedMaterials[1] = outlineMaterial;
                renderer.sharedMaterials = sharedMaterials;
                modifiedCount++;
            }
            // If element 1 is already assigned, we skip as per instructions
        }

        Debug.Log($"Outline Material applied to {modifiedCount} MeshRenderers.");
        EditorUtility.DisplayDialog("Success", $"Applied outline material to {modifiedCount} objects.", "OK");
    }
}
