using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class InputEnabler : MonoBehaviour
    {
        public void EnableGameplayInput()
        {
            InputManager.Instance.EnablePlayerActions();
        }

        public void EnableUIInput()
        {
            InputManager.Instance.EnableUIActions();
        }

        public void DisableAllInput()
        {
            InputManager.Instance.DisableAllActions();
        }
    }
}