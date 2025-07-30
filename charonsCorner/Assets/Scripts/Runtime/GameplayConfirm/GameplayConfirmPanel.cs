using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class GameplayConfirmPanel : UIPanel
    {
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private Button yesButton;
        [SerializeField] private TMP_Text yesButtonText;

        private protected override void Initialize()
        {
            
        }

        public void SetupContents(string warningContent, string yesContent, UnityAction yesButtonAction)
        {
            warningText.text = warningContent;
            yesButtonText.text = yesContent;

            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(yesButtonAction);

            // To make the content size fitter recalculate the layout immediately with the new text
            LayoutRebuilder.ForceRebuildLayoutImmediate(warningText.rectTransform);
        }

        public override void CloseUI()
        {
            GameManager.Instance.ChangeGameState(GameState.Gameplay);
        }
    }
}
