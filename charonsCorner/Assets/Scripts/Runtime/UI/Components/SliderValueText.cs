using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(TMP_Text))]
    public class SliderValueText : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        private TMP_Text valueText;

        private void Awake()
        {
            valueText = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            slider.onValueChanged.AddListener(Slider_OnValueChanged);

            FormatValueText(slider.value);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(Slider_OnValueChanged);
        }

        private void Slider_OnValueChanged(float value)
        {
            FormatValueText(value);
        }

        private void FormatValueText(float value)
        {
            if(slider.wholeNumbers)
                valueText.text = Mathf.RoundToInt(value).ToString(); // Round to nearest whole number
            else
                valueText.text = value.ToString("F2"); // Format to 2 decimal places
        }
    }
}
