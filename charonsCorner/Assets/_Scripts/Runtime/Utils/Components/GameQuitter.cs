using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class GameQuitter : MonoBehaviour
    {
        public void QuitGame()
        {
            GameManager.QuitGame();
        }
    }
}