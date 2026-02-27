using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Executes UnityEvents based on the current progress flag state.
    /// Reacts dynamically to flag changes at runtime.
    /// </summary>
    public class ProgressFlagActions : MonoBehaviour
    {
        /// <summary>
        /// A collection of required progress flags and their minimum values.
        /// All conditions must be met for the object to be considered unlocked.
        /// </summary>
        [SerializeField, SerializedDictionary("Progress Flag", "Required Value")]
        private SerializedDictionary<ProgressFlag, int> _requiredFlags =
            new SerializedDictionary<ProgressFlag, int>();

        /// <summary>
        /// Invoked when all required progression conditions are satisfied.
        /// </summary>
        [field: SerializeField] public UnityEvent UnlockedAction { get; private set; } = new UnityEvent();

        /// <summary>
        /// Invoked when one or more progression conditions are not satisfied.
        /// </summary>
        [field: SerializeField] public UnityEvent LockedAction { get; private set; } = new UnityEvent();

        private void OnEnable()
        {
            FlagManager.OnFlagChanged += HandleFlagChanged;
            Evaluate(); // evaluate immediately on enable
        }

        private void OnDisable()
        {
            FlagManager.OnFlagChanged -= HandleFlagChanged;
        }

        /// <summary>
        /// Re-evaluates whether all required flag conditions are met
        /// and invokes the appropriate event if the state has changed.
        /// </summary>
        private void Evaluate()
        {
            if (_requiredFlags.Count == 0)
            {
                UnlockedAction.Invoke();
                return;
            }
            
            bool shouldBeUnlocked = true;
            foreach (var requirement in _requiredFlags)
            {
                if (FlagManager.Get(requirement.Key) < requirement.Value)
                {
                    shouldBeUnlocked = false;
                    break;
                }
            }
            
            if (shouldBeUnlocked)
                UnlockedAction.Invoke();
            else
                LockedAction.Invoke();
        }

        private void HandleFlagChanged(ProgressFlag flag, int value)
        {
            // Only re-evaluate if this flag matters to us
            if (!_requiredFlags.ContainsKey(flag))
                return;

            Evaluate();
        }
    }
}