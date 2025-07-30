using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TargetSceneSwitcher : MonoBehaviour
    {
        [SerializeField] private SceneReference targetScene;
        [SerializeField] private GameState stateAfterSwitch;

        public void SwitchScene()
        {
            GameManager.Instance.SwitchScenes(targetScene, stateAfterSwitch).Forget();
        }
    }
}
