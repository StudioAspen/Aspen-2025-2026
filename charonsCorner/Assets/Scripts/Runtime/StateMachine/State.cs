using UnityEngine;

namespace CharonsCorner.Runtime
{
    [System.Serializable]
    public abstract class State<TContext> where TContext : MonoBehaviour
    {
        private protected StateMachine<TContext> stateMachine;
        private protected TContext context;

        public virtual void Init(StateMachine<TContext> stateMachine, TContext context)
        {
            this.stateMachine = stateMachine;
            this.context = context;
            OnInit();
        }

        private protected virtual void OnInit() { }
        
        public abstract void Enter();
        public abstract void Exit();
        public abstract void Update();
        public abstract void FixedUpdate();
    }
}
