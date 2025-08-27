using UnityEngine;

namespace CharonsCorner.Runtime
{
    [CreateAssetMenu(fileName = "PlayerControllerConfig", menuName = "CharonsCorner/Player/ControllerConfig", order = 0)]
    public class PlayerControllerConfigSO : ScriptableObject
    {
        [field: SerializeField] public GroundedSuperState GroundedSuperState { get; private set; } = new();
        [field: SerializeField] public AirborneSuperState AirborneSuperState { get; private set; } = new();

        /// <summary>
        /// Runs in awake. Sets up the state machine and all the states for the player controller.
        /// </summary>
        public void InitializeStates(StateMachine<PlayerController> machine, PlayerController context)
        {
            GroundedSuperState.Init(machine, context);
            AirborneSuperState.Init(machine, context);

            machine.ChangeState(GroundedSuperState, context);
        }
    }
}
