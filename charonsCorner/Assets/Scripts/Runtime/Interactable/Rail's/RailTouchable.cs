using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class RailTouchable : TouchInteractable
    {
        private RailSystem _railSystem;
        private GameplayPlayerController _player;

        private CollisionDetectionMode _originalMode;
        private bool _modeChanged = false;

        private void Awake()
        {
            _railSystem = GetComponent<RailSystem>();
            _player = FindFirstObjectByType<GameplayPlayerController>();
        }


        public void HandleTouch()
        {
            if (_player == null || _railSystem == null) return;

            Rigidbody playerRb = _player.Rb;
            if (playerRb == null) return;

            playerRb.WakeUp();

            if (!_modeChanged)
            {
                //Save Current Collision Detection:
                _originalMode = playerRb.collisionDetectionMode;

                //Apply Change Collision Detection Mode:
                playerRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

                _modeChanged = true;
            }

            //Variable Setup:
            Vector3 currentVelocity = playerRb.linearVelocity;
            float deltaTime = Time.deltaTime;

            //Calculate Repel Force:
            Vector3 velocityChange = CalculateVelocityChange(currentVelocity, deltaTime);

            //Apply Repel Force:
            if (velocityChange != Vector3.zero)
            {
                Vector3 impulse = velocityChange * playerRb.mass;
                playerRb.AddForce(impulse, ForceMode.Impulse);
            }
        }


        private Vector3 CalculateVelocityChange(Vector3 currentVelocity, float deltaTime)
        {
            if (_railSystem.NextNode == null) return Vector3.zero;

            //Rail Direction To Next Node:
            Vector3 railStart = _railSystem.transform.position;
            Vector3 railEnd = _railSystem.NextNode.transform.position;
            Vector3 railDirection = (railEnd - railStart).normalized;

            //Calculate Player Speed Along The Rail:
            float playerSpeed = currentVelocity.magnitude * _railSystem.RepelForceMultiplier;
            float minSpeed = 1f;
            if (playerSpeed < minSpeed) playerSpeed = minSpeed;

            //Calculate The Velocity Change Along The Rail Direction:
            float railForceScale = 0.1f;
            Vector3 deltaVelocity = railDirection * playerSpeed * railForceScale * deltaTime;

            //Find Out The Player Position Relative To The Entire railDir:
            Vector3 railStartToPlayer = _player.transform.position - railStart;
            Vector3 closestPoint = railStart + Vector3.Project(railStartToPlayer, railDirection);

            //Perpendicular (90 Degree) Offset Direction To Bounce The Player Back:
            Vector3 fromRailToPlayer = _player.transform.position - closestPoint;

            //Set A Repel Force:
            Vector3 repel = Vector3.zero;

            if (_railSystem.IsAutoBounce)
            {
                //Apply The Calculated Bounce Angle -> Repel Force:
                if (fromRailToPlayer.sqrMagnitude > 0.001f)
                    repel = fromRailToPlayer.normalized * playerSpeed;
            }
            else
            {
                //Manually Set Vector3 Values For Set Bounces:
                repel.x = _railSystem.ManualRepel.x != 0 ? _railSystem.ManualRepel.x : fromRailToPlayer.x;
                repel.y = _railSystem.ManualRepel.y != 0 ? _railSystem.ManualRepel.y : fromRailToPlayer.y;
                repel.z = _railSystem.ManualRepel.z != 0 ? _railSystem.ManualRepel.z : fromRailToPlayer.z;

                //Apply The Calculated Bounce Angle + Manual Values -> Repel Force:
                if (repel.sqrMagnitude > 0.001f)
                    repel = repel.normalized * playerSpeed;
            }

            //Apply Repel Force To Player:
            deltaVelocity += repel;

            //Clamp The Force Values:
            if (deltaVelocity.magnitude < _railSystem.MinimumForce)
            {
                deltaVelocity = repel.normalized * _railSystem.MinimumForce;
            }
            else if (deltaVelocity.magnitude > _railSystem.MaximumForce)
            {
                deltaVelocity = repel.normalized * _railSystem.MaximumForce;
            }


#if UNITY_EDITOR
            //Debug Information:
            Debug.Log(deltaVelocity.magnitude);
            Debug.DrawRay(_player.transform.position, deltaVelocity, Color.yellow, 0.5f);
#endif

            return deltaVelocity;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_modeChanged) return;
            if (_player == null) return;

            Rigidbody playerRb = _player.Rb;
            if (playerRb == null) return;

            //Revert Back To Original Collision Detection Mode:
            playerRb.collisionDetectionMode = _originalMode;
            _modeChanged = false;
        }
    }
}