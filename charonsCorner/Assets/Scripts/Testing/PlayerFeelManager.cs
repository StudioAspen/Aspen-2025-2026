using UnityEngine;
using MoreMountains.Feedbacks;
public class PlayerFeelManager : MonoBehaviour
{
    [Header("FEEL Players")]
    [SerializeField] private MMF_Player damageFeedBack;
    [SerializeField] private MMF_Player attackFeedBack;
    [SerializeField] private MMF_Player jumpFeedBack;
    [SerializeField] private MMF_Player ScaleFeedBack;



    public void PlayDamage() => damageFeedBack?.PlayFeedbacks();
    public void PlayAttack() => attackFeedBack?.PlayFeedbacks();
    public void PlayJump() => jumpFeedBack?.PlayFeedbacks();
    public void PlayScale() => ScaleFeedBack?.PlayFeedbacks();


    // Example use

    // void jump()
    // {
    //     feel.PlayJump();
    // }

}
