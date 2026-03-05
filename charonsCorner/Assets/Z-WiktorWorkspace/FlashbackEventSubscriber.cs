using UnityEngine;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorCore.Typing;
using MoreMountains.Feedbacks;
using Eflatun.SceneReference;
using CharonsCorner.Runtime;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;

public class FlashbackEventSubscriber : MonoBehaviour
{
    [Header("Level Scenes")]
    [SerializeField] private SceneReference level0;
    [SerializeField] private SceneReference level1;
    [SerializeField] private SceneReference level2;
    [SerializeField] private SceneReference level3;
    [SerializeField] private SceneReference level4;
    [SerializeField] private SceneReference level5;
    [SerializeField] private SceneReference level6;
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private CinemachineCamera camToBowley;
    [SerializeField] private CinemachineCamera camToCharon;
    
    private void SwitchToLevel(SceneReference scene)
    {
        if (scene != null && !string.IsNullOrEmpty(scene.Name))
        {
            GameManager.Instance.SwitchScenes(scene, GameState.Gameplay).Forget();
        }
        else
        {
            Debug.LogWarning($"[FlashbackEventSubscriber] Scene is not assigned or invalid.");
        }
    }
    
// inside your script
    [SerializeField] TypewriterComponent typewriter;

// adds and removes callbacks
    void OnEnable() => typewriter.onMessage.AddListener(OnMessage);
    void OnDisable() => typewriter.onMessage.RemoveListener(OnMessage);

// does stuff based on the received marker
    void OnMessage(EventMarker marker)
    {
        switch (marker.name)
        {
            // once the typewriter meets the "<?something>" tag
        
            case "GoToLevel0":
                Debug.Log("GoToLevel0");
                SwitchToLevel(level0);
                break;
            case "GoToLevel1":
                SwitchToLevel(level1);
                break;
            case "GoToLevel2":
                SwitchToLevel(level2);
                break;
            case "GoToLevel3":
                SwitchToLevel(level3);
                break;
            case "GoToLevel4":
                SwitchToLevel(level4);
                break;
            case "GoToLevel5":
                SwitchToLevel(level5);
                break;
            case "GoToLevel6":
                SwitchToLevel(level6);
                break;
            case "CamToBowley":
                cameraSwitcher.SwitchCamera(camToBowley);
                break;
            case "CamToCharon":
                cameraSwitcher.SwitchCamera(camToCharon);
                break;
        }
    }
}
