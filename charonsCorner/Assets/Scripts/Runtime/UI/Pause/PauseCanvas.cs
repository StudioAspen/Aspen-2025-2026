using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PauseCanvas : Singleton<PauseCanvas>
    {
        [SerializeField] private PausePanel _pausePanel;

        public void ShowPause()
        {
            UIPanel.Focus(_pausePanel);
        }
    }
}