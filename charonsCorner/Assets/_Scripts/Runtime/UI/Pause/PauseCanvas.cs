using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PauseCanvas : Singleton<PauseCanvas>
    {
        [SerializeField] private PausePanel _pausePanel;

        [ShowInInspector, ReadOnly] public static bool IsPauseBlocked { get; private set; }
        
        public void ShowPause()
        {
            if (IsPauseBlocked)
                return;
            
            UIPanel.Focus(_pausePanel);
        }
        
        public void BlockPause(bool isBlocked)
        {
            IsPauseBlocked = isBlocked;
        }
    }
}