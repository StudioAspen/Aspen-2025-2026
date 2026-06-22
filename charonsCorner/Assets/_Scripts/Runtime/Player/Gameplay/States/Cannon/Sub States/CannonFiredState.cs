using UnityEngine;
using System.Collections;

namespace CharonsCorner.Runtime
{
    public class CannonFiredState : State<GameplayPlayerController>
    {
        private float _launchTimer;
        private Vector3 _targetVelocity;
        public bool IsLaunching { get; private set; } = false;

        [Header("Cannon Launch Detection Settings")]
        [SerializeField] private float CollisionCheckRadius = 1;
        [SerializeField] private float CollisionCheckDelay = 0.1f;
        [SerializeField] private LayerMask _collisionLayers = ~0;
        private LayerMask _collisionMask;

        private protected override void OnEnter()
        {
            //Set Launching Flag:
            IsLaunching = true;

            //Set Current Substate Name:
            _context.CurrentSubState = GetType().Name;

            //Get Current Cannon Being Used By Player:
            CannonBall cannon = _context.CurrentCannon;
            if (cannon == null)
            {
                //If No Cannon Found, Return To Ground State:
                _context.CannonBallSuperState.LaunchFailed = true;
                return;
            }
            cannon.PlayLaunchEffect();

            //Setup Collision Mask to Ignore Current Cannon Layer:
            int cannonLayer = _context.CurrentCannon.gameObject.layer;
            _collisionMask = _collisionLayers & ~(1 << cannonLayer);

            //Set Rigidbody to Non-Kinematic and Reset Target Velocity:
            _context.Rb.isKinematic = false;
            _targetVelocity = Vector3.zero;

            //Reset Launch Completion Flag & Timer:
            _context.CannonBallSuperState.LaunchCompleted = false;
            _launchTimer = 0f;
        }

        private protected override void OnExit() 
        {
            //Reset Launching Flag and Collision Mask:
            IsLaunching = false;
            _collisionMask = 0;

            //Set Launch Completion Flag:
            _context.CannonBallSuperState.LaunchCompleted = true;
        }

        private void ApplyStraightLaunch(CannonBall cannon)
        {
            Transform launchObject = cannon.LaunchObject != null ? cannon.LaunchObject : cannon.transform;
            _targetVelocity = launchObject.forward * cannon.LaunchForce;
            _context.Rb.linearVelocity = _targetVelocity;
            _context.transform.rotation = Quaternion.LookRotation(launchObject.forward, Vector3.up);
            _context.CannonBallSuperState.LaunchCompleted = true;
        }

        private protected override void OnUpdate()
        {
            //Get Current Cannon:
            CannonBall cannon = _context.CurrentCannon;
            if (cannon == null)
            {
                _context.CannonBallSuperState.LaunchFailed = true;
                return;
            }

            ApplyStraightLaunch(cannon);
        }

        private protected override void OnFixedUpdate()
        {
            //Continue applying velocity if needed, though usually one-shot is enough for physics:
            if (_targetVelocity != Vector3.zero)
            {
                _context.Rb.linearVelocity = _targetVelocity;
            }
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            return null;
        }
    }
}
