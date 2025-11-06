using UnityEngine;
using DG.Tweening;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerController : MonoBehaviour
    {
        public InputManager Input { get; private set; }
        public Rigidbody RigidBody { get; private set; }
        public SphereCollider SphereCollider { get; private set; }

        [field: Header("State Machine")]
        [field: SerializeField] public StateMachine<PlayerController> StateMachine { get; private set; }
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

        public bool IsGrounded => GroundedSuperState.IsGrounded;
        public float CurrentSpeed => RigidBody.linearVelocity.magnitude;
        public bool IsInWater { get; set; }

        private void Awake()
        {
            Input = InputManager.Instance;
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            SetupStateMachine();
        }

        private void OnDestroy()
        {
            StateMachine.Destroy();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + GroundedSuperState.BaseGroundCheckDistance * Vector3.down, GroundedSuperState.BaseGroundCheckRadius);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + transform.right * 2f);

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

        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<PlayerController>(this);

            GroundedSuperState.Init(StateMachine, this);
            AirborneSuperState.Init(StateMachine, this);

            StateMachine.ChangeState(GroundedSuperState, true);
        }
    }
}