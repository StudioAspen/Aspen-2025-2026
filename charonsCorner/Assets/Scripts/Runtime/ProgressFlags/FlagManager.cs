using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public enum ProgressFlag
    {
        TutorialCompleted,
        CurrentChapterIndex,
        CurrentDialogueOpenerIndex,
        CurrentDialogueSequenceCompleted, // 0 - non finished, 1 - first dialogue finished, 2 - second dialogue finished
        CurrentSRankDialogueIndex,
        CurrentMomentoDialogueOpenerIndex,
        CurrentMomentoDialogueSequenceCompleted
    }

    /// <summary>
    /// Handles the saving and tracking of progression through flags.
    /// Provides change notifications when flag values are modified.
    /// </summary>
    public static class FlagManager
    {
        /// <summary>
        /// Invoked when a progress flag value changes.
        /// The event is fired only if the new value differs from the previous value.
        /// </summary>
        public static event Action<ProgressFlag, int> OnFlagChanged;
        
        /// <summary>
        /// Retrieves the current value of a progress flag.
        /// If the flag has not been set, returns the provided default value.
        /// </summary>
        /// <param name="flag">The progress flag to retrieve.</param>
        /// <param name="defaultValue">Value returned if the flag does not exist.</param>
        /// <returns>The stored value of the flag.</returns>
        public static int Get(ProgressFlag flag, int defaultValue = 0)
        {
            return SaveManager.GameStore.GetInt(flag.ToString(), defaultValue);
        }

        /// <summary>
        /// Sets the value of a progress flag.
        /// Triggers the OnFlagChanged event if the value has changed.
        /// </summary>
        public static void Set(ProgressFlag flag, int value)
        {
            int previousValue = Get(flag);
            if (previousValue == value)
                return;

            SaveManager.GameStore.SetInt(flag.ToString(), value);
            OnFlagChanged?.Invoke(flag, value);
        }

        /// <summary>
        /// Increments the value of a progress flag by one.
        /// If the flag does not exist, it will be initialized to 1.
        /// </summary>
        /// <param name="flag">The progress flag to increment.</param>
        public static void Increment(ProgressFlag flag)
        {
            Set(flag, Get(flag) + 1);
        }

        /// <summary>
        /// Resets all progress flags to their default value (0).
        /// Triggers change events for any flags that were previously non-zero.
        /// </summary>
        public static void ResetAll()
        {
            foreach (ProgressFlag flag in Enum.GetValues(typeof(ProgressFlag)))
            {
                Set(flag, 0);
            }
        }
        
        /// <summary>
        /// Clears all event subscribers.
        /// Called automatically on domain reload / play mode start.
        /// Prevents leaked subscriptions from previous sessions.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEvents()
        {
            OnFlagChanged = null;
        }
    }
}