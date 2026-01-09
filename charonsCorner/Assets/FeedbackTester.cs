using UnityEngine;
using MoreMountains.Feedbacks;

public class FeedbackTester : MonoBehaviour
{
    [Header("Feedbacks to test (Assigned to keys 1-9)")]
    public MMF_Player[] Feedbacks = new MMF_Player[9];

    private void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            // KeyCode.Alpha1 is 49, Alpha2 is 50, etc.
            KeyCode key = KeyCode.Alpha1 + i;
            if (Input.GetKeyDown(key))
            {
                PlayFeedback(i);
        }
    }

    private void PlayFeedback(int index)
    {
        if (index < 0 || index >= Feedbacks.Length) return;
        
        MMF_Player player = Feedbacks[index];
        if (player != null)
        {
            Debug.Log($"[FeedbackTester] Playing feedback at index {index} (Key {index + 1})");
            player.Initialization();
            player.PlayFeedbacks();
        }
        else
        {
            Debug.LogWarning($"[FeedbackTester] No feedback assigned for index {index} (Key {index + 1})");
        }
    }
}
