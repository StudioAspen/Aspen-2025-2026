using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    public class SlopeSensor : MonoBehaviour
    {
        [field:SerializeField] public float RayLength{get; private set;}
        [field:SerializeField] public float MaxSlopeAngle {get; private set;} = 60f; // Maximum angle for a surface to be considered a slope, if a surface has a greater angle than this number then it will not be walkable
        [field:SerializeField] public float MinSlopeAngle {get; private set;} = 10f; // Minimum angle for a surface to be considered a slope, if a surface has an angle smaller than this number, then it will be considered regular ground
        [field:ReadOnly, SerializeField] public float CurrentSlopeAngle {get; private set;}
        [field:SerializeField] public SphereCollider PlayerSphereCollider { get; private set; }
        [field:SerializeField] public LayerMask GroundLayer { get; private set; }
        [field:SerializeField] public bool IsOnSlope { get; private set; }
        [field:SerializeField] public RaycastHit Hit { get; private set; }
        
        private void FixedUpdate()
        {
            CheckSlope();
        }

        private void CheckSlope()
        {
            bool raycast = Physics.SphereCast(new Ray(transform.position, Vector3.down), PlayerSphereCollider.radius * 0.9f, out RaycastHit tempHit, RayLength, GroundLayer);
            Hit = tempHit;
            CurrentSlopeAngle = Vector3.Angle(Hit.normal, Vector3.up);
            // Debug.Log("SLOPE: " + raycast + ", " + (CurrentSlopeAngle < MaxSlopeAngle) + ", " + (CurrentSlopeAngle > MinSlopeAngle));
            IsOnSlope = raycast && CurrentSlopeAngle < MaxSlopeAngle && CurrentSlopeAngle > MinSlopeAngle;
        }
    
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsOnSlope ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position+Vector3.down*RayLength, PlayerSphereCollider.radius * 0.9f);
        }
    }
}
