using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TitleManager : MonoBehaviour
    {
        // Called by button UI events
        public void StartGame() => GameManager.Instance.StartGame();
        public void ShowSettings() => SettingsCanvas.ShowSettings();
        public static void QuitGame() => GameManager.QuitGame();
    }
}
