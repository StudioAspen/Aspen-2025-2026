using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TitleManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private UIManager uiManager;

        public void StartGame()
        {
            gameManager.StartGame();
        }

        public void ShowSettings()
        {
            uiManager.ShowPanel(UIManager.PanelName.Settings);
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
