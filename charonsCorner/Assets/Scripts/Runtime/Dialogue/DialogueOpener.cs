using DG.Tweening;
using NaughtyAttributes;
using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DialogueOpener : MonoBehaviour
    {
        private DialogueManager dialogueManager;

        [SerializeField] private DialogueOpenerSO opener;

        [Header("Camera Config")]
        [SerializeField] private bool useCamera = true;
        [SerializeField, ShowIf("useCamera")] private Transform mainCamera;
        [SerializeField, ShowIf("useCamera")] private GameObject cinemachineCamera;
        [SerializeField, ShowIf("useCamera")] private Transform lookAtTarget;
        [SerializeField, ShowIf("useCamera")] private Vector3 cameraTargetOffset;

        private void Awake()
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>(FindObjectsInactive.Include);
        }

        private void OnDrawGizmos()
        {
            if (useCamera)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + cameraTargetOffset, 0.1f);
                Gizmos.DrawLine(cameraTargetOffset + transform.position, lookAtTarget.position);
            }
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

            cinemachineCamera.SetActive(false);
            mainCamera.DOMove(transform.position + cameraTargetOffset, 0.5f).SetEase(Ease.OutQuad);
            Quaternion lookAtRotation = Quaternion.LookRotation(lookAtTarget.position - (transform.position + cameraTargetOffset));
            mainCamera.DORotateQuaternion(lookAtRotation, 0.5f).SetEase(Ease.OutQuad);
        }

        private void DialogueManager_OnDialogueEnded()
        {
            dialogueManager.OnDialogueEnded -= DialogueManager_OnDialogueEnded;

            cinemachineCamera.SetActive(true);
        }
    }
}
