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
            Vector3 oppositeForce = damp * -_context.PlanarVelocity;
            
            _context.SetPlanarVelocity(_context.PlanarVelocity + oppositeForce * Time.deltaTime);
            _context.ApplyPlanarVelocity();
        }

        private protected override State<PlayerController> GetTransition()
        {
            if (_context.Input.MoveDirection != Vector2.zero)
                return _context.GroundedSuperState.MoveState;

            return null;
        }
    }
}
