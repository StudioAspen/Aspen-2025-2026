using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerDebugUI : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private TMP_Text _superStateText;
        [SerializeField] private TMP_Text _subStateText;
        [SerializeField] private TMP_Text _speedText;

        private void LateUpdate()
        {
            ShowStates();
            _speedText.text = $"Speed: {Utilities.FloatToString(_playerController.CurrentSpeed, 2)}";
        }

        private void ShowStates()
        {
            string superStateName = _playerController.StateMachine.CurrentState.GetType().Name;
            string subStateName = "None";
            if(_playerController.StateMachine.CurrentState is SuperState<PlayerController> hierarchicalState && hierarchicalState.SubStateMachine.CurrentState != null)
                subStateName = hierarchicalState.SubStateMachine.CurrentState.GetType().Name;

            _superStateText.text = $"Superstate: {superStateName}";
            _subStateText.text = $"Substate: {subStateName}";
        }
    }
}
