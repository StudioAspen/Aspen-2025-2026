using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class MoveObject_E : MonoBehaviour
    {
        [SerializeField] GameObject TargetObject;

        [Header("Spawned Object Settings")]
        [Tooltip("Where object moves to")]
        [SerializeField] Transform TargetPosition;
        [Tooltip("Speed of object")]
        [SerializeField] float ObjectSpeed = 25f;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //TargetObject[i].transform.position = Vector3.MoveTowards(transform.position, new Vector3(TargetObject[i].transform.position.x, Target_Y_Value, TargetObject[i].transform.position.z), Speed * Time.deltaTime);
                TargetObject.GetComponent<TrackTarget_E>().enabled = true;
                //Give script the target position to move towards (the player)
                TargetObject.GetComponent<TrackTarget_E>().TargetObject = TargetPosition;
                //Set speed of object
                TargetObject.GetComponent<TrackTarget_E>().Speed = ObjectSpeed;
            }
        }
    }
}
