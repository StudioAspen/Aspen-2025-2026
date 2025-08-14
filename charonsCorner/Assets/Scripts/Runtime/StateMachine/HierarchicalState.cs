using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public abstract class HierarchicalState<TContext> : State<TContext> where TContext : MonoBehaviour
    {
        [field: Header("States")]
        [field: SerializeField] public StateMachine<TContext> SubStateMachine { get; private set; }
        public abstract State<TContext> InitialSubState { get; }

        private protected override void OnInit()
        {
            SubStateMachine = new StateMachine<TContext>(context);
            InitializeSubStates();
        }

        private protected abstract void InitializeSubStates();
    }
}
