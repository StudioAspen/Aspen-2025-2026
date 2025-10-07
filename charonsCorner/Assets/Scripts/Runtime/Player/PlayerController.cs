using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerController : MonoBehaviour
    {
        public InputManager Input { get; private set; } // For quick access
        public Rigidbody RigidBody { get; private set; }
        public SphereCollider SphereCollider { get; private set; }

        [field: Header("State Machine")]
        [field: SerializeField] public StateMachine<PlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

        /// <summary>
        /// Easy access to whether the player is grounded or not from the context. GroundedSuperState handles the actual checking.
        /// </summary>
        public bool IsGrounded => GroundedSuperState.IsGrounded;
        public float CurrentSpeed => RigidBody.linearVelocity.magnitude;

        private void Awake()
        {
            Input = InputManager.Instance;
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            SetupStateMachine();
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

        private void OnDestroy()
        {
            StateMachine.Destroy();
        }

        private void OnDrawGizmos()
        {
            // Draws the ground check sphere in the editor for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + GroundedSuperState.GroundCheckDistance * Vector3.down, GroundedSuperState.GroundCheckRadius);

           
            if (Application.isPlaying)
            {
                // Forward (Z axis)
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

                // Right (X axis)
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + transform.right * 2f);

                // Velocity direction (actual movement)
                if (RigidBody != null)
                {
                    Gizmos.color = Color.yellow;
                    Vector3 velocityDir = RigidBody.linearVelocity;
                    Gizmos.DrawLine(transform.position, transform.position + velocityDir.normalized * 2f);
                }
            }
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
    }
}
