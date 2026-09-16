using UnityEngine;
using CharonsCorner.Runtime;
using Eflatun.SceneReference;
using Cysharp.Threading.Tasks;

public class CreditsController : MonoBehaviour
{    
    [Header("Settings"), Range(10f, 100f)]
    [SerializeField] float _scrollSpeed = 40f;
    [SerializeField] float _endY = 1000f;

    [Header("Skip Settings")]
    [SerializeField] GameObject _skipButton;
    [SerializeField] SceneReference _nextScene;
    [SerializeField] GameState _nextGameState = GameState.Gameplay;

    RectTransform _rectTransform;

    bool _isSkipping = false;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact += HandleInteract;
        }
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact -= HandleInteract;
        }
    }

    private void HandleInteract()
    {
        if (_skipButton != null && !_skipButton.activeSelf)
        {
            _skipButton.SetActive(true);
        }
        else
        {
            SkipSequence();
        }
    }

    private void SkipSequence()
    {
        if (_isSkipping) return;

        if (_nextScene != null && !string.IsNullOrEmpty(_nextScene.Name))
        {
            _isSkipping = true;
            GameManager.Instance.SwitchScenes(_nextScene, _nextGameState).Forget();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_skipButton != null)
        {
            _skipButton.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _rectTransform.anchoredPosition += new Vector2(0f, _scrollSpeed * Time.deltaTime);

        if (_rectTransform.anchoredPosition.y >= _endY)
        {
            SkipSequence();
        }
    }
}
