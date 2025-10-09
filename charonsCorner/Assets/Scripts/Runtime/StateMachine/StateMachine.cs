using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public class StateMachine<TContext> where TContext : MonoBehaviour
    {
        public TContext Context { get; private set; }

        public State<TContext> CurrentState { get; private set; }
        [SerializeField, ReadOnly, AllowNesting] private string currentStateName = "None";

        public bool HasTransitionedThisFrame { get; private set; } = false;

        public StateMachine(TContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Changes the current state to the new state.
        /// If the new state is the same as the current state, it will not change unless forceChangeToSameState is true.
        /// </summary>
        public void ChangeState(State<TContext> newState, bool forceChangeToSameState = false)
        {
            if (HasTransitionedThisFrame)
                return;

            if (newState == null)
                return;

            if(CurrentState == newState && !forceChangeToSameState)
                return;

            HasTransitionedThisFrame = true;

            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();

            currentStateName = CurrentState != null ? CurrentState.GetType().Name : "None";
        }

        public void Update()
        {
            HasTransitionedThisFrame = false;
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        public void ExitCurrentState()
        {
            CurrentState?.Exit();
            CurrentState = null;
            currentStateName = "None";
        }

        public void Destroy()
        {
            CurrentState?.Destroy();
        }
    }
}
