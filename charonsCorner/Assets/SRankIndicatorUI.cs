using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using CharonsCorner.Runtime;
using System.Collections;
using Sirenix.OdinInspector;

public class SRankIndicatorUI : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [Header("Feedbacks")]
    [SerializeField] private MMF_Player _entranceFeedback;
    [SerializeField] private MMF_Player _exitFeedback;
    [SerializeField] private MMF_Player _incrementFeedback;

    [Header("UI Objects")]
    [SerializeField] private TextMeshProUGUI _numberText;

    [Header("Events")]
    [SerializeField] private string _incrementEventName = "SRankIncrementEvent";
    [SerializeField] private string _enterEventName = "SRankEnterEvent";
    [SerializeField] private string _exitEventName = "SRankExitEvent";

    [Header("Settings")]
    [SerializeField] private float _incrementDelay = 1f;

    [Button("Trigger Increment Event")]
    public void TestIncrementEvent()
    {
        MMGameEvent.Trigger(_incrementEventName);
    }

    [Button("Trigger Enter Event")]
    public void TestEnterEvent()
    {
        MMGameEvent.Trigger(_enterEventName);
    }

    [Button("Trigger Exit Event")]
    public void TestExitEvent()
    {
        MMGameEvent.Trigger(_exitEventName);
    }

    public void IncrementSequence()
    {
        int currentCount = FlagManager.Get(ProgressFlag.SRankCount);
        _numberText.text = $" {currentCount - 1}/7";
        
        if (_entranceFeedback != null)
        {
            _entranceFeedback.PlayFeedbacks();
        }

        StartCoroutine(IncrementSequenceCoroutine());
    }

    private IEnumerator IncrementSequenceCoroutine()
    {
        yield return new WaitForSeconds(_incrementDelay);
        
        if (_incrementFeedback != null)
        {
            _incrementFeedback.PlayFeedbacks();
        }
    }

    public void Increment()
    {
        int currentCount = FlagManager.Get(ProgressFlag.SRankCount);
        _numberText.text = $" {currentCount}/7";
    }

    public void SRankEnterNormal()
    {
        int currentCount = FlagManager.Get(ProgressFlag.SRankCount);
        _numberText.text = $" {currentCount}/7";

        if (_entranceFeedback != null)
        {
            _entranceFeedback.PlayFeedbacks();
        }
    }

    public void SRankExitNormal()
    {
        if (_exitFeedback != null)
        {
            _exitFeedback.PlayFeedbacks();
        }
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == _incrementEventName)
        {
            IncrementSequence();
        }
        else if (gameEvent.EventName == _enterEventName)
        {
            SRankEnterNormal();
        }
        else if (gameEvent.EventName == _exitEventName)
        {
            SRankExitNormal();
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }
}
