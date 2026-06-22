using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(Selectable))]
    public class UISelectableAudio : MonoBehaviour, ISelectHandler, IPointerEnterHandler, ISubmitHandler, IPointerClickHandler
    {
        private Selectable _selectable;
    
        [SerializeField] private FloatRange _hoverPitchRange = new FloatRange(0.9f, 1.1f); 

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!_selectable.interactable)
                return;
        
            Debug.Assert(AudioManager.Instance != null);
            // AudioManager.Instance.Play("SFX_Hover", AudioManager.MixerTarget.UI, null, _hoverPitchRange.RandomValue());
        }
    
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnSelect(eventData);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            
            // AudioManager.Instance.Play("SFX_Click", AudioManager.MixerTarget.UI, null, 1f, true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnSubmit(eventData);
        }
    }
}