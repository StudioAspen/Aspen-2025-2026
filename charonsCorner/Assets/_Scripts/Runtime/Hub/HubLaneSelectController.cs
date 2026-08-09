using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharonsCorner.Runtime;
using Sirenix.OdinInspector;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class HubLaneSelectController : MonoBehaviour
    {
        [SerializeField] private InputInteraction _inputInteraction;
        [ShowInInspector, ReadOnly] public bool IsSelectingLevel { get; private set; }
        
        public UnityEvent OnLevelSelectStarted = new UnityEvent();
        public UnityEvent OnLevelSelectEnded = new UnityEvent();
        
        [Button("Start Level Select", ButtonSizes.Large)]
        public void StartLevelSelect()
        {
            if (IsSelectingLevel) // Can't start if already selecting
                return;
            
            IsSelectingLevel = true;
            
            if (_inputInteraction != null)
                _inputInteraction.Appear();
            
            OnLevelSelectStarted.Invoke();
        }

        [Button("End Level Select", ButtonSizes.Large)]
        public void EndLevelSelect()
        {
            if (!IsSelectingLevel) // Can't end selection if not selecting
                return;
            
            IsSelectingLevel = false;
            
            if (_inputInteraction != null)
                _inputInteraction.Disappear();
            
            OnLevelSelectEnded.Invoke();
        }
    }
}
