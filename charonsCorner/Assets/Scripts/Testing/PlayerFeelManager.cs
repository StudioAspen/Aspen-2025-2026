using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
public class PlayerFeelManager : MonoBehaviour
{
    [Header("FEEL Players")]
    [SerializeField] private MMF_Player damageFeedBack;
    [SerializeField] private MMF_Player attackFeedBack;
    [SerializeField] private MMF_Player jumpFeedBack;
    [SerializeField] private MMF_Player ScaleFeedBack;

    // Source - https://stackoverflow.com/a
    // Posted by Antnio Pedro Gonalves Ferreira
    // Retrieved 2025-11-17, License - CC BY-SA 4.0

    public static List<MMF_Player> mmfPlayerList = new List<MMF_Player>();



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
