using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public abstract class State<TContext> where TContext : MonoBehaviour
    {
        private protected StateMachine<TContext> stateMachine;
        private protected TContext context;

        private static readonly bool enableDebug = false; // Set to true to enable debug logs for state transitions

        public virtual void Init(StateMachine<TContext> stateMachine, TContext context)
        {
            this.stateMachine = stateMachine;
            this.context = context;
            OnInit();
        }

        private protected virtual void OnInit() { }
        
        public virtual void Destroy()
        {
            if(enableDebug)
                Debug.Log($"Destroying state {GetType().Name}");
            Exit();
        }

        public virtual void Enter()
        {
            if(enableDebug)
                Debug.Log($"Entering state {GetType().Name}");
            OnEnter();
        }

        public virtual void Exit()
        {
            if(enableDebug)
                Debug.Log($"Exiting state {GetType().Name}");
            OnExit();
        }

        public virtual void Update()
        {
            State<TContext> transitionState = GetTransition();
            if (transitionState != null && !stateMachine.HasTransitionedThisFrame)
            {
                stateMachine.ChangeState(transitionState);
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
