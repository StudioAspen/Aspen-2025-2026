using UnityEngine;
using MoreMountains.Feedbacks;

public class ShakeCameraManager : MonoBehaviour
{
    public static ShakeCameraManager Instance;

    public MMF_Player shakePlayer;

    private MMF_CameraShake shakeFb;

    void Awake()
    {
        Instance = this;

        foreach (var feedBack in shakePlayer.FeedbacksList)
        {
            if (feedBack is MMF_CameraShake s)
            {
                shakeFb = s;
            }
        }
    }



    public void Shake(float intensity, float duration)
    {
        shakeFb.CameraShakeProperties.Amplitude = intensity;
        shakeFb.CameraShakeProperties.Duration = duration;
        shakePlayer.PlayFeedbacks();
    }


    // call from anywhere using 
    // ShakeCameraManager.Instance.Shake(2f,0.5f);

}
