using DG.Tweening;
using NaughtyAttributes;
using System;
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
            GameManager.Instance.ChangeGameState(GameState.Dialogue);
            _dialogueManager.StartDialogueOpener(_opener);

            _dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;

            if (_useCamera)
                CameraManager.Instance.ChangeActiveCamera(_cinemachineCamera);
        }

        private void DialogueManager_OnDialogueEnded()
        {
            _dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;

            if(_useCamera)
                CameraManager.Instance.ResetActiveCamera();
        }
    }
}
