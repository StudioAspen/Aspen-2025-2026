using DG.Tweening;
using System;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueOpener : MonoBehaviour
    {
        private DialogueManager _dialogueManager;

        [SerializeField] private DialogueOpenerSO _opener;

        [Header("Camera Config")]
        [SerializeField] private bool _useCamera = true;
        [SerializeField, ShowIf("_useCamera")] private CinemachineCamera _cinemachineCamera;
        [SerializeField, ShowIf("_useCamera")] private CinemachineCamera _endingDialogueCinemachineCamera;


        private void Awake()
        {
            _dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
        }

        private void OnDestroy()
        {
            if (_dialogueManager != null)
                _dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;
        }

        public void StartOpener()
        {
            StartOpener(_opener);
        }

        public void StartOpener(DialogueOpenerSO opener)
        {
            Debug.Log($"[DialogueOpener] Starting dialogue opener: {opener.name}");
            GameManager.Instance.ChangeGameState(GameState.Dialogue);
            _dialogueManager.StartDialogueOpener(opener);

            _dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;

            if (_useCamera)
                CameraManager.Instance.ChangeActiveCamera(_cinemachineCamera);
        }

        private void DialogueManager_OnDialogueEnded()
        {
            _dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;

            if(_useCamera)
                CameraManager.Instance.ChangeActiveCamera(_endingDialogueCinemachineCamera);
        }
    }
}
