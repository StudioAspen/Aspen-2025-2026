using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Helper component to easily open the loading screen.
    /// </summary>
    public class LoadingScreenOpener : MonoBehaviour
    {
        public void OpenLoadingScreen()
        {
            LoadingCanvas.Instance.Show();
        }
    }
}