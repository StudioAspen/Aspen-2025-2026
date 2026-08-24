using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

public class BallSwap : MonoBehaviour, MMEventListener<MMGameEvent>
{
    public enum BallState { Ball, Skull, Yarn }

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player _ballFeedback;
    [SerializeField] private MMF_Player _skullFeedback;
    [SerializeField] private MMF_Player _yarnFeedback;

    [Header("Settings")]
    [SerializeField] private string _triggerEventName;
    [SerializeField] private BallState _currentState = BallState.Ball;

    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == _triggerEventName)
        {
            SwitchToNewState();
        }
    }

    private void SwitchToNewState()
    {
        BallState nextState;
        
        // Ensure we don't pick the current state
        do
        {
            nextState = (BallState)Random.Range(0, 3);
        } while (nextState == _currentState);

        _currentState = nextState;

        PlayCurrentStateFeedback();
    }

    private void PlayCurrentStateFeedback()
    {
        switch (_currentState)
        {
            case BallState.Ball:
                if (_ballFeedback != null) _ballFeedback.PlayFeedbacks();
                break;
            case BallState.Skull:
                if (_skullFeedback != null) _skullFeedback.PlayFeedbacks();
                break;
            case BallState.Yarn:
                if (_yarnFeedback != null) _yarnFeedback.PlayFeedbacks();
                break;
        }
    }
}
