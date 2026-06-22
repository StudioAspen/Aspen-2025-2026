using UnityEngine;

public class CharonBowlingSequence : MonoBehaviour
{
    [SerializeField] private Animator charonAnimator;
    [SerializeField] private Animator skullAnimator;
    [SerializeField] private Animator skull001Animator;
    [SerializeField] private Animator cameraAnimator;

    void Start()
    {
        PlayAll();
    }

    void PlayAll()
    {
        charonAnimator.Play("Armature|Bow Anim 1");     // your actual clip names
        skullAnimator.Play("Armature|SkullAction");
        skull001Animator.Play("Armature|Skull001Action");
        cameraAnimator.Play("Camera|Action");
    }
}