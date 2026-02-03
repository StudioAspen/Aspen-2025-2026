using System.Linq;
using UnityEditor;
using UnityEngine;

public class MaterialsOneOffs
{
    [MenuItem("One-Offs/Fix Broken Look Dev Materials")]
    public static void FixBrokenMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("Material");
        Material[] materials = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<Material>)
            .ToArray();

        Shader lookDevShader = Shader.Find("Shader Graphs/C_VLighting");
        
        foreach (Material material in materials)
        {
            if (material)
            {
                if (material.shader.name == "Hidden/InternalErrorShader")
                {
                    material.shader = lookDevShader;
                }
            }
        }

        AssetDatabase.Refresh();
    }
}
