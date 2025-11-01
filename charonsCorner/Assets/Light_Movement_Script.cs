using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Splines;

public class Light_Movement_Script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //This script is attached to a LightSphere object.
    //This script moves the light sphere along a spline as long as the player is near the LightSphere. 

    //Update Notes: 
    //Was able to update code so it follows a spline path. You will need to create one spline, which is the ground path.
    //Then you need to duplicate that spline, and offset it so it sits above the original spline by a bit.
    //This second duplicative spline path is what the light ball will follow.
    //Make sure to select the duplicated spline path as the spline container in the inspector under this script. 

    //Additional note. On the LightSphere object, we have two child objects. One is a global volume bloom, providing the glowing effect.
    //The second is a "visible ground" object, that shows a small semi-transparent oval underneath the LightSphere.
    //The oval was made by making a new material, "visible ground" and including a UIsprite in the occlusion map.
    //This works for now, but an updated oval texture would work better. 

    //public Vector3 Target_Position; No longer need. 
    public Transform player;
    private float MoveDistance = 5f;
    private float speed = 3f;
    //private Vector3 Start_Position; No longer need. 
    public SplineContainer SplinePath;
    private float progress = 0f;

    void Start()
    {

        
    }

    // Update is called once per frame
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
