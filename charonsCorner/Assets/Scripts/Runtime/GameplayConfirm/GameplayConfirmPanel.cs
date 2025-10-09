using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class GameplayConfirmPanel : UIPanel
    {
        [SerializeField] private TMP_Text _warningText;
        [SerializeField] private Button _yesButton;
        [SerializeField] private TMP_Text _yesButtonText;

        private protected override void Initialize()
        {
            
        }

        public void SetupContents(string warningContent, string yesContent, UnityAction yesButtonAction)
        {
            _warningText.text = warningContent;
            _yesButtonText.text = yesContent;

            _yesButton.onClick.RemoveAllListeners();
            _yesButton.onClick.AddListener(yesButtonAction);

            // To make the content size fitter recalculate the layout immediately with the new text
            LayoutRebuilder.ForceRebuildLayoutImmediate(_warningText.rectTransform);
        }

        public override void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
}
