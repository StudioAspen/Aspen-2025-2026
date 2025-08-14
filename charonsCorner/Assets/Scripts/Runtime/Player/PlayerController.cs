using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerController : MonoBehaviour
    {
        public Rigidbody RigidBody { get; private set; }
        public SphereCollider SphereCollider { get; private set; }

        [field: Header("State Machine")]
        [field: SerializeField] public StateMachine<PlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

        public bool IsGrounded => GroundedSuperState.IsGrounded;
        public float CurrentSpeed => RigidBody.linearVelocity.magnitude;

        private void Awake()
        {
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            SetupStateMachine();
        }

        private void OnDestroy()
        {
            StateMachine.Dispose();
        }

        private void OnDrawGizmos()
        {
            // Draws the ground check sphere in the editor for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + GroundedSuperState.GroundCheckDistance * Vector3.down, GroundedSuperState.GroundCheckRadius);
        }

        private void Update()
        {
            StateMachine.Update();
        }

        private void FixedUpdate()
        {
            GroundedSuperState.CheckGrounded();

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
    }
}
