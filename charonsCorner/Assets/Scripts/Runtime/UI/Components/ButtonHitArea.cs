using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class ButtonHitArea : MonoBehaviour
    {
        /// <summary>
        /// Configures the UI hit-test threshold for the Image component on this GameObject.
        /// </summary>
        /// <remarks>
        /// Sets the Image's <c>alphaHitTestMinimumThreshold</c> to 0.1 to refine click/touch hit detection.
        /// Requires an <c>Image</c> component to be present on the same GameObject.
        /// </remarks>
        private void Start()
        {
            this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }
    }
}