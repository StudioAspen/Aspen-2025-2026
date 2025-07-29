using DG.Tweening;
using NaughtyAttributes;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueOpener : MonoBehaviour
    {
        private DialogueManager dialogueManager;

        [SerializeField] private DialogueOpenerSO opener;

        [Header("Camera Config")]
        [SerializeField] private bool useCamera = true;
        [SerializeField, ShowIf("useCamera")] private CinemachineCamera cinemachineCamera;

        private void Awake()
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
        }

        private void OnDestroy()
        {
            if (dialogueManager != null)
                dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;
        }

        public void StartOpener()
        {
            GameManager.Instance.ChangeGameState(GameState.Dialogue);
            dialogueManager.StartDialogueOpener(opener);

            dialogueManager.OnDialogueEnded += DialogueManager_OnDialogueEnded;

            if (useCamera)
            {
                CameraManager.Instance.RegisterCamera(CameraManager.CameraType.Dialogue, cinemachineCamera);
                CameraManager.Instance.ChangeActiveCamera(CameraManager.CameraType.Dialogue);
            }
        }

        private void DialogueManager_OnDialogueEnded()
        {
            dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;

            if(useCamera)
                CameraManager.Instance.ChangeActiveCamera(CameraManager.CameraType.Player);
        }
    }
}
