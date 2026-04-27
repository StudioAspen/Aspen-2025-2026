using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class SplineLaunchState : State<GameplayPlayerController>
    {
        private SplineContainer _splineContainer;
        private float _launchSpeed;
        private float _t;
        private float _splineLength;
        private Vector3 _exitForce;
        private System.Action _onExitCallback;
        public bool LaunchCompleted { get; private set; }

        public void SetLaunchParameters(SplineContainer spline, float speed, Vector3 exitForce, System.Action onExitCallback = null)
        {
            _splineContainer = spline;
            _launchSpeed = speed;
            _exitForce = exitForce;
            _onExitCallback = onExitCallback;
            LaunchCompleted = false;
        }

        private protected override void OnEnter()
        {
            if (_splineContainer == null)
            {
                LaunchCompleted = true;
                return;
            }

            _t = 0f;
            _splineLength = _splineContainer.CalculateLength();
            _context.Rb.isKinematic = true;
            LaunchCompleted = false;
        }

        private protected override void OnExit()
        {
            _context.Rb.isKinematic = false;
            if (_exitForce.sqrMagnitude > 0.001f)
            {
                _context.Rb.AddForce(_exitForce, ForceMode.VelocityChange);
            }
            _onExitCallback?.Invoke();
            _onExitCallback = null;
            LaunchCompleted = false;
        }

        private protected override void OnUpdate()
        {
            if (_splineContainer == null)
            {
                LaunchCompleted = true;
                return;
            }

            float speedOnSpline = _launchSpeed / _splineLength;
            _t += speedOnSpline * Time.deltaTime;

            if (_t >= 1f)
            {
                _t = 1f;
                LaunchCompleted = true;
            }

            _splineContainer.Evaluate(_t, out float3 position, out float3 forward, out float3 upVector);
            _context.Rb.position = (Vector3)position;
            // Optionally rotate player to match spline forward? 
            // The requirement didn't specify, but usually "launched" means following the path orientation too.
            _context.transform.forward = (Vector3)forward;
        }

        private protected override void OnFixedUpdate()
        {
        }

        private protected override State<GameplayPlayerController> GetTransition()
        {
            if (LaunchCompleted)
            {
                return _context.IsGrounded ? _context.GroundSuperState : _context.AirSuperState;
            }
            return null;
        }
    }
}
