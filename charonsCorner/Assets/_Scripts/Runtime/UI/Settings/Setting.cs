using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public abstract class Setting : MonoBehaviour
    {
        [Header("Visual Feedback")]
        [SerializeField] private protected TMP_Text _settingLabel;
        [SerializeField] private protected Color _dirtyColor = Color.yellow;
        [SerializeField] private protected Color _cleanColor = Color.white;

        /// <summary>
        /// The key used to store the setting in the save store.
        /// </summary>
        private protected abstract string SaveKey { get; }

        protected virtual void Update()
        {
            UpdateLabelColor();
        }

        protected void UpdateLabelColor()
        {
            if (_settingLabel == null) return;
            _settingLabel.color = IsDirty() ? _dirtyColor : _cleanColor;
        }

        /// <summary>
        /// Save the current value of the setting to save store and apply it to the game.
        /// </summary>
        public abstract void Apply();

        /// <summary>
        /// Equivalent to Awake, but called manually to ensure settings are initialized even if the object is disabled at startup.
        /// Grab the current value from save store and apply it to the setting.
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
