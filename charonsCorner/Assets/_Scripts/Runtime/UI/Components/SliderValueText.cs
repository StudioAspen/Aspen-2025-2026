using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(TMP_Text))]
    public class SliderValueText : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        private TMP_Text _valueText;

        private void Awake()
        {
            _valueText = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            _slider.onValueChanged.AddListener(Slider_OnValueChanged);

            FormatValueText(_slider.value);
        }

        private void OnDestroy()
        {
            if(_slider != null)
                _slider.onValueChanged.RemoveListener(Slider_OnValueChanged);
        }

        private void Slider_OnValueChanged(float value)
        {
            FormatValueText(value);
        }

        private void FormatValueText(float value)
        {
            if(_slider.wholeNumbers)
                _valueText.text = Mathf.RoundToInt(value).ToString(); // Round to nearest whole number
            else
                _valueText.text = value.ToString("F2"); // Format to 2 decimal places
        }
    }
}
