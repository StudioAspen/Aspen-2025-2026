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

        [field: Header("State Machine")]
        [field: SerializeReference] public StateMachine<PlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

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
            StateMachine.Update();
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            StateMachine.FixedUpdate();
        }

        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<PlayerController>(this);

            GroundedSuperState.Init(StateMachine, this);
            AirborneSuperState.Init(StateMachine, this);

            StateMachine.ChangeState(GroundedSuperState, true);
        }

        private void CheckGrounded()
        {
            bool isGrounded = Physics.CheckSphere(transform.position + groundCheckDistance * Vector3.down, groundCheckRadius, groundLayerMask);
            if(isGrounded != IsGrounded)
            {
                IsGrounded = isGrounded;
                StateMachine.ChangeState(IsGrounded ? GroundedSuperState : AirborneSuperState);
            }
        }
    }
}
