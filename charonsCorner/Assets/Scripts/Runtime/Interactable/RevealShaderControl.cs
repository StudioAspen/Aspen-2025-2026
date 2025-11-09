using UnityEngine;

namespace CharonsCorner.Runtime

/// <summary>
/// This script is referenced in the LightSphere object. This script is attached to the 'ToggleTransparencyLightPath' shader material and needs to be added on a empty gameobject that  is child of the light shphere and parents the sphere/cube that toggles the material's visibility. 
/// 
///  Note: The Light_movement script should be moved to Asset>Scripts>Runtime>Interactable. Right now the Spline package doesn't register in the subfolders, so it is here under Assets until resolved.
///  Additional note. On the LightSphere object, we have two child objects. One is a global volume bloom, providing the glowing effect, the other is RevelController which parents the sphere radius of material affect. 
/// </summary>

{
    public class RevealController : MonoBehaviour
    {
        [SerializeField] private Material revealMaterial; // Assign the material using the shader
        [SerializeField] private Transform revealField; // The object that reveals the target
        [SerializeField] private float revealRadius = 1.0f; // Radius of the reveal effect
        [SerializeField] private float fadeWidth = 0.5f; // Width of the fade transition
        [SerializeField] private bool proximityEnabled = true; // Toggle proximity effect

        private void Update()
        {
            if (revealMaterial != null)
            {
                // Update the proximity toggle
                revealMaterial.SetFloat("_ProximityEnabled", proximityEnabled ? 1.0f : 0.0f);

                if (proximityEnabled && revealField != null)
                {
                    // Update the reveal position
                    revealMaterial.SetVector("_RevealPosition", revealField.position);

                    // Update the reveal radius and fade width
                    revealMaterial.SetFloat("_RevealRadius", revealRadius);
                    revealMaterial.SetFloat("_FadeWidth", fadeWidth);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (proximityEnabled && revealField != null)
            {
                // Draw the reveal radius in the Scene view for debugging
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(revealField.position, revealRadius);
            }
        }
    }
}