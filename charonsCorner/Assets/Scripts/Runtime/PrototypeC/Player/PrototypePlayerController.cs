using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
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
        
        
        [field: Header("State Machine")]
        [field: SerializeField] public StateMachine<PrototypePlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundSuperState Grounded { get; private set; } = new();


        [NonSerialized] public String CurrentSubState;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            SetupStateMachine();
        }

        private void Update()
        {
            StateMachine.Update();
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(2);
            }
            
        }
        
        private void FixedUpdate()
        {
            StateMachine.FixedUpdate();
            ApplyGravity();
        }

        private void ApplyGravity()
        {
            Rb.AddForce(Vector3.down * GravityAmount, ForceMode.Acceleration);
        }


        public bool IsGrounded()
        {
            return Physics.Raycast(transform.position, Vector3.down, GroundCheckLength, GroundLayer);
        }
        
        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<PrototypePlayerController>(this);

            Grounded.Init(StateMachine, this);

            StateMachine.ChangeState(Grounded, true);
        }

        private void OnDrawGizmos()
        {
            Handles.color = Color.red;
            Handles.Label(transform.position + Vector3.up, 
                StateMachine.CurrentState + ">" + CurrentSubState);
        }
    }
}