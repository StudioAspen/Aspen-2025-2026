using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class CannonFiredState : State<GameplayPlayerController>
    {
        private float _launchTimer;
        private Vector3 _targetVelocity;
        public bool IsLaunching { get; private set; } = false;

        [Header("Cannon Launch Detection Settings")]
        [SerializeField] private const float CollisionCheckRadius = 1;
        [SerializeField] private const float CollisionCheckDelay = 0.1f;
        private LayerMask _collisionMask;

        private protected override void OnEnter()
        {
            //Set Launching Flag:
            IsLaunching = true;

            //Set Current Substate Name:
            _context.CurrentSubState = GetType().Name;

            //Setup Collision Mask to Ignore Current Cannon Layer:
            int cannonLayer = _context.CurrentCannon.gameObject.layer;
            _collisionMask = ~ (1 << cannonLayer);

            //Ensure Rigidbody is non-kinematic to allow physics to take over after launch:
            _context.Rb.isKinematic = false;

            //Reset Launch Completion Flag & Timer:
            _context.CannonBallSuperState.LaunchCompleted = false;
            _launchTimer = 0f;
        }

        private protected override void OnExit() 
        {
            //Reset Launching Flag and Collision Mask:
            IsLaunching = false;
            _collisionMask = 0;

            //Reset Cannon Ball Touchable Activation:
            if (_context.CurrentCannon.GetComponent<CannonBallTouchable>() != null)
            {
                _context.CurrentCannon.GetComponent<CannonBallTouchable>().ResetActivation();
            }

            //Set Launch Completion Flag:
            _context.CannonBallSuperState.LaunchCompleted = true;
        }

        private protected override void OnUpdate()
        {
            //Get Current Cannon:
            CannonBall cannon = _context.CurrentCannon;
            if (cannon == null) return;

            //Update Launch Timer based on Shot Power:
            _launchTimer += Time.deltaTime * cannon.ShotPower;
            float t = _launchTimer;

            //Calculate Launch Position and Velocity:
            Vector3 startPos = cannon.CannonBase.position;
            Vector3 forward = cannon.LaunchDirection.forward;
            Quaternion angleRot = Quaternion.AngleAxis(cannon.ShotAngle, cannon.CannonBase.right);
            Vector3 launchDir = angleRot * forward;

            Vector3 initialVelocity = launchDir.normalized * -cannon.LaunchVelocity;

            //Calculate Displacement:
            Vector3 displacement = initialVelocity * t;

            //Add Vertical Displacement due to Gravity:
            displacement.y += (0.5f * cannon.Acceleration * t * t) + cannon.CurrentHeight;

            //Determine Target Position:
            Vector3 targetPosition = startPos + displacement;

            //Calculate Target Velocity:
            float verticalVelocity = initialVelocity.y + cannon.Acceleration * t;
            _targetVelocity = initialVelocity;
            _targetVelocity.y = verticalVelocity;

            //Update Context Position and Rotation:
            _context.transform.position = targetPosition;
            _context.transform.rotation = Quaternion.LookRotation(launchDir, Vector3.up);

            //Check for Collision after Delay:
            if (_launchTimer > CollisionCheckDelay)
            {
                if (_context.CheckOverlap(_collisionMask, CollisionCheckRadius, out Collider hit))
                {
                    Debug.Log($"Cannon Launch Interrupted — Hit: {hit.name}");
                    _context.CannonBallSuperState.LaunchCompleted = true;
                    return;
                }
                else if (verticalVelocity <= 0f && _context.IsGrounded)
                {
                    _context.CannonBallSuperState.LaunchCompleted = true;
                    return;
                }
            }
        }

        private protected override void OnFixedUpdate()
        {
            //Apply Target Velocity to Rigidbody:
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
