using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine.Animations;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class GameplayPlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SpawnPointManager _spawnPointManager;

        [Header("Ground Check")]
        [SerializeField] private float _groundCheckLength = 0.5f;
        [SerializeField] private LayerMask _groundLayer;
        [field: SerializeField] public float Gravity { get; private set; } = 40f;
        
        [Header("Drift Settings")]
        [SerializeField] private bool _canDrift = true;
        [SerializeField] private float _driftCooldownTime = 5f;
        private Coroutine _driftCooldownRoutine;

        [Header("References")]
        [field: SerializeField] public Transform Orientation { get; private set; }
        [field: SerializeField] public CinemachineCamera PlayerCamera { get; private set; }
        [field: SerializeField] public FollowTarget CameraTargetFollowTarget { get; private set; }
        [field: SerializeField] public MMFeedbacks DriftFeedbacks { get; private set; }

        public Rigidbody Rb { get; private set; }
        public SphereCollider Collider { get; private set; }
        public SlopeSensor SlopeSensor { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool CannonAir { get; private set; } = false;
        public CannonBall CurrentCannon { get; private set; }
        public bool JustLanded { get; set; }
        private bool _wasGrounded { get; set; }

        #region StateMachine Vars
        public StateMachine<GameplayPlayerController> StateMachine { get; private set; }
        
        [field: Header("State Machine")]
        [field: SerializeField] public GroundSuperState GroundSuperState { get; private set; } = new();
        [field: SerializeField] public AirSuperState AirSuperState { get; private set; } = new();
        [field: SerializeField] public CannonBallSuperState CannonBallSuperState { get; private set; } = new();
        [field: SerializeField] public DriftSuperState DriftSuperState { get; private set; } = new();
        
        [ReadOnly] public String CurrentSubState;
        #endregion

        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            Collider = GetComponent<SphereCollider>();
            SlopeSensor = GetComponentInChildren<SlopeSensor>();
            DriftFeedbacks?.Initialization();
            SetupStateMachine();
        }

        private void Start()
        {
            if(_spawnPointManager)
                _spawnPointManager.OnRespawn += Respawn;
        }

        private void OnDestroy()
        {
            if(_spawnPointManager)
                _spawnPointManager.OnRespawn -= Respawn;
        }

        private void Update()
        {
            StateMachine.Update();

            if (!_canDrift && _driftCooldownRoutine == null)
            {
                _driftCooldownRoutine = StartCoroutine(DriftCooldown());
            }
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            StateMachine.FixedUpdate();
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Drift += Drift;
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
                InputManager.Instance.Drift -= Drift;
        }

        public void ApplyGravity()
        {
            Rb.AddForce(Vector3.down * Gravity, ForceMode.Acceleration);
        }

        private Collider[] _overlapResults = new Collider[10]; // Reusable buffer
        public bool CheckOverlap(LayerMask layerMask, float sizeScale, out Collider hitCollider)
        {
            //Reset Out Parameter:
            hitCollider = null;
            if (Collider == null) return false;

            Vector3 center = Collider.bounds.center;
            float radius = Collider.radius * sizeScale;

            //Check Overlap Sphere:
            int hitCount = Physics.OverlapSphereNonAlloc(center, radius, _overlapResults, layerMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hitCount; i++)
            {
                if (_overlapResults[i] == Collider) continue;
                hitCollider = _overlapResults[i];
                return true;
            }

            return false;
        }

        private void CheckGrounded()
        {
            bool grounded = Physics.CheckSphere(transform.position + Vector3.down * _groundCheckLength, Collider.radius * 0.9f, _groundLayer);

            JustLanded = !_wasGrounded && grounded; // true if we were not grounded last frame but are grounded now

            _wasGrounded = grounded;
            IsGrounded = grounded;
        }

        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        private void SetupStateMachine()
        {
            StateMachine = new StateMachine<GameplayPlayerController>(this);
           
            GroundSuperState.Init(StateMachine, this);

            StateMachine.ChangeState(GroundSuperState, true);
            AirSuperState.Init(StateMachine, this);
            CannonBallSuperState.Init(StateMachine, this);  
            DriftSuperState.Init(StateMachine, this);
                    }

        public void SetCurrentCannon(CannonBall cannon)
        {
            CurrentCannon = cannon;
        }   
        public void SetCannonAir(bool t)
        {
            CannonAir = t;
        }

        private void Respawn(Vector3 position)
        {
            transform.position = position;
            Rb.linearVelocity = Vector3.zero;
        }

        private void Drift(bool drift)
        {
            // If releasing dont do anything
            if (!drift)
                return;

            if (!_canDrift)
                return;
            
            if (_driftCooldownRoutine != null)
                return;

            StateMachine.ChangeState(DriftSuperState);
            
            _canDrift = false;
            _driftCooldownRoutine = StartCoroutine(DriftCooldown());
        }

        private IEnumerator DriftCooldown()
        {
            yield return new WaitForSeconds(_driftCooldownTime);
            _canDrift = true;
            _driftCooldownRoutine = null;
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