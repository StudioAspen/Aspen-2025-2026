using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

public class GlobalFeelManager : MonoBehaviour
{
    public static GlobalFeelManager Instance;

    [System.Serializable]
    public class GlobalFeelEffect
    {
        public string Name;
        public MMF_Player player;
    }

    public List<GlobalFeelEffect> effects = new List<GlobalFeelEffect>();
    private Dictionary<string, MMF_Player> lookup;

    void Awake()
    {
        Instance = this;
        BuildLookUp();
    }




    /// <summary>
    /// Adds effects in the list to a dictionary,making it easy to access 
    /// </summary>
    void BuildLookUp()
    {
        lookup = new Dictionary<string, MMF_Player>();
        foreach(GlobalFeelEffect effect in effects)
        {
            if(effect.Name != null && !lookup.ContainsKey(effect.Name))
                lookup.Add(effect.Name, effect.player);       
            
        }
    }

    /// <summary>
    /// Plays the effect in the list matching the given name
    /// </summary>
    /// <param name="name">name/id of given to play the matching mmf player</param>
    public void Play(string name)
    {
        
        if (lookup.TryGetValue(name, out MMF_Player player))
            player.PlayFeedbacks();
        
    }

    public bool has(string name) => lookup.ContainsKey(name);

    // HOW TO USE 

    // if you need local and global
    // feel.PlayUnified("Jump");

    // if you only need local 
    // feel.PlayLocal("Jump");

}
