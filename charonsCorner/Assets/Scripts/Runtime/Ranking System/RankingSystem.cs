using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MoreMountains.Feedbacks;
using CharonsCorner.Runtime;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class RankingSystem : MonoBehaviour
{
    [Header("Checks Setting")]
    [SerializeField] Color _checkColor = Color.white;
    [SerializeField] LayerMask _playerLayer;
    [SerializeField] GameObject _startCheck;
    [SerializeField] GameObject _endCheck;
    [SerializeField] float _radius;
    [SerializeField] bool _useAlternativeEndScene;
    [SerializeField] Eflatun.SceneReference.SceneReference _alternativeEndScene;

    [Header("UI Settings")]
    [SerializeField] GameObject _RankingPanel;
    [SerializeField] GameObject _interactIcon;
    [SerializeField] TextMeshProUGUI _finalScoreText;
    [SerializeField] TextMeshProUGUI _finalTimerText;
    [SerializeField] TextMeshProUGUI _timerText;
    [SerializeField] TextMeshProUGUI _levelText;
    // Debug Feature
    [SerializeField] TextMeshProUGUI _rankText;
    [SerializeField] TextMeshProUGUI _nextRankText;
    [SerializeField] GameObject _pinUIPrefab;
    [SerializeField] GameObject _minusTextPrefab;
    [SerializeField] float _uiDestroyDelay = 3f;
    [SerializeField] Transform _uiParent;

    [Header("Score Settings")]
    [SerializeField] RankScoreSO _rankScore;
    [SerializeField] int _chapterIndex;

    [Header("Feedbacks Settings")]
    [SerializeField] MMF_Player _endLevelSequence;
    [SerializeField] MMF_Player _subtractFeedback;

    // Time
    float _timer;
    public float LevelTimeSeconds => _timer;
    
    // Checks
    bool _hasPlayerStarted;
    bool _hasPlayerFinished;

    private void OnEnable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact += HandleInteract;
        }

        PinScoring.OnPinScored += SubtractTime;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact -= HandleInteract;
        }

        PinScoring.OnPinScored -= SubtractTime;
    }

    private void HandleInteract()
    {
        if (_hasPlayerFinished)
        {
            if (_useAlternativeEndScene && _alternativeEndScene != null && !string.IsNullOrEmpty(_alternativeEndScene.Name))
            {
                GameManager.Instance.SwitchScenes(_alternativeEndScene, GameState.Gameplay).Forget();
            }
            else
            {
                GameManager.Instance.ReturnToHub();
            }
        }
    }

    private void Start()
    {
        _timer = 0f;
        _hasPlayerStarted = false;
        _hasPlayerFinished = false;

        if (_interactIcon != null)
        {
            _interactIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlayerEnd();
        }

        CheckPlayerStart();
        if (!_hasPlayerFinished) Timer();
        CheckPlayerEnd();

        if (_hasPlayerFinished) FinalRank();
    }

    public void PlayerEnd()
    {
        if (_hasPlayerFinished) return;
        
        _hasPlayerFinished = true;
        _hasPlayerStarted = true; // Ensure timer stops if it was running

        UpdateChapterProgression(_chapterIndex);

        if (_endLevelSequence != null)
        {
            _endLevelSequence.PlayFeedbacks();
        }

        if (_interactIcon != null)
        {
            _interactIcon.SetActive(true);
        }
    }

    void FinalRank()
    {
        if (_rankScore == null)
        {
            Debug.LogError("Rank Score has not been assigned");
            return;
        }
        _RankingPanel.SetActive(true);
        _finalScoreText.text = $"";
        /*int minutes = Mathf.FloorToInt(_timer / 60);
        int seconds = Mathf.FloorToInt(_timer % 60);
        _finalTimerText.text = $"Time: {string.Format("{0:00} : {1:00}", minutes, seconds)}";*/
        _finalTimerText.text = $"";
        _levelText.text = $"Chapter {_chapterIndex}";
        
        List<float> times = new List<float>();
        foreach (var time in _rankScore.Ranks) times.Add(time.Key);
        _rankText.gameObject.SetActive(true);

        // Note: Refactor to a cleaner check & elimate boundary error
        // S Rank Check
        if (_timer <= times[0])
        {
            _rankText.text = "S-Rank";
            _rankScore.SetFinalRank(Ranks.S);
            SetNextRankText("", 0); // No next rank
        }
        // A Rank Check
        else if (_timer > times[0] && _timer <= times[1])
        {
            _rankText.text = "A-Rank";
            _rankScore.SetFinalRank(Ranks.A);
            SetNextRankText("S", times[0]);
        }
        // B Rank Check
        else if (_timer > times[1] && _timer <= times[2])
        {
            _rankText.text = "B-Rank";
            _rankScore.SetFinalRank(Ranks.B);
            SetNextRankText("A", times[1]);
        }
        // C Rank Check
        else if (_timer > times[2] && _timer <= times[3])
        {
            _rankText.text = "C-Rank";
            _rankScore.SetFinalRank(Ranks.C);
            SetNextRankText("B", times[2]);
        }
        // F Rank Check
        else
        {
            _rankText.text = "F-Rank";
            _rankScore.SetFinalRank(Ranks.F);
            SetNextRankText("C", times[3]);
        }
    }

    void SetNextRankText(string rankName, float timeThreshold)
    {
        if (_nextRankText == null) return;

        if (string.IsNullOrEmpty(rankName))
        {
            _nextRankText.text = "";
            return;
        }

        int minutes = Mathf.FloorToInt(timeThreshold / 60);
        int seconds = Mathf.FloorToInt(timeThreshold % 60);
        _nextRankText.text = $"Get {string.Format("{0:0}:{1:00}", minutes, seconds)} to get an {rankName}-rank!";
    }

    void CheckPlayerStart()
    {
        if (_hasPlayerStarted) return;

        if (_playerLayer.value == 0)
        {
            Debug.LogError("Player Layer has not been assigned");
            return;
        }

        if (_startCheck == null)
        {
            Debug.LogError("Start Check's gameObject has not been assigned");
            return;
        }

        if (_radius <= 0)
        {
            Debug.LogError("Start and End check's radius needs to be higher than 0");
            return;
        }

        _hasPlayerStarted = Physics.CheckSphere(_startCheck.transform.position, _radius, _playerLayer);
    }

    void CheckPlayerEnd()
    {
        if (_hasPlayerFinished) return;

        if (_playerLayer == LayerMask.NameToLayer("Nothing"))
        {
            Debug.LogError("Player Layer has not been assigned");
            return;
        }

        if (_endCheck == null)
        {
            Debug.LogError("End Check's gameObject has not been assigned");
            return;
        }

        if (_radius <= 0)
        {
            Debug.LogError("Start and End check's radius needs to be higher than 0");
            return;
        }

        if (Physics.CheckSphere(_endCheck.transform.position, _radius, _playerLayer))
        {
            PlayerEnd();
        }
    }

    public void UpdateChapterProgression(int chapterIndex)
    {
        int currentChapter = FlagManager.Get(ProgressFlag.CurrentChapterIndex);
        if (currentChapter < chapterIndex)
        {
            FlagManager.Set(ProgressFlag.CurrentChapterIndex, chapterIndex);
        }
    }

    public void SubtractTime(float seconds)
    {
        if (_pinUIPrefab != null)
        {
            GameObject pinUIObj = Instantiate(_pinUIPrefab, _uiParent != null ? _uiParent : transform);
            PinUI pinUI = pinUIObj.GetComponentInChildren<PinUI>();
            if (pinUI != null)
            {
                Debug.Log($"[RankingSystem] Found PinUI on instantiated prefab {pinUIObj.name}, setting up action.");
                pinUI.OnAllowSubtractTime = () =>
                {
                    Debug.Log($"[RankingSystem] OnAllowSubtractTime triggered from {pinUI.name}. Calling PerformTimeSubtraction({seconds}).");
                    PerformTimeSubtraction(seconds);
                    pinUI.OnAllowSubtractTime = null; // Prevent double trigger
                };
            }
            else
            {
                Debug.LogError($"[RankingSystem] PinUI component not found in instantiated prefab {pinUIObj.name}. Ensure PinUI script is on the prefab!");
            }
            Destroy(pinUIObj, _uiDestroyDelay);
        }
        else
        {
            PerformTimeSubtraction(seconds);
        }
    }

    private void PerformTimeSubtraction(float seconds)
    {
        _timer -= seconds;
        if (_timer < 0) _timer = 0;
        UpdateTimerText();

        if (_subtractFeedback != null)
        {
            _subtractFeedback.PlayFeedbacks();
        }

        if (_minusTextPrefab != null)
        {
            GameObject minusTextObj = Instantiate(_minusTextPrefab, _uiParent != null ? _uiParent : transform);
            TextMeshProUGUI tmpText = minusTextObj.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = $"-{seconds}";
            }
            Destroy(minusTextObj, _uiDestroyDelay);
        }
    }
    
    void Timer()
    {
        if (_hasPlayerStarted)
        {
            _timer += Time.deltaTime;
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (_timerText == null)
        {
            Debug.LogError("Timer text has not been assigned");
            return;
        }

        int minutes = Mathf.FloorToInt(_timer / 60);
        int seconds = Mathf.FloorToInt(_timer % 60);
        _timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    private void OnDrawGizmos()
    {
        if (_startCheck == null || _endCheck == null) return;

        Gizmos.color = _checkColor;
        Gizmos.DrawSphere(_startCheck.transform.position, _radius);
        Gizmos.DrawSphere(_endCheck.transform.position, _radius);
    }
}
