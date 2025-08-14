using NaughtyAttributes;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class StateMachine<TContext> where TContext : MonoBehaviour
    {
        public TContext Context { get; private set; }

        public State<TContext> CurrentState { get; private set; }
        [SerializeField, ReadOnly, AllowNesting] private string currentStateName = "None";

        public event Action<State<TContext>> OnStateChanged = delegate { };

        public StateMachine(TContext context)
        {
            Context = context;
        }

        public void ChangeState(State<TContext> newState, bool force = false)
        {
            if (newState == null)
                return;

            if(CurrentState == newState && !force)
                return;

            HierarchicalState<TContext> hierarchicalState = CurrentState as HierarchicalState<TContext>;
            if (hierarchicalState != null)
                hierarchicalState.SubStateMachine?.CurrentState?.Exit();
            CurrentState?.Exit();

            CurrentState = newState;

            CurrentState?.Enter();
            hierarchicalState = CurrentState as HierarchicalState<TContext>;
            if (hierarchicalState != null)
                hierarchicalState.SubStateMachine?.ChangeState(hierarchicalState.InitialSubState, true);

            OnStateChanged.Invoke(CurrentState);

            currentStateName = CurrentState != null ? CurrentState.GetType().Name : "None";
        }

        public void Update()
        {
            CurrentState?.Update();

            if (CurrentState is HierarchicalState<TContext> hierarchicalState)
                hierarchicalState.SubStateMachine?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();

            if (CurrentState is HierarchicalState<TContext> hierarchicalState)
                hierarchicalState.SubStateMachine?.FixedUpdate();
        }
        
        public void Dispose()
        {
            if (CurrentState is HierarchicalState<TContext> hierarchicalState)
                hierarchicalState.SubStateMachine?.Dispose();
            CurrentState?.Exit();
            OnStateChanged = null; // Unsubscribe all listeners
        }
    }
}
