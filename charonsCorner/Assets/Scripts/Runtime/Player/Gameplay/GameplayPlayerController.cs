using System;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class GameplayPlayerController : MonoBehaviour
    {
        [SerializeField] private float _gravityAmount = 30f;

        [Header("Ground Check")]
        [SerializeField] private float _groundCheckLength = 0.5f;

        [SerializeField] private LayerMask _groundLayer;
        
        [field: SerializeField] public Transform Orientation { get; private set; }
        
        [field: SerializeField] public CinemachineCamera PlayerCamera { get; private set; }


        public Rigidbody Rb { get; private set; }
        public SphereCollider Collider { get; private set; }
        public SlopeSensor SlopeSensor { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool CannonAir { get; private set; } = false;

        #region StateMachine Vars
        public StateMachine<GameplayPlayerController> StateMachine { get; private set; }
        
        [field: Header("State Machine")]
        [field: SerializeField] public GroundSuperState GroundState { get; private set; } = new();
        [field: SerializeField] public AirSuperState AirState { get; private set; } = new();
        [field: SerializeField] public CannonBallSuperState CannonState { get; private set; } = new();

        [field: SerializeField] public GroundSuperState GroundSuperState { get; private set; } = new();
        [field: SerializeField] public AirSuperState AirSuperState { get; private set; } = new();
        [field: SerializeField] public CannonBallSuperState CannonBallSuperState { get; private set; } = new();


        [field: SerializeField] public DriftSuperState DriftSuperState { get; private set; } = new();

        
        [HideInInspector] public String CurrentSubState;
        #endregion

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Collider = GetComponent<SphereCollider>();
            SlopeSensor = GetComponentInChildren<SlopeSensor>();
            SetupStateMachine();
        }

        private void Update()
        {
            StateMachine.Update();
        }
        
        private void FixedUpdate()
        {
            CheckGrounded();
            StateMachine.FixedUpdate();
        }

        public void ApplyGravity()
        {
            Rb.AddForce(Vector3.down * _gravityAmount, ForceMode.Acceleration);
        }
        
        private void CheckGrounded()
        {
            IsGrounded = Physics.CheckSphere(transform.position + Vector3.down * _groundCheckLength, Collider.radius * 0.9f, _groundLayer);
        }

        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<GameplayPlayerController>(this);

            GroundState.Init(StateMachine, this);
            AirState.Init(StateMachine, this);
            CannonState.Init(StateMachine, this);

            StateMachine.ChangeState(GroundState, true);
            GroundSuperState.Init(StateMachine, this);
            AirSuperState.Init(StateMachine, this);
            DriftSuperState.Init(StateMachine, this);
            
            StateMachine.ChangeState(GroundSuperState, true);
        }

        public void SetCannonAir(bool t)
        {
            CannonAir = t;
        }

        private void OnDrawGizmos()
        {
        #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position - Vector3.up * _groundCheckLength, Collider.radius * 0.9f);
                
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.red;
                style.fontSize = 40;
                Handles.Label(transform.position + Vector3.up, 
                    StateMachine.CurrentState.GetType().Name + ">" + CurrentSubState, style);
            }
        #endif
        }
    }
}