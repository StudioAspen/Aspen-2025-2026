using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class JumpState : State<PlayerController>
    {
        [field: SerializeField] public float JumpHeight { get; private set; } = 2f;

        public override void Enter()
        {

        }

        public override void Exit()
        {

        }

        public override void Update()
        {

        }

        public override void FixedUpdate()
        {

        }
    }
}
