using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[System.Serializable]
	public class AIActionsList : MMReorderableArray<AIAction>
	{
	}
	[System.Serializable]
	public class AITransitionsList : MMReorderableArray<AITransition>
	{
	}
	
	public struct AIStateEvent
	{
		public AIBrain Brain;
		public AIState ExitState;
		public AIState EnterState;

		public AIStateEvent(AIBrain brain, AIState exitState, AIState enterState)
		{
			Brain = brain;
			ExitState = exitState;
			EnterState = enterState;
		}

		static AIStateEvent e;
		public static void Trigger(AIBrain brain, AIState exitState, AIState enterState)
		{
			e.Brain = brain;
			e.ExitState = exitState;
			e.EnterState = enterState;
			MMEventManager.TriggerEvent(e);
		}
	}

	/// <summary>
	/// A State is a combination of one or more actions, and one or more transitions. An example of a state could be "_patrolling until an enemy gets in range_".
	/// </summary>
	[System.Serializable]
	public class AIState 
	{
		/// the name of the state (will be used as a reference in Transitions
		public string StateName;

		[MMReorderableAttribute(null, "Action", null)]
		public AIActionsList Actions;
		[MMReorderableAttribute(null, "Transition", null)]
		public AITransitionsList Transitions;/*

        /// a list of actions to perform in this state
        public List<AIAction> Actions;
        /// a list of transitions to evaluate to exit this state
        public List<AITransition> Transitions;*/

		protected AIBrain _brain;

		/// <summary>
		/// Sets this state's brain to the one specified in parameters
		/// </summary>
		/// <param name="brain"></param>
		public virtual void SetBrain(AIBrain brain)
		{
			_brain = brain;
		}
                	
		/// <summary>
		/// On enter state we pass that info to our actions and decisions
		/// </summary>
		public virtual void EnterState()
		{
			foreach (AIAction action in Actions)
			{
				action.OnEnterState();
			}
			foreach (AITransition transition in Transitions)
			{
				if (transition.Decision != null)
				{
					transition.Decision.OnEnterState();
				}
			}
		}

		/// <summary>
		/// On exit state we pass that info to our actions and decisions
		/// </summary>
		public virtual void ExitState()
		{
			foreach (AIAction action in Actions)
			{
				action.OnExitState();
			}
			foreach (AITransition transition in Transitions)
			{
				if (transition.Decision != null)
				{
					transition.Decision.OnExitState();
				}
			}
		}

		/// <summary>
		/// Performs this state's actions
		/// </summary>
		public virtual void PerformActions()
		{
			if (Actions.Count == 0) { return; }
			for (int i=0; i<Actions.Count; i++) 
			{
				if (Actions[i] != null)
				{
					Actions[i].PerformAction();
				}
				else
				{
					Debug.LogError("An action in " + _brain.gameObject.name + " on state " + StateName + " is null.");
				}
			}
		}

		/// <summary>
		/// Tests this state's transitions
		/// </summary>
		public virtual void EvaluateTransitions()
		{
			if (Transitions.Count == 0) { return; }
			for (int i = 0; i < Transitions.Count; i++) 
			{
				if (Transitions[i].Decision != null)
				{
					if (Transitions[i].Decision.Decide())
					{
						Transitions[i].LastDecisionEvaluation = true;
						if (!string.IsNullOrEmpty(Transitions[i].TrueState))
						{
							_brain.TransitionToState(Transitions[i].TrueState);
							break;
						}
					}
					else
					{
						Transitions[i].LastDecisionEvaluation = false;
						if (!string.IsNullOrEmpty(Transitions[i].FalseState))
						{
							_brain.TransitionToState(Transitions[i].FalseState);
							break;
						}
					}
				}                
			}
		}        
	}
}