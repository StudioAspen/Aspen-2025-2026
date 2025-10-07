using CharonsCorner.Runtime;
using Steamworks;
using UnityEngine;

public class SteamOverlayListener : MonoBehaviour
{
    private GameManager _gameManager;
    private Callback<GameOverlayActivated_t> _overlayActivatedCallback;

    public static bool IsOverlayOpen { get; private set; }

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        if (SteamManager.Initialized)
            _overlayActivatedCallback = Callback<GameOverlayActivated_t>.Create(OnOverlayActivated);
    }

    private void OnDisable()
    {
        _overlayActivatedCallback?.Unregister();
    }

    private void OnOverlayActivated(GameOverlayActivated_t callback)
    {
        IsOverlayOpen = callback.m_bActive != 0;
        if (!IsOverlayOpen)
            return;

        // If open, pause the game if it's currently in gameplay state
        if (_gameManager.CurrentGameState == GameState.Gameplay)
            _gameManager.ChangeGameState(GameState.Paused);
    }
}
