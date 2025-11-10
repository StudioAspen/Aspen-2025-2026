using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;
//using static ak.wwise.core;

public class LightPath : MonoBehaviour
{

    /// <summary>
    /// This script is attached to a LightSphere object. This script moves the light sphere along a spline as long as the player is near the LightSphere.
    /// The path needs to have the 'ToggleTransparencyLightPath' material added, which will render the 
    ///  material invisible until the light's radius is updated. 
    ///  You will need to create one spline, which is the ground path.
    ///  Make sure to select the spline path as the spline container in the inspector under this script. 
    ///  Additional note. On the LightSphere object, we have one child object. Global volume bloom, providing the glowing effect.
    /// </summary>

    [Header("Light Sphere Variables")]
    public Transform player; //needs to be player not playerwithcamera
    [SerializeField] private float MoveDistance = 10f;
    [SerializeField] private float speed = 10f;
    public SplineContainer SplinePath;
    private float progress = 0f;
    
    [Header("Shader Variables")]
    [SerializeField] private Material revealMaterial; // Assign the material using the shader
    [SerializeField] private Transform revealField; // The object that reveals the target
    [SerializeField] private float revealRadius = 1.0f; // Radius of the reveal effect
    [SerializeField] private float fadeWidth = 0.5f; // Width of the fade transition
    [SerializeField] private bool proximityEnabled = true; // Toggle proximity effect

    private void Update()
    {

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= MoveDistance) //The light ball will only move if the player is near it. We can adjust how close the player needs to be with the MoveDistance variable. 
        {
            progress += speed * Time.deltaTime / SplinePath.CalculateLength();
            progress %= 1f;

            // Get position and tangent from the spline
            Vector3 position = SplinePath.EvaluatePosition(progress);
            Vector3 tangent = SplinePath.EvaluateTangent(progress);

            // Update object's position and rotation
            transform.position = position;
            transform.rotation = Quaternion.LookRotation(tangent);
            //transform.position = Vector3.MoveTowards(transform.position, Target_Position, speed * Time.deltaTime);
        }

        //Shader
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


