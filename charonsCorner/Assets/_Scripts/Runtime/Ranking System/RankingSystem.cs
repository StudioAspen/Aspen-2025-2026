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
    [SerializeField] GameObject _endCheck;
    [SerializeField] float _radius;
    [SerializeField] bool _useAlternativeEndScene;
    [SerializeField] Eflatun.SceneReference.SceneReference _alternativeEndScene;

    [Header("UI Settings")]
    [SerializeField] GameObject _interactIcon;
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        PinScoring.OnPinScored += SubtractTime;
        Checkpoint.OnCheckpointHit += HandleCheckpointHit;
    }

    private void OnDisable()
    {
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Interact -= HandleInteract;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        PinScoring.OnPinScored -= SubtractTime;
        Checkpoint.OnCheckpointHit -= HandleCheckpointHit;
    }

    private void HandleCheckpointHit(Checkpoint checkpoint)
    {
        if (!_hasPlayerStarted)
        {
            _hasPlayerStarted = true;
            Debug.Log($"RankingSystem: Timer started by checkpoint {checkpoint.CheckpointIndex}");
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
    }

    private void HandleInteract()
    {
        if (_hasPlayerFinished)
        {
            if (_useAlternativeEndScene && _alternativeEndScene != null && !string.IsNullOrEmpty(_alternativeEndScene.Name))
            {
                if (FlagManager.Get(ProgressFlag.SeenAlternativeEndScene) == 0)
                {
                    FlagManager.Set(ProgressFlag.SeenAlternativeEndScene, 1);
                    GameManager.Instance.SwitchScenes(_alternativeEndScene, GameState.Gameplay).Forget();
                }
                else
                {
                    GameManager.Instance.ReturnToHub();
                }
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

        if (GameManager.Instance != null)
        {
            // Visibility logic moved to TimerUI
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            PlayerEnd();
        }

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
        
        List<float> times = new List<float>();
        foreach (var time in _rankScore.Ranks) times.Add(time.Key);
        _rankText.gameObject.SetActive(true);

        Ranks currentRank;
        // Note: Refactor to a cleaner check & elimate boundary error
        // S Rank Check
        if (_timer <= times[0])
        {
            _rankText.text = "S-Rank";
            currentRank = Ranks.S;
            SetNextRankText("", 0); // No next rank
            UpdateSRankProgression();
        }
        // A Rank Check
        else if (_timer > times[0] && _timer <= times[1])
        {
            _rankText.text = "A-Rank";
            currentRank = Ranks.A;
            SetNextRankText("S", times[0]);
        }
        // B Rank Check
        else if (_timer > times[1] && _timer <= times[2])
        {
            _rankText.text = "B-Rank";
            currentRank = Ranks.B;
            SetNextRankText("A", times[1]);
        }
        // C Rank Check
        else if (_timer > times[2] && _timer <= times[3])
        {
            _rankText.text = "C-Rank";
            currentRank = Ranks.C;
            SetNextRankText("B", times[2]);
        }
        // F Rank Check
        else
        {
            _rankText.text = "F-Rank";
            currentRank = Ranks.F;
            SetNextRankText("C", times[3]);
        }
        
        _rankScore.SetFinalRank(currentRank);
        SaveBestStats(currentRank, _timer);
    }

    private void SaveBestStats(Ranks rank, float time)
    {
        string levelKey = $"Level_{_chapterIndex}"; 
        string bestRankKey = $"{levelKey}_BestRank";
        string bestTimeKey = $"{levelKey}_BestTime";

        // Lower enum value means better rank (S=0, A=1, etc.)
        int savedRankInt = SaveManager.GameStore.GetInt(bestRankKey, (int)Ranks.F + 1);
        
        if ((int)rank < savedRankInt)
        {
            SaveManager.GameStore.SetInt(bestRankKey, (int)rank);
        }

        float savedTime = SaveManager.GameStore.GetFloat(bestTimeKey, float.MaxValue);
        if (time < savedTime)
        {
            SaveManager.GameStore.SetFloat(bestTimeKey, time);
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

    void CheckPlayerEnd()
    {
        if (_hasPlayerFinished) return;

        if (_playerLayer.value == 0)
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

    private void UpdateSRankProgression()
    {
        var list = SaveManager.GameStore.GetList<int>(DialogueSaveKeys.SRankAchievedListKey, new());
        if (!list.Contains(_chapterIndex))
        {
            list.Add(_chapterIndex);
            SaveManager.GameStore.SetList(DialogueSaveKeys.SRankAchievedListKey, list);
            FlagManager.Increment(ProgressFlag.SRankCount);
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
        }
    }

    private void OnDrawGizmos()
    {
        if (_endCheck == null) return;

        Gizmos.color = _checkColor;
        Gizmos.DrawSphere(_endCheck.transform.position, _radius);
    }
}
