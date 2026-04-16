using UnityEngine;
using System.Globalization;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorCore.Typing;
using MoreMountains.Feedbacks;
using Eflatun.SceneReference;
using CharonsCorner.Runtime;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using TMPro;

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
    
    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmplitude = 1f;
    [SerializeField] private float shakeFrequency = 1f;
    [SerializeField] private MMChannelModes shakeChannelMode = MMChannelModes.Int;
    [SerializeField] private int shakeChannelInt = 0;
    [SerializeField] private MMChannel shakeChannelDefinition = null;

    [Header("Text Color Settings")]
    [SerializeField] private TMP_Text flashbackText;
    [SerializeField] private Color charonColor = Color.white;
    [SerializeField] private Color bowleyColor = Color.white;
    
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
                if (flashbackText != null) flashbackText.color = bowleyColor;
                break;
            case "CamToCharon":
                cameraSwitcher.SwitchCamera(camToCharon);
                if (flashbackText != null) flashbackText.color = charonColor;
                break;
            case "ShakeCamera":
                MMCameraShakeEvent.Trigger(shakeDuration, shakeAmplitude, shakeFrequency, 0f, 0f, 0f, false, new MMChannelData(shakeChannelMode, shakeChannelInt, shakeChannelDefinition));
                break;
            case "CamSwitchSpeed":
                if (marker.parameters.Length > 0 && float.TryParse(marker.parameters[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float speed))
                {
                    cameraSwitcher.BlendDuration = speed;
                }
                break;
        }
    }
}
