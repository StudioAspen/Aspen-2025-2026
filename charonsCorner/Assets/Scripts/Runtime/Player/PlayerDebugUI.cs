using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerDebugUI : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private TMP_Text superStateText;
        [SerializeField] private TMP_Text subStateText;
        [SerializeField] private TMP_Text speedText;

        private void LateUpdate()
        {
            ShowStates();
            speedText.text = $"Speed: {Utilities.FloatToString(playerController.CurrentSpeed, 2)}";
        }

        private void ShowStates()
        {
            string superStateName = playerController.StateMachine.CurrentState.GetType().Name;
            string subStateName = "None";
            if(playerController.StateMachine.CurrentState is SuperState<PlayerController> hierarchicalState && hierarchicalState.SubStateMachine.CurrentState != null)
                subStateName = hierarchicalState.SubStateMachine.CurrentState.GetType().Name;

            superStateText.text = $"Superstate: {superStateName}";
            subStateText.text = $"Substate: {subStateName}";
        }
    }
}
