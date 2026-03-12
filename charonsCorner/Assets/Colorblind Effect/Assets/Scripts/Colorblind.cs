// Copyright (c) 2016 Jakub Boksansky, Adam Pospisil - All Rights Reserved
// Colorblind Effect Unity Plugin 1.0
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wilberforce
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
	[HelpURL("https://projectwilberforce.github.io/colorblind/")]
    [AddComponentMenu("Image Effects/Color Adjustments/Colorblind")]
    public class Colorblind : MonoBehaviour
    {
        // public Parameters  
		public int Type = 0;

        // private Parameters
		public Shader colorblindShader;
        private bool isSupported;
        private Material ColorblindMaterial;

		// method for logging if something goes wrong
        private void ReportError(string error)
        {
            if (Debug.isDebugBuild) Debug.Log("Colorblind Effect Error: " + error);
        }

		// initialization method
        void Start()
        {
			// if shader is not set, try to find it first
            if (colorblindShader == null) colorblindShader = Shader.Find("Hidden/Wilberforce/Colorblind");

			// shader wasn't found 
            if (colorblindShader == null)
            {
                ReportError("Could not locate Colorblind Shader. Make sure there is 'Colorblind.shader' file added to the project.");
                isSupported = false;
                enabled = false;
                return;
            }

			// check if image effect are supported on current setup
            if (!SystemInfo.supportsImageEffects || SystemInfo.graphicsShaderLevel < 30)
            {
                if (!SystemInfo.supportsImageEffects) ReportError("System does not support image effects.");
                if (SystemInfo.graphicsShaderLevel < 30) ReportError("This effect needs at least Shader Model 3.0.");

                isSupported = false;
                enabled = false;
                return;
            }

			// initialize the material responsible for this image effect
            EnsureMaterials();

			// check if the material was set properly
            if (!ColorblindMaterial || ColorblindMaterial.passCount != 1)
            {
                ReportError("Could not create shader.");
                isSupported = false;
                enabled = false;
                return;
            }

            isSupported = true;
        }

        private static Material CreateMaterial(Shader shader)
        {
            if (!shader) return null;
            Material m = new Material(shader);
            m.hideFlags = HideFlags.HideAndDontSave;
            return m;
        }

        private static void DestroyMaterial(Material mat)
        {
            if (mat)
            {
                DestroyImmediate(mat);
                mat = null;
            }
        }

        private void EnsureMaterials()
        {
            if (!ColorblindMaterial && colorblindShader.isSupported)
            {
				// create the material object around the shader program
                ColorblindMaterial = CreateMaterial(colorblindShader);
            }

            if (!colorblindShader.isSupported)
            {
                ReportError("Could not create shader (Shader not supported).");
            }
        }
			
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!isSupported || !colorblindShader.isSupported)
            {	
				// uncheck the component if not supported
                enabled = false;
                return;
            }

            EnsureMaterials();

            // Shader pass
			// bind the 'Type' attribute to 'type' variable in shader program
			ColorblindMaterial.SetInt ("type", Type);
			// run the shader
			Graphics.Blit (
				source, // input texture
				destination, // output texture
				ColorblindMaterial, // which shader to use
				0 // which shader pass 
			);
        }
    }

	// ensure unity editor is present - so it doesn't crash when running built project
	#if UNITY_EDITOR 

	// custom gui for inspector
	[CustomEditor(typeof(Colorblind))]
	public class ColorblindEditor : Editor
	{	
		// names appearing in the dropdown menu
		private readonly GUIContent[] typeTexts = new GUIContent[4] {
			new GUIContent("Normal Vision"),
			new GUIContent("Protanopia"),
			new GUIContent("Deuteranopia"),
			new GUIContent("Tritanopia")
		};
		// label and tooltip for the dropdown menu
		private readonly GUIContent typeLabelContent = new GUIContent("Type:", "Type of color blindness");

		// numbers passed to shader - indices of color-shifting matrices
		private readonly int[] typeInts = new int[4] { 0, 1, 2, 3 };

		// this method contains the custom gui for editor
		override public void OnInspectorGUI()
		{
			// bind the connected script to local variable
			var colorblindScript = target as Colorblind;

            // bind the 'Type' parameter of the Colorblind script to dropdown in GUI
            colorblindScript.Type = EditorGUILayout.IntPopup(typeLabelContent, colorblindScript.Type, typeTexts, typeInts);

			// if user made some changes (selected new value from the dropdown) we have to forward the notification
			if (GUI.changed)
			{
				// mark as dirty
				EditorUtility.SetDirty(target);
			}
		}
	}
	#endif
}