using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent (typeof(Collider))]
    public class DeathBox : MonoBehaviour
    {
        public static event Action<DeathBox> OnPlayerDeath;

        private void OnTriggerEnter(Collider other)
        {
            // notify player death handler
            if (!other.CompareTag("Player")) return;
            Activate();
        }

        [Button("Activate", ButtonSizes.Large)]
        public void Activate()
        {
            OnPlayerDeath?.Invoke(this);
        }
    }
}
