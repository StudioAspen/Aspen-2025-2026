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

    [SerializeField] private List<FeelEntry> effects = new List<FeelEntry>();
    private Dictionary<string, FeelEntry> lookup;

    void Awake()
    {
        BuildLookup();
    }

    
    /// <summary>
    /// Adds effects in the list to a dictionary,making it easy to access 
    /// </summary>
    void BuildLookup()
    {
        lookup = new Dictionary<string, FeelEntry>();

        foreach (FeelEntry entry in effects)
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

    /// <summary>
    /// Plays only the local MMFplayer, that matches the name given
    /// </summary>
    /// <param name="name">Name given to run the matching mmf player in the list</param>
    public void PlayerLocal(string name)
    {
        if (lookup.TryGetValue(name, out FeelEntry entry))
        {
            entry.player.PlayFeedbacks();
        }
    }


    /// <summary>
    /// Plays both local and global mmf players that match the given name
    /// if there is no matching global mmf player it simply skips 
    /// </summary>
    /// <param name="name">Name given to run the matching mmf player in the list</param>
    public void PlayUnified(string name)
    {
        PlayerLocal(name);

        if(lookup.TryGetValue(name, out FeelEntry entry))
        {
            if (entry.alsoTriggerGlobal == true && GlobalFeelManager.Instance != null)
                GlobalFeelManager.Instance.Play(name);
             
        }
    }


    public bool Has(string name) => lookup.ContainsKey(name);

    


}
