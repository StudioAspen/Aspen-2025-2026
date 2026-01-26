namespace CharonsCorner.Runtime
{
    public enum ProgressFlag
    {
        TutorialCompleted,
        World1CurrentChapter,
        World1CurrentDialogue
    }

    public static class FlagManager
    {
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
        /// This will overwrite any existing value.
        /// </summary>
        /// <param name="flag">The progress flag to set.</param>
        /// <param name="value">The value to assign to the flag.</param>
        public static void Set(ProgressFlag flag, int value)
        {
            SaveManager.GameStore.SetInt(flag.ToString(), value);
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
        /// Intended for new games, save wipes, or debugging.
        /// </summary>
        public static void ResetAll()
        {
            foreach (ProgressFlag flag in System.Enum.GetValues(typeof(ProgressFlag)))
            {
                SaveManager.GameStore.SetInt(flag.ToString(), 0);
            }
        }
    }
}