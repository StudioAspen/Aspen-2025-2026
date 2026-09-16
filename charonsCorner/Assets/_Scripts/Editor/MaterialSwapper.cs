using UnityEditor;
using UnityEngine;

public class MaterialSwapper : EditorWindow
{
    private Material sourceMaterial;
    private Material targetMaterial;

    [MenuItem("Tools/Materials/Swap Materials")]
    public static void ShowWindow()
    {
        GetWindow<MaterialSwapper>("Material Swapper");
    }

    private void OnGUI()
    {
        GUILayout.Label("Swap Materials in Scene", EditorStyles.boldLabel);

        sourceMaterial = (Material)EditorGUILayout.ObjectField("Material to Replace (X)", sourceMaterial, typeof(Material), false);
        targetMaterial = (Material)EditorGUILayout.ObjectField("Replacement Material (Y)", targetMaterial, typeof(Material), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Swap Materials"))
        {
            if (sourceMaterial == null || targetMaterial == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Source and Target materials.", "OK");
                return;
            }

            SwapMaterials();
        }
    }

    private void SwapMaterials()
    {
        Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int modifiedCount = 0;
        int totalReplacements = 0;

        Undo.RecordObjects(renderers, "Swap Materials");

        foreach (var renderer in renderers)
        {
            Material[] sharedMaterials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                if (sharedMaterials[i] == sourceMaterial)
                {
                    sharedMaterials[i] = targetMaterial;
                    changed = true;
                    totalReplacements++;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = sharedMaterials;
                modifiedCount++;
            }
        }

        Debug.Log($"Swapped {totalReplacements} material instances on {modifiedCount} objects.");
        EditorUtility.DisplayDialog("Success", $"Swapped {totalReplacements} material instances on {modifiedCount} objects.", "OK");
    }
}
