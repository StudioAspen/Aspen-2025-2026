using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(UIPanel))]
    public class DefaultSceneUIPanelSetter : MonoBehaviour
    {
        private UIPanel _panel;

        private void Awake()
        {
            _panel = GetComponent<UIPanel>();
            _panel.SetPreviousPanel(null);
            InitializePanel().Forget();
        }

        private async UniTaskVoid InitializePanel()
        {
            // Debug.Log($"[DefaultSceneUIPanelSetter] {name} starting initialization. ActivePanel: {(UIPanel.ActivePanel != null ? UIPanel.ActivePanel.name : "null")}");
            
            // Wait for LoadingCanvas to finish its work if it's currently active
            if (LoadingCanvas.Instance != null)
            {
                while (!LoadingCanvas.Instance.IsLoaded)
                {
                    await UniTask.Yield();
                }
            }

            // Debug.Log($"[DefaultSceneUIPanelSetter] {name} post-loading check. ActivePanel: {(UIPanel.ActivePanel != null ? UIPanel.ActivePanel.name : "null")}");

            // If we are in Gameplay state, we generally don't want to auto-focus a panel (like Title Screen)
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameState.Gameplay)
            {
                return;
            }

            if (UIPanel.ActivePanel == null)
            {
                // Debug.Log($"[DefaultSceneUIPanelSetter] {name} focusing panel.");
                _panel.Focus();
            }
            else if (UIPanel.ActivePanel != _panel)
            {
                // Debug.Log($"[DefaultSceneUIPanelSetter] {name} NOT focusing panel because ActivePanel is {UIPanel.ActivePanel.name}. Setting as previous.");
                UIPanel.ActivePanel.SetPreviousPanel(_panel);
            }
        }
    }
}