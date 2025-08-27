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
        [field: SerializeField] public PlayerControllerConfigSO Config { get; private set; }

        /// <summary>
        /// Easy access to whether the player is grounded or not from the context. GroundedSuperState handles the actual checking.
        /// </summary>
        [ShowNativeProperty] public bool IsGrounded => Config.GroundedSuperState.IsGrounded;
        [ShowNativeProperty] public float CurrentSpeed => RigidBody.linearVelocity.magnitude;

        private void Awake()
        {
            Input = InputManager.Instance;
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            StateMachine = new StateMachine<PlayerController>(this);
            Config.InitializeStates(StateMachine, this);
        }

        private void OnDestroy()
        {
            StateMachine.Destroy();
        }

        private void OnDrawGizmos()
        {
            // Draws the ground check sphere in the editor for visualization
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Config.GroundedSuperState.GroundCheckDistance * Vector3.down, Config.GroundedSuperState.GroundCheckRadius);
        }

        private void Update()
        {
            StateMachine.Update();
        }

        private void FixedUpdate()
        {
            Config.GroundedSuperState.CheckGrounded();

            StateMachine.FixedUpdate();
        }
    }
}
