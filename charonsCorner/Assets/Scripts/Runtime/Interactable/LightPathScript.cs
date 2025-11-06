using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;
//using static ak.wwise.core;

public class Light_Movement_Script : MonoBehaviour
{

    /// <summary>
    /// This script is attached to a LightSphere object. This script moves the light sphere along a spline as long as the player is near the LightSphere.
    /// The path needs to have the 'ToggleTransparencyLightPath' material added, which will render the 
    ///  material invisible until the light's radius (defined in child object script - RevealController.cs) is updated. 
    ///  You will need to create one spline, which is the ground path.
    ///  Then you need to duplicate that spline, and offset it so it sits above the original spline.
    ///  This second duplicative spline path is what the light ball will follow.
    ///  Make sure to select the duplicated spline path as the spline container in the inspector under this script. 
    ///  
    ///  Additional note. On the LightSphere object, we have two child objects. One is a global volume bloom, providing the glowing effect.
    /// </summary>


    public Transform player; //needs to be player not playerwithcamera
    [SerializeField] private float MoveDistance = 10f;
    [SerializeField] private float speed = 10f;
    public SplineContainer SplinePath;
    private float progress = 0f;


    void Update()
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



    }


}
