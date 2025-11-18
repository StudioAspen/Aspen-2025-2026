using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

public class GlobalFeelManager : MonoBehaviour
{
    public static GlobalFeelManager Instance;

    [System.Serializable]
    public class CameraEffect
    {
        public string id;
        public MMF_Player player;
    }



    public List<CameraEffect> effects = new List<CameraEffect>();
    private Dictionary<string, MMF_Player> lookup;

    void Awake()
    {
        Instance = this;
        BuildLookUp();
    }

    void BuildLookUp()
    {
        lookup = new Dictionary<string, MMF_Player>();

        foreach(var effect in effects)
        {
            if(effect.id == null && !lookup.ContainsKey(effect.id))
                lookup.Add(effect.id, effect.player);
        }


    }


    public void Play(string id)
    {
        if (lookup.TryGetValue(id, out var player))
            player.PlayFeedbacks();
    }

    public bool has(string id) => lookup.ContainsKey(id);

    // HOW TO USE 

    // if you need local and global
    // feel.PlayUnified("Jump");

    // if you only need local 
    // feel.PlayLocal("Jump");

}
