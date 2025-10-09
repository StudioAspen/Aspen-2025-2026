using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class PrototypePlayerController : MonoBehaviour
    {
        [field: SerializeField] public float GravityAmount {get; private set;}

        [Header("Ground Check")]
        [field: SerializeField]
        public float GroundCheckLength { get; private set; } = 1.1f;
        [field: SerializeField] public LayerMask GroundLayer {get; private set;}
        
        
        [field:SerializeField] public Transform Orientation { get; private set; }

        public Rigidbody Rb { get; private set; }
        public SphereCollider Collider { get; private set; }
        public SlopeSensor SlopeSensor { get; private set; }
        public bool Grounded { get; private set; }
        
        [field: Header("State Machine")]
        [field: SerializeField] public StateMachine<PrototypePlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundSuperState GroundState { get; private set; } = new();
        [field: SerializeField] public AirSuperState AirState { get; private set; } = new();


        [NonSerialized] public String CurrentSubState;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Collider = GetComponent<SphereCollider>();
            SlopeSensor = GetComponentInChildren<SlopeSensor>();
            SetupStateMachine();
        }

        private void Update()
        {
            HandleTransitions();
            StateMachine.Update();
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(2);
            }
            
        }
        
        private void FixedUpdate()
        {
            ApplyGravity();
            CheckGrounded();
            StateMachine.FixedUpdate();
        }

        private void ApplyGravity()
        {
            Rb.AddForce(Vector3.down * GravityAmount, ForceMode.Acceleration);
        }

        private void HandleTransitions()
        {
            if (Grounded)
            {
                StateMachine.ChangeState(GroundState);
            }
            else
            {
                StateMachine.ChangeState(AirState);
            }
        }


        private void CheckGrounded()
        {
            Grounded = Physics.CheckSphere(transform.position + Vector3.down * GroundCheckLength, Collider.radius * 0.9f, GroundLayer);
        }

        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<PrototypePlayerController>(this);

            GroundState.Init(StateMachine, this);
            AirState.Init(StateMachine, this);
            
            StateMachine.ChangeState(GroundState, true);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Grounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position - Vector3.up * GroundCheckLength, Collider.radius * 0.9f);
                
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.red;
                style.fontSize = 40;
                Handles.Label(transform.position + Vector3.up, 
                    StateMachine.CurrentState.GetType().Name + ">" + CurrentSubState, style);
            }
        }
    }
}