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
            if (_railSystem.nextNode == null) return Vector3.zero;

            //Rail Direction To Next Node:
            Vector3 railStart = _railSystem.transform.position;
            Vector3 railEnd = _railSystem.nextNode.transform.position;
            Vector3 railDirection = (railEnd - railStart).normalized;

            //Calculate Player Speed Along The Rail:
            float playerSpeed = currentVelocity.magnitude * _railSystem.repelForceMultiplier;
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

            if (_railSystem.isAutoBounce)
            {
                //Apply The Calculated Bounce Angle -> Repel Force:
                if (fromRailToPlayer.sqrMagnitude > 0.001f)
                    repel = fromRailToPlayer.normalized * playerSpeed;
            }
            else
            {
                //Manually Set Vector3 Values For Set Bounces:
                repel.x = _railSystem.manualRepel.x != 0 ? _railSystem.manualRepel.x : fromRailToPlayer.x;
                repel.y = _railSystem.manualRepel.y != 0 ? _railSystem.manualRepel.y : fromRailToPlayer.y;
                repel.z = _railSystem.manualRepel.z != 0 ? _railSystem.manualRepel.z : fromRailToPlayer.z;

                //Apply The Calculated Bounce Angle + Manual Values -> Repel Force:
                if (repel.sqrMagnitude > 0.001f)
                    repel = repel.normalized * playerSpeed;
            }

            //Apply Repel Force To Player:
            deltaVelocity += repel;

            //Ensure Minimum Force Is Applied:
            if (deltaVelocity.magnitude < _railSystem.minimumForce)
            {
                deltaVelocity = railDirection * _railSystem.minimumForce;
            }

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