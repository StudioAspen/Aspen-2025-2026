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
        public bool IsBig => isBig;

        public event System.Action<bool> OnBigStateChanged;

        // Size Toggle Fields
        [Header("Size Settings")]
        public Vector3 normalScale = Vector3.one;
        public Vector3 bigScale = new Vector3(2f, 2f, 2f);
        public float normalMass = 1f;
        public float bigMass = 4f;
        public float normalGravity = -9.81f;
        public float bigGravity = -19.62f; // Example: double gravity when big
        public bool isBig = false;

        private void Awake()
        {
            Input = InputManager.Instance;
            RigidBody = GetComponent<Rigidbody>();
            SphereCollider = GetComponent<SphereCollider>();

            SetupStateMachine();

            // Ensure starting size is normal
            SetSize(false);
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

            // Size Toggle Logic
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                isBig = !isBig;
                SetSize(isBig);
            }
        }

        private void FixedUpdate()
        {
            GroundedSuperState.CheckGrounded();
            StateMachine.FixedUpdate();

            // Apply custom gravity
            float gravity = isBig ? bigGravity : normalGravity;
            RigidBody.AddForce(Vector3.up * gravity * RigidBody.mass, ForceMode.Force);
        }

        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<PlayerController>(this);

            GroundedSuperState.Init(StateMachine, this);
            AirborneSuperState.Init(StateMachine, this);

            StateMachine.ChangeState(GroundedSuperState, true);
        }

        // Helper to Set Size
        private void SetSize(bool big)
        {
            Vector3 targetScale = big ? bigScale : normalScale;
            float targetMass = big ? bigMass : normalMass;
            float targetGravity = big ? bigGravity : normalGravity;
            float duration = 0.3f;

            // Smoothly animate the scale change
            transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);

            // Set Rigidbody mass
            RigidBody.mass = targetMass;
            OnBigStateChanged?.Invoke(big);
        }
    }
}