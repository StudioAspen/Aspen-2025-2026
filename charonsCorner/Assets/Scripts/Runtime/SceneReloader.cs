using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SceneReloader : MonoBehaviour
    {
        [SerializeField] private GameState _stateAfterSwitch;
        
        [Button("Reload Scene", ButtonSizes.Large)]
        public void ReloadScene()
        {
            GameManager.Instance.ReloadScene(_stateAfterSwitch);
        }
    }
}