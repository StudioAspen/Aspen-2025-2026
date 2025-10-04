using System;
using UnityEngine;
using CharonsCorner.Runtime;
using NUnit.Framework;
using UnityEngine.SceneManagement;

namespace CharonsCorner.PrototypeC
{
    [RequireComponent(typeof(Rigidbody))]
    public class PrototypePlayerController : MonoBehaviour
    {
        [field: SerializeField] public float Speed {get; private set;}
        [field: SerializeField] public float GroundDrag {get; private set;}
        [field: SerializeField] public float GravityAmount {get; private set;}

        [Header("Ground Layer")]
        [field: SerializeField]
        public float GroundCheckLength { get; private set; } = 1.1f;
        [field: SerializeField] public LayerMask GroundLayer {get; private set;}
        
        [field:SerializeField] public Transform Orientation { get; private set; }

        public Rigidbody Rb { get; private set; }
        
        
        // [field: Header("State Machine")]
        // [field: SerializeField] public StateMachine<PrototypePlayerController> StateMachine { get; private set; }
        // [field: SerializeField] public GroundedSuperState Grounded { get; private set; } = new();
        // [field: SerializeField] public AirborneSuperState Airborne { get; private set; } = new();


        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(2);
            }
        }
        
        private void FixedUpdate()
        {
            Rb.AddForce(Vector3.down * GravityAmount, ForceMode.Acceleration);
            
            Rb.linearDamping = IsGrounded() ? GroundDrag : 0;
            
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Rb.AddForce(Orientation.forward * (input.y * Speed) + Orientation.right * (input.x * Speed), ForceMode.VelocityChange);
        }


        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, GroundCheckLength, GroundLayer);
        }
    }
}