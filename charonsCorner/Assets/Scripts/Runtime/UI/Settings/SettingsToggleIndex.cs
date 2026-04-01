using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public abstract class SettingsToggleIndex : Setting
    {
        [SerializeField] protected List<Toggle> _toggles;

        protected virtual void OnEnable()
        {
            AttachListeners();
        }

        protected virtual void OnDisable()
        {
            DetachListeners();
        }

        private void AttachListeners()
        {
            if (_toggles == null) return;

            foreach (var toggle in _toggles)
            {
                if (toggle != null)
                    toggle.onValueChanged.AddListener(OnToggleValueChanged);
            }
        }

        private void DetachListeners()
        {
            if (_toggles == null) return;

            foreach (var toggle in _toggles)
            {
                if (toggle != null)
                    toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
            }
        }

        private void OnToggleValueChanged(bool _)
        {
            // Let derived classes react if needed
            OnSelectionChanged(GetSelectedIndex());
        }

        protected int GetSelectedIndex()
        {
            if (_toggles == null)
                return 0;

            for (int i = 0; i < _toggles.Count; i++)
            {
                if (_toggles[i] != null && _toggles[i].isOn)
                    return i;
            }

            return 0;
        }

        protected void SetTogglesToIndex(int index)
        {
            if (_toggles == null) return;

            for (int i = 0; i < _toggles.Count; i++)
            {
                if (_toggles[i] != null)
                    _toggles[i].isOn = (i == index);
            }
        }

        protected virtual void OnSelectionChanged(int index) { }
    }
}