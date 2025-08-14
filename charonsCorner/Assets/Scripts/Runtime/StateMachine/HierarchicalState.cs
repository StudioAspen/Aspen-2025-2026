using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public abstract class HierarchicalState<TContext> : State<TContext> where TContext : MonoBehaviour
    {
        [field: SerializeField] public StateMachine<TContext> SubStateMachine { get; private set; }
        public abstract State<TContext> InitialSubState { get; }
        [SerializeField, ReadOnly, AllowNesting] private string initialSubStateName = "None";

        private protected override void OnInit()
        {
            SubStateMachine = new StateMachine<TContext>(context);
            InitializeSubStates();

            initialSubStateName = InitialSubState != null ? InitialSubState.GetType().Name : "None";
        }

        private protected abstract void InitializeSubStates();
    }
}
