using UnityEngine;
using TMPro;

public class PlayerStanceDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController; 
    [SerializeField] private TMP_Text infoText;


    void Update()
    {
        //Null Check:
        if (playerController == null || infoText == null) return;


        var stance = playerController._state.Stance;
        var velocity = playerController._state.Velocity;
        var acceleration = playerController._state.Acceleration;
        var speed = velocity.magnitude;

        infoText.text = $"Stance: {stance}\n" + $"Velocity: {velocity:F2}\n" + $"Speed: {speed:F2}\n";
    }
}
