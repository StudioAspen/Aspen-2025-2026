using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TargetSceneSwitcher : MonoBehaviour
    {
        [SerializeField] private SceneReference _targetScene;
        [SerializeField] private GameState _stateAfterSwitch;

        public void SwitchScene()
        {
            GameManager.Instance.SwitchScenes(_targetScene, _stateAfterSwitch).Forget();
        }
    }
}
