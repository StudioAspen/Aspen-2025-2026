using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Wwise requires a WwiseGlobal object to exist in every scene.
    /// This script attaches to a WwiseGlobal object in the Bootstrap scene to make it persistent.
    /// The WwiseGlobal object should not be in any other scene besides the Bootstrap scene.
    /// To call Wwise events, please serialize an AK.Wwise.Event type and call that instance's Post(gameObject) method.
    /// </summary>
    public class WwiseGlobalSingleton : Singleton<WwiseGlobalSingleton>
    {
        
    }
}
