using Unity.Mathematics;
using UnityEngine;
namespace CharonsCorner.Runtime
{
    public class particle_follow_player : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Transform _Parent;
    [SerializeField] private Rigidbody _ParentRigidbody;
   
   
    // [Header("offsets")]
    [SerializeField] private bool _followDirectionOfParent;
    private Vector3 Offset;

    
    void Start()
    {
        Offset = transform.localPosition - _Parent.localPosition;
    }

    // Update is called once per frame
    void Update()
    {

        // not sure what this offset does exactly so ill leave alone
        transform.localPosition = _Parent.localPosition + Offset;

        if (_followDirectionOfParent && _ParentRigidbody.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.forward = _ParentRigidbody.linearVelocity.normalized;
        }


    }
}

}

