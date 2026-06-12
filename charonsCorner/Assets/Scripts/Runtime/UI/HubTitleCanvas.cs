using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class HubTitleCanvas : Singleton<HubTitleCanvas>
    {
        [field: SerializeField, Required] public HubTitlePanel Panel { get; private set; }

        /// <summary>
        /// Focuses the hub title panel.
        /// </summary>
        public static void ShowHubTitle()
        {
            if (Instance != null && Instance.Panel != null)
            {
                UIPanel.Focus(Instance.Panel);
            }
        }
        
        /// <summary>
        /// Hides the hub title panel.
        /// </summary>
        public static void HideHubTitle()
        {
            if (Instance != null && Instance.Panel != null)
            {
                Instance.Panel.Close();
            }
        }
    }
}
