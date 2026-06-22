using Steamworks;
using UnityEngine;

public partial class SteamManager
{
    /// <summary>
    /// Unlocks a Steam achievement by its ID.
    /// Make sure the achievement ID is correctly defined in the Steamworks dashboard.
    /// </summary>
    public void UnlockAchievement(string achievementId)
    {
        if (!Initialized)
        {
            Debug.LogWarning("SteamManager is not initialized. Cannot unlock achievement: " + achievementId);
            return;
        }

        if (SteamUserStats.SetAchievement(achievementId))
        {
            SteamUserStats.StoreStats();
            Debug.Log($"Achievement unlocked: {achievementId}");
        }
        else
        {
            Debug.LogWarning($"Failed to set achievement: {achievementId}");
        }
    }

    /// <summary>
    /// Gets whether a Steam achievement is unlocked by its ID.
    /// Make sure the achievement ID is correctly defined in the Steamworks dashboard.
    /// </summary>
    public bool IsAchievementUnlocked(string achievementId)
    {
        if (!Initialized)
            return false;

        bool isUnlocked = false;
        SteamUserStats.GetAchievement(achievementId, out isUnlocked);
        return isUnlocked;
    }
}