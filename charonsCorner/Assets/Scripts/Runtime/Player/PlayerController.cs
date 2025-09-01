using NaughtyAttributes;
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

        public StateMachine<PlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

        /// <summary>
        /// Easy access to whether the player is grounded or not from the context. GroundedSuperState handles the actual checking.
        /// </summary>
        [ShowNativeProperty] public bool IsGrounded => GroundedSuperState.IsGrounded;
        public float CurrentSpeed => RigidBody.linearVelocity.magnitude;

        private void Awake()
        {
            Input = InputManager.Instance;
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            StateMachine = new StateMachine<PlayerController>(this);
            InitializeStates(StateMachine, this);
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
        public void InitializeStates(StateMachine<PlayerController> machine, PlayerController context)
        {
            GroundedSuperState.Init(machine, context);
            AirborneSuperState.Init(machine, context);

            machine.ChangeState(GroundedSuperState, context);
        }
    }
}
