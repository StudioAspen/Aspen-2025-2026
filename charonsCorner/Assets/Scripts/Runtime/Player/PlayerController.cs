using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerController : MonoBehaviour
    {
        public InputManager Input { get; private set; } // For quick access
        [field: SerializeField] public CharacterController Controller { get; private set; }
        [field: SerializeField] public GameObject VisualObject { get; private set; }

        public StateMachine<PlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

        [field: Header("Config")]
        [field: SerializeField] public float StrictSpeedCap { get; private set; } = 100f;
        
        private Vector3 _velocity;
        [ShowNativeProperty] public Vector3 Velocity => _velocity;
        public Vector3 PlanarVelocity => _velocity.WithY(0f);
        public float CurrentSpeed => _velocity.magnitude;
        /// <summary>
        /// Easy access to whether the player is grounded or not from the context. GroundedSuperState handles the actual checking.
        /// </summary>
        [ShowNativeProperty] public bool IsGrounded => GroundedSuperState.IsGrounded;

        private void Awake()
        {
            Input = InputManager.Instance;

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
        private void InitializeStates(StateMachine<PlayerController> machine, PlayerController context)
        {
            GroundedSuperState.Init(machine, context);
            AirborneSuperState.Init(machine, context);

            machine.ChangeState(GroundedSuperState, context);
        }

        public void SetVelocity(Vector3 newVelocity) => _velocity = newVelocity;
        
        public void SetPlanarVelocity(Vector3 newVelocity) => _velocity = newVelocity.WithY(_velocity.y);
        
        public void ApplyPlanarVelocity() => Controller.Move(Time.deltaTime * PlanarVelocity);

        public void ApplyGravity()
        {
            if(!IsGrounded)
                _velocity += Time.deltaTime * Physics.gravity;
            Controller.Move(_velocity.y * Time.deltaTime * Vector3.up);
        }
    }
}