using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class GroundedIdleState : State<PlayerController>
    {
        [SerializeField] private float damp = 0.1f;

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
            Vector3 torque = damp * -_context.RigidBody.angularVelocity;
            _context.RigidBody.AddTorque(torque, ForceMode.VelocityChange);

            _context.VisualObject.transform.rotation = _context.RigidBody.rotation;
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.Input.MoveDirection != Vector2.zero)
                return _context.GroundedSuperState.MoveState;

            return null;
        }
    }
}
