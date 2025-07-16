using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Bootstrap : MonoBehaviour
    {
        private static bool hasBootstrapped = false;

        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameState initialGameState;

        private void Awake()
        {
            if (hasBootstrapped)
            {
                Destroy(gameObject);
                return;
            }

            hasBootstrapped = true;
            gameManager.ChangeInitialGameState(initialGameState);

            DontDestroyOnLoad(gameObject);
        }
    }
}
