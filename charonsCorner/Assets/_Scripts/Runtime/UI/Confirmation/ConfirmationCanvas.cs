using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharonsCorner.Runtime
{
    public class ConfirmationCanvas : Singleton<ConfirmationCanvas>
    {
        [SerializeField] private UIPanel _confirmationPanel;
    
        [Header("UI References")]
        [SerializeField] private TMP_Text _confirmationText;
        [SerializeField] private Button _yesButton;
        [SerializeField] private TMP_Text _yesButtonText;
        [SerializeField] private Button _noButton;
        [SerializeField] private TMP_Text _noButtonText;
        
        public void ShowConfirmation(string confirmationText, string yesButtonText, string noButtonText, Action yesButtonAction,
            Action noButtonAction)
        {
            _confirmationText.text = confirmationText;
        
            _yesButtonText.text = yesButtonText;
            _noButtonText.text = noButtonText;
        
            _yesButton.onClick.RemoveAllListeners();
            _yesButton.onClick.AddListener(() =>
            {
                CloseConfirmation();
                if(yesButtonAction != null)
                    yesButtonAction();
            });
        
            _noButton.onClick.RemoveAllListeners();
            _noButton.onClick.AddListener(() =>
            {
                CloseConfirmation();
                if(noButtonAction != null)
                    noButtonAction();
            });
            
            UIPanel.Focus(_confirmationPanel);
        }

        public void CloseConfirmation()
        {
            _confirmationPanel.BackOrClose();
        }
    }
}