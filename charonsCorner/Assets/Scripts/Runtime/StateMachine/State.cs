using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public abstract class State<TContext> where TContext : MonoBehaviour
    {
        private protected StateMachine<TContext> _stateMachine;
        private protected TContext _context;

        private static readonly bool EnableDebug = false; // Set to true to enable debug logs for state transitions

        public virtual void Init(StateMachine<TContext> stateMachine, TContext context)
        {
            this._stateMachine = stateMachine;
            this._context = context;
            OnInit();
        }

        private protected virtual void OnInit() { }
        
        public virtual void Destroy()
        {
            if(EnableDebug)
                Debug.Log($"Destroying state {GetType().Name}");
            Exit();
        }

        public virtual void Enter()
        {
            if(EnableDebug)
                Debug.Log($"Entering state {GetType().Name}");
            OnEnter();
        }

        public virtual void Exit()
        {
            if(EnableDebug)
                Debug.Log($"Exiting state {GetType().Name}");
            OnExit();
        }

        public virtual void Update()
        {
            State<TContext> transitionState = GetTransition();
            if (transitionState != null && !_stateMachine.HasTransitionedThisFrame)
            {
                _stateMachine.ChangeState(transitionState);
                return;
            }

            OnUpdate();
        }

        public virtual void FixedUpdate()
        {
            OnFixedUpdate();
        }

        private protected abstract void OnEnter();
        private protected abstract void OnExit();
        private protected abstract void OnUpdate();
        private protected abstract void OnFixedUpdate();

        /// <summary>
        /// Polls for a transition to another state.
        /// Update will call this method and if it returns a non-null state, the state machine will transition to that state.
        /// Override this method to implement custom transition logic.
        /// </summary>
        private protected virtual State<TContext> GetTransition() => null;
    }
}
