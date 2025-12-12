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


    void OnEnable()
    {
        Offset = transform.position - _Parent.position;

    }

    // Update is called once per frame
    void Update()
    {


        if (_followDirectionOfParent && _ParentRigidbody.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.position = _Parent.position;
            transform.forward = _ParentRigidbody.linearVelocity.normalized;
        }
        else
        {
            transform.position = _Parent.position + Offset;
        }


    }
}

}

