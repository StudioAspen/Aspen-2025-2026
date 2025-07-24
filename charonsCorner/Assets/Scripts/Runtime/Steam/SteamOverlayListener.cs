using CharonsCorner.Runtime;
using Steamworks;
using UnityEngine;

public class SteamOverlayListener : MonoBehaviour
{
    private GameManager gameManager;
    private Callback<GameOverlayActivated_t> overlayActivatedCallback;

    public static bool IsOverlayOpen { get; private set; }

    private void Start()
    {
        gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        if (SteamManager.Initialized)
            overlayActivatedCallback = Callback<GameOverlayActivated_t>.Create(OnOverlayActivated);
    }

    private void OnDisable()
    {
        overlayActivatedCallback?.Unregister();
    }

    private void OnOverlayActivated(GameOverlayActivated_t callback)
    {
        IsOverlayOpen = callback.m_bActive != 0;
        if (!IsOverlayOpen)
            return;

        // If open, pause the game if it's currently in gameplay state
        if (gameManager.CurrentGameState == GameState.Gameplay)
            gameManager.ChangeGameState(GameState.Paused);
    }
}
