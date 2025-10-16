using UnityEngine;
using System.Collections.Generic;

namespace CharonsCorner.Runtime
{
    public class BigStateObjectManager : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private List<GameObject> bigStateObjects = new List<GameObject>();
        [SerializeField] private List<Renderer> bigStateRenderers = new List<Renderer>();
        [SerializeField] private Material transparentMaterial;
        [SerializeField] private Material visibleMaterial;

        private void Awake()
        {
            if (playerController == null)
                playerController = Object.FindFirstObjectByType<PlayerController>();

            if (playerController != null)
                playerController.OnBigStateChanged += HandleBigStateChanged;
        }

        private void OnDestroy()
        {
            if (playerController != null)
                playerController.OnBigStateChanged -= HandleBigStateChanged;
        }

        private void HandleBigStateChanged(bool isBig)
        {
            // Enable/disable colliders
            foreach (var obj in bigStateObjects)
            {
                if (obj != null)
                {
                    var collider = obj.GetComponent<Collider>();
                    if (collider != null)
                        collider.enabled = isBig;
                }
            }

            // Swap materials
            foreach (var rend in bigStateRenderers)
            {
                if (rend != null)
                {
                    var mats = rend.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++)
                        mats[i] = isBig ? visibleMaterial : transparentMaterial;
                    rend.sharedMaterials = mats;
                }
            }
        }
    }
}