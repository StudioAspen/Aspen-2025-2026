using UnityEngine;
using TMPro;

public class PlayerStanceDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController; 
    [SerializeField] private TMP_Text stanceText; 

    void Update()
    {
        //Null Check:
        if (playerController == null || stanceText == null) return;

        // Get the current stance from the player controller
        var stance = playerController._state.Stance;
        stanceText.text = $"Stance: {stance}";
    }
}
