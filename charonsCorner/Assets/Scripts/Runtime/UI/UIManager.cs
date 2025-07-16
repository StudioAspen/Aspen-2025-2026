using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        public enum PanelName
        {
            PauseMenu,
            Settings,
            Dialogue,
        }

        [Header("Panels")]
        [SerializeField, SerializedDictionary("Panel Name", "Panel")] 
        private SerializedDictionary<PanelName, UIPanel> panels = new SerializedDictionary<PanelName, UIPanel>();
        [field: SerializeField, ReadOnly] public UIPanel CurrentPanel { get; private set; }

        public event Action<UIPanel> OnPanelChanged = delegate { };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePanels();
        }

        private void InitializePanels()
        {
            foreach (UIPanel panel in panels.Values)
                panel.Init(this);
        }

        public void ShowPanel(PanelName panelName)
        {
            UIPanel panel = panels[panelName];

            if (CurrentPanel == panel)
                return;

            if (CurrentPanel != null && CurrentPanel != panel)
                CurrentPanel.Hide();

            CurrentPanel = panel;
            CurrentPanel.Show();
            OnPanelChanged.Invoke(CurrentPanel);

            InputManager.Instance.EnableUIActions();
        }

        public void HideAllPanels()
        {
            foreach (UIPanel panel in panels.Values)
                panel.Hide();

            CurrentPanel = null;
        }
    }
}
