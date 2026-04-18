using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorCore.Typing;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
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
    [SerializeField] private SceneReference hubScene;
    [SerializeField] private CameraSwitcher cameraSwitcher;
    [SerializeField] private CinemachineCamera camToBowley;
    [SerializeField] private CinemachineCamera camToBowlingAlley;
    [SerializeField] private CinemachineCamera camToBowlingAlleyPan;
    [SerializeField] private CinemachineCamera camToCharon;
    [SerializeField] private CinemachineCamera camCharonLooksAtPlayer;
    [SerializeField] private CinemachineCamera camPlayerCloseup;
    [SerializeField] private CinemachineCamera camToFadeOut;
    
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    
    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeAmplitude = 1f;
    [SerializeField] private float shakeFrequency = 1f;
    [SerializeField] private MMChannelModes shakeChannelMode = MMChannelModes.Int;
    [SerializeField] private int shakeChannelInt = 0;
    [SerializeField] private MMChannel shakeChannelDefinition = null;

    [Header("Bowley Shake")]
    [SerializeField] private MMRotationShaker bowleyRotationShaker;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player movePlayerFeedback;
    [SerializeField] private MMF_Player playerScaredFeedback;
    [SerializeField] private MMF_Player bowlingLightsFeedback;
    [SerializeField] private MMF_Player bowlingPanFeedback;
    [SerializeField] private MMF_Player startBowlingAnimationFeedback;

    [Header("Text Color Settings")]
    [SerializeField] private TMP_Text flashbackText;
    [SerializeField] private Color charonColor = Color.white;
    [SerializeField] private Color bowleyColor = Color.white;
    
    public void SwitchToLevel(SceneReference scene)
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

    void OnEnable()
    {
        typewriter.onMessage.AddListener(OnMessage);
        FlashbackText.OnNextLineRequested += StopBowleyShaking;
        FlashbackText.OnDialogueFinished += StopBowleyShaking;
    }

    void OnDisable()
    {
        typewriter.onMessage.RemoveListener(OnMessage);
        FlashbackText.OnNextLineRequested -= StopBowleyShaking;
        FlashbackText.OnDialogueFinished -= StopBowleyShaking;
    }

    private void StopBowleyShaking()
    {
        if (bowleyRotationShaker != null)
        {
            bowleyRotationShaker.Stop();
        }
    }

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
            case "GoToHubScene":
                SwitchToLevel(hubScene);
                break;
            case "CamToBowley":
                cameraSwitcher.SwitchCamera(camToBowley);
                if (flashbackText != null) flashbackText.color = bowleyColor;
                MMGameEvent.Trigger("BowleyTalk");
                break;
            case "CamToBowlingAlley":
                cameraSwitcher.SwitchCamera(camToBowlingAlley);
                break;
            case "CameraToBowlingAlleyPan":
                cameraSwitcher.SwitchCamera(camToBowlingAlleyPan);
                break;
            case "CamToCharon":
                cameraSwitcher.SwitchCamera(camToCharon);
                if (flashbackText != null) flashbackText.color = charonColor;
                MMGameEvent.Trigger("CharonTalk");
                break;
            case "CharonLooksAtPlayer":
                cameraSwitcher.SwitchCamera(camCharonLooksAtPlayer);
                if (flashbackText != null) flashbackText.color = charonColor;
                MMGameEvent.Trigger("CharonTalk");
                break;
            case "CameraPlayerCloseup":
                cameraSwitcher.SwitchCamera(camPlayerCloseup);
                break;
            case "CamToFadeOut":
                cameraSwitcher.SwitchCamera(camToFadeOut);
                break;
            case "ShakeCamera":
                MMCameraShakeEvent.Trigger(shakeDuration, shakeAmplitude, shakeFrequency, 0f, 0f, 0f, false, new MMChannelData(shakeChannelMode, shakeChannelInt, shakeChannelDefinition));
                break;
            case "MovePlayerFeedback":
                if (movePlayerFeedback != null) movePlayerFeedback.PlayFeedbacks();
                break;
            case "PlayerScaredFeedback":
                if (playerScaredFeedback != null) playerScaredFeedback.PlayFeedbacks();
                break;
            case "BowlingLights":
                if (bowlingLightsFeedback != null) bowlingLightsFeedback.PlayFeedbacks();
                break;
            case "BowlingPan":
                if (bowlingPanFeedback != null) bowlingPanFeedback.PlayFeedbacks();
                break;
            case "StartBowlingAnimation":
                if (startBowlingAnimationFeedback != null) startBowlingAnimationFeedback.PlayFeedbacks();
                break;
            case "BowleyStartShaking":
            {
                StopBowleyShaking();
                float shakeSpeed = 0f;
                float range = 0f;
                int direction = 0;
                if (marker.parameters.Length > 0) float.TryParse(marker.parameters[0], NumberStyles.Float, CultureInfo.InvariantCulture, out shakeSpeed);
                if (marker.parameters.Length > 1) float.TryParse(marker.parameters[1], NumberStyles.Float, CultureInfo.InvariantCulture, out range);
                if (marker.parameters.Length > 2) int.TryParse(marker.parameters[2], out direction);

                Vector3 mainDirection = Vector3.zero;
                if (direction == 1) mainDirection = Vector3.right;
                else if (direction == 2) mainDirection = Vector3.up;
                else if (direction == 3) mainDirection = Vector3.forward;

                if (bowleyRotationShaker != null)
                {
                    bowleyRotationShaker.ShakeSpeed = shakeSpeed;
                    bowleyRotationShaker.ShakeRange = range;
                    bowleyRotationShaker.ShakeMainDirection = mainDirection;
                    bowleyRotationShaker.Play();
                }
                break;
            }
            case "BowleyStopShaking":
                StopBowleyShaking();
                break;
            case "CamSwitchSpeed":
                if (marker.parameters.Length > 0 && float.TryParse(marker.parameters[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float speed))
                {
                    cameraSwitcher.BlendDuration = speed;
                }
                break;
            case "FadeToBlack":
                if (marker.parameters.Length > 0 && float.TryParse(marker.parameters[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float duration))
                {
                    FadeToBlack(duration).Forget();
                }
                break;
        }
    }

    private async UniTask FadeToBlack(float duration)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("[FlashbackEventSubscriber] fadeImage is not assigned.");
            return;
        }

        float elapsedTime = 0f;
        Color color = fadeImage.color;
        float startAlpha = color.a;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 1f, elapsedTime / duration);
            fadeImage.color = color;
            await UniTask.Yield();
        }

        color.a = 1f;
        fadeImage.color = color;
    }
}
