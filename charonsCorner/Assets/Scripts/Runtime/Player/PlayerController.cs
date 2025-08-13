using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerController : MonoBehaviour
    {
        public Rigidbody RigidBody { get; private set; }
        public SphereCollider SphereCollider { get; private set; }

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private float groundCheckRadius = 0.9f;
        [SerializeField] private LayerMask groundLayerMask;
        [field: SerializeField, ReadOnly] public bool IsGrounded { get; private set; }

        [Header("State Machine")]
        [SerializeReference] private StateMachine<PlayerController> stateMachine;
        [field: SerializeField] public IdleState IdleState { get; private set; }
        [field: SerializeField] public RollState RollState { get; private set; }
        [field: SerializeField] public JumpState JumpState { get; private set; }

        public float CurrentSpeed => RigidBody.linearVelocity.magnitude;

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            SetupStateMachine();
        }

        private void OnDrawGizmos()
        {
            // Draws the ground check sphere in the editor for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + groundCheckDistance * Vector3.down, groundCheckRadius);
        }

        private void Update()
        {
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            stateMachine.FixedUpdate();
        }

        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        private void SetupStateMachine()
        {
            stateMachine = new StateMachine<PlayerController>(this);

            IdleState.Initialize(stateMachine, this);
            RollState.Initialize(stateMachine, this);
            JumpState.Initialize(stateMachine, this);

            stateMachine.ChangeState(IdleState, true);
        }

        private void CheckGrounded()
        {
            IsGrounded = Physics.CheckSphere(transform.position + groundCheckDistance * Vector3.down, groundCheckRadius, groundLayerMask);
        }
    }
}
