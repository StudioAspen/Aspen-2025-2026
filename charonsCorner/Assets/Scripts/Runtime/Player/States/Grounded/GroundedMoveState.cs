using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedMoveState : State<PlayerController>
    {
        [SerializeField] private float acceleration = 1f;
        [SerializeField] private float turnResponsiveness = 1f;
        [SerializeField] private float maxSpeed = 25f;

        private protected override void OnEnter()
        {
            
        }

        private protected override void OnExit()
        {

        }

        private protected override void OnUpdate()
        {

        }

        private protected override void OnFixedUpdate()
        {
            Vector3 moveDirection = Utilities.GetCameraBasedMoveInput(
                CameraManager.Instance.CurrentCamera.transform, 
                _context.Input.MoveDirection
                );
            Vector3 groundNormal = _context.GroundedSuperState.GetGroundNormal();
            Vector3 slopeAdjustedDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;
            
            _context.SetPlanarVelocity(_context.PlanarVelocity + acceleration * Time.deltaTime * slopeAdjustedDirection);
            _context.ApplyPlanarVelocity();
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.Input.MoveDirection == Vector2.zero)
                return _context.GroundedSuperState.IdleState;

            return null;
        }
    }
}
