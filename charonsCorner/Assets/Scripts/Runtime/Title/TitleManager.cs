using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TitleManager : MonoBehaviour
    {
        public void StartGame() => GameManager.Instance.StartGame();

        public void ShowSettings() => UIManager.Instance.ShowPanel(UIManager.PanelName.Settings);

        public static void QuitGame() => GameManager.QuitGame();
    }
}
