using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This script is attached to a LightSphere object. 
    /// The path needs to have the 'ToggleTransparencyLightPath' material added, which will render the 
    /// material invisible until the light's radius is updated. 
    /// You will need to create one spline, which is the ground path.
    /// Make sure to select the spline path as the spline container in the inspector under this script. 
    /// Additional note. On the LightSphere object, we have one child object. Global volume bloom, providing the glowing effect.
    /// </summary>
    public class LightPath : MonoBehaviour
    {

        [Header("Light Sphere Variables")]
        [field: SerializeField] public Transform Player { get; private set; }
        [field: SerializeField] public SplineContainer SplinePath { get; private set; }
        [field: SerializeField] public float MoveDistance { get; private set; } = 10f;
        [field: SerializeField] public float Speed { get; private set; } = 10f;
        
        [Header("Shader Variables")]
        [SerializeField] private Material _revealMaterial; 
        [SerializeField] private Transform _revealField; 
        [SerializeField] private float _revealRadius = 5.0f; 
        [SerializeField] private float _fadeWidth = 0.5f; 
        [SerializeField] private bool _proximityEnabled = true;
        private float _progress = 0f;
        private void Update()
        {

            float distance = Vector3.Distance(transform.position, Player.position);

            if (distance <= MoveDistance) // Check if player is within move distance
            {
                _progress += Speed * Time.deltaTime / SplinePath.CalculateLength();
                _progress %= 1f;

               
                Vector3 position = SplinePath.EvaluatePosition(_progress); // Get position and tangent from the spline
                Vector3 tangent = SplinePath.EvaluateTangent(_progress);

                transform.position = position;   // Update object's position and rotation
                transform.rotation = Quaternion.LookRotation(tangent);
                
            }

            
            if (_revealMaterial != null) 
            {
                _revealMaterial.SetFloat("_ProximityEnabled", _proximityEnabled ? 1.0f : 0.0f); // Update the proximity toggle

                if (_proximityEnabled && _revealField != null)
                { 
               
                    _revealMaterial.SetVector("_RevealPosition", _revealField.position);  // Update the reveal position

                   
                    _revealMaterial.SetFloat("_RevealRadius", _revealRadius);  // Update the reveal radius and fade width
                    _revealMaterial.SetFloat("_FadeWidth", _fadeWidth);
                }
            }
        }

        private void OnDrawGizmos() // Draw the reveal radius in the Scene view for debugging
        {
            if (_proximityEnabled && _revealField != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_revealField.position, _revealRadius);
            }
        }
    }
}


