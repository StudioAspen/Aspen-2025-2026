using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TrackTarget_E : MonoBehaviour
    {
        [HideInInspector] public Transform TargetObject;
        [SerializeField] float Speed = 10f;

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, TargetObject.position, Speed * Time.deltaTime);
        }
    }
}
