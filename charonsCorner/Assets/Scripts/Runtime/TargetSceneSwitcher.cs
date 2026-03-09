using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TargetSceneSwitcher : MonoBehaviour
    {
        [SerializeField, Required] private SceneReference _targetScene;
        [SerializeField] private GameState _stateAfterSwitch;

        [Button("Switch Scene", ButtonSizes.Large)]
        public void SwitchScene()
        {
            GameManager.Instance.SwitchScenes(_targetScene, _stateAfterSwitch).Forget();
        }
    }
}
