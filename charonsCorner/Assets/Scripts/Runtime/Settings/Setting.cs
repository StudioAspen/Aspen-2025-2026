using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class Setting : MonoBehaviour
    {
        /// <summary>
        /// The key used to store the setting in PlayerPrefs.
        /// </summary>
        private protected abstract string playerPrefsKey { get; }

        /// <summary>
        /// Save the current value of the setting to PlayerPrefs and apply it to the game.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Equivalent to Awake, but called manually to ensure settings are initialized even if the object is disabled at startup.
        /// Grab the current value from PlayerPrefs and apply it to the setting.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Undo any changes made to the setting since the last load or apply.
        /// </summary>
        public abstract void Discard();

        /// <summary>
        /// If the setting has been changed from its last saved state.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsDirty();
    }
}
