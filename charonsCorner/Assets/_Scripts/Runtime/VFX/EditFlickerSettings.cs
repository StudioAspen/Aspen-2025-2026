using UnityEngine;

namespace CharonsCorner.Runtime.VFX
{
    public class EditFlickerSettings : MonoBehaviour
    {
        [SerializeField] private FlickerSettingsSO _flickerSettings;

        public void SetFlickerRate(float rate)
        {
            if (_flickerSettings != null)
            {
                _flickerSettings.FlickerRate = rate;
            }
        }

        public void SetFlickerLength(float length)
        {
            if (_flickerSettings != null)
            {
                _flickerSettings.FlickerLength = length;
            }
        }
    }
}
