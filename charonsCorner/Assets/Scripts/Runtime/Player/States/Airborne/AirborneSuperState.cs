using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class AirborneSuperState : HierarchicalState<PlayerController>
    {
        public override State<PlayerController> InitialSubState => null;

        private protected override void InitializeSubStates()
        {
            
        }

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
