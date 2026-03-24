using System.Collections.Generic;
using TMPro;
using UnityEngine;
using MoreMountains.Feedbacks;
using CharonsCorner.Runtime;
using UnityEngine.InputSystem;

public class RankingSystem : MonoBehaviour
{
    [Header("Checks Setting")]
    [SerializeField] Color _checkColor = Color.white;
    [SerializeField] LayerMask _playerLayer;
    [SerializeField] GameObject _startCheck;
    [SerializeField] GameObject _endCheck;
    [SerializeField] float _radius;

    [Header("UI Settings")]
    [SerializeField] TextMeshProUGUI _timerText;
    // Debug Feature
    [SerializeField] TextMeshProUGUI _rankText;

    [Header("Score Settings")]
    [SerializeField] RankScoreSO _rankScore;
    [SerializeField] int _chapterIndex;

    [Header("Feedbacks Settings")]
    [SerializeField] MMF_Player _endLevelSequence;

    // Time
    float _timer;
    
    // Checks
    bool _hasPlayerStarted;
    bool _hasPlayerFinished;

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
        if (_hasPlayerFinished)
        {
            GameManager.Instance.ReturnToHub();
        }
    }

    private void Start()
    {
        _timer = 0f;
        _hasPlayerStarted = false;
        _hasPlayerFinished = false;
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

        // Note: Refactor to a cleaner check & elimate boundary error
        // S Rank Check
        if (_timer < times[0])
        {
            _rankText.text = "S-Rank";
            _rankScore.SetFinalRank(Ranks.S);
        }
        // A Rank Check
        else if (times[0] < _timer && _timer < times[1])
        {
            _rankText.text = "A-Rank";
            _rankScore.SetFinalRank(Ranks.A);
        }
        // B Rank Check
        else if (times[1] < _timer && _timer < times[2])
        {
            _rankText.text = "B-Rank";
            _rankScore.SetFinalRank(Ranks.B);
        }
        // C Rank Check
        else if (times[2] < _timer && _timer < times[3])
        {
            _rankText.text = "C-Rank";
            _rankScore.SetFinalRank(Ranks.C);
        }
        // F Rank Check
        else
        {
            _rankText.text = "F-Rank";
            _rankScore.SetFinalRank(Ranks.F);
        }
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
