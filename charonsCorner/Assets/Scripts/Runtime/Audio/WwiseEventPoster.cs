using System;
using NaughtyAttributes;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This component allows you to specify a Wwise event and post it through a public method.
    /// </summary>
    public class WwiseEventPoster : MonoBehaviour
    {
        [SerializeField] private AK.Wwise.Event _event = new();

        /// <summary>
        /// Posts the serialized event through the gameObject this script is attached to.
        /// Doesn't work if the soundbank associated with the event is not loaded.
        /// </summary>
        [Button("Post")]
        public void PostEvent()
        {
            _event.Post(gameObject);
        }
    }
}
