using UnityEngine;

namespace CharonsCorner.Runtime.VFX
{
    [CreateAssetMenu(fileName = "FlickerSettings", menuName = "CharonsCorner/VFX/FlickerSettings")]
    public class FlickerSettingsSO : ScriptableObject
    {
        [SerializeField] private float _flickerRate = 0.05f;
        [SerializeField] private float _flickerLength = 0.5f;

        public float FlickerRate
        {
            get => _flickerRate;
            set => _flickerRate = value;
        }

        public float FlickerLength
        {
            get => _flickerLength;
            set => _flickerLength = value;
        }
    }
}
