using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TrackTarget_E : MonoBehaviour
    {
        [HideInInspector] public Transform TargetObject;
        [HideInInspector] public float Speed = 25f;

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, TargetObject.position, Speed * Time.deltaTime);
        }
    }
}
