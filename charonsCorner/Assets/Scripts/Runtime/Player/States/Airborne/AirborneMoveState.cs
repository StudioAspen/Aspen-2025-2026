using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneMoveState : State<PlayerController>
    {
        [field: SerializeField] public float Damp { get; private set; } = 0.1f;

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
            /*Vector3 moveDirection =
                Utilities.GetCameraBasedMoveInput(CameraManager.Instance.CurrentCamera.transform, _context.Input.MoveDirection);

            if(moveDirection != Vector3.zero)
                _context.RigidBody.AddForce(_context.AirborneSuperState.MaxSpeed * moveDirection - _context.RigidBody.linearVelocity.WithY(0), ForceMode.VelocityChange);
            
            _context.VisualObject.transform.rotation = _context.RigidBody.rotation;*/
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.Input.MoveDirection == Vector2.zero)
                return _context.AirborneSuperState.IdleState;

            return null;
        }
    }
}
