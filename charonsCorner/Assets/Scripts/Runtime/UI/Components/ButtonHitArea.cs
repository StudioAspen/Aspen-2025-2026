using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class ButtonHitArea : MonoBehaviour
    {
        private void Start()
        {
            this.GetComponent<Image>().alphaHitTestMinimumThreshold = 0.1f;
        }
    }
}
