using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneIdleState : State<PlayerController>
    {
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
            _context.RigidBody.AddForce(_context.RigidBody.linearVelocity.WithY(0).normalized * _context.AirborneSuperState.MaxSpeed - _context.RigidBody.linearVelocity.WithY(0), ForceMode.VelocityChange);
            
            _context.VisualObject.transform.rotation = _context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.Input.MoveDirection != Vector2.zero)
                return _context.AirborneSuperState.MoveState;

            return null;
        }
    }
}
