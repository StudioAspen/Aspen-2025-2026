using UnityEngine;

public class PingPongAnimations : MonoBehaviour
{
    void Start()
    {
        foreach (AnimationClip clip in GetComponent<Animator>()
                     .runtimeAnimatorController.animationClips)
        {
            clip.wrapMode = WrapMode.PingPong;
        }
    }
}