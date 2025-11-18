using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

public class LocalFeelManager : MonoBehaviour
{
    [System.Serializable]
    public class FeelEntry
    {
        public string name;
        public MMF_Player player;
        public bool alsoTriggerGlobal = false;
    }

    public List<FeelEntry> effects = new List<FeelEntry>();

    private Dictionary<string, FeelEntry> lookup;

    void Awake()
    {
        BuildLookup();
    }


    void BuildLookup()
    {
        lookup = new Dictionary<string, FeelEntry>();

        foreach (var entry in effects)
        {
            if (entry.player == null)
            {
                Debug.LogWarning($"[LocalFeelManager] Effect '{entry.name}' has no assigned player on {gameObject.name}");
                continue;
            }

            if (!lookup.ContainsKey(entry.name))
                lookup.Add(entry.name, entry);
        }
    }

    // for local player 
    
    public void PlayerLocal(string id)
    {
        if (lookup.TryGetValue(id, out var entry))
        {
            entry.player.PlayFeedbacks();
        }
    }


    // for global player

    public void PlayUnified(string id)
    {
        PlayerLocal(id);

        if(lookup.TryGetValue(id, out var entry))
        {
            if (entry.alsoTriggerGlobal && GlobalFeelManager.Instance != null)
            {
                GlobalFeelManager.Instance.Play(id);
            }
        }
    }


    public bool Has(string id) => lookup.ContainsKey(id);

    


}
