using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// This script should be attached to the player and includes coroutines to handle the player respawn. 
    /// The player respawns after hitting a DeathBox, which quickly moves the player back to the current checkpoint.
    /// </summary>
    public class PlayerDeathHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Rigidbody rb;

        private InputManager _inputManager;

        [Header("Respawn Motion")]
        [SerializeField] private float _liftHeight = 3f;
        [SerializeField] private float _liftTime = 0.25f;
        [SerializeField] private float _moveTime = 1f;
        [SerializeField] AnimationCurve _ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _extraRespawnHeight = 7f; // optional
 
        private bool _isRespawning;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            _inputManager = InputManager.Instance;
        }

        /// <summary>
        /// Respawns the rigidbody to the respawn point.
        /// </summary>
        /// <param name="respawnPoint"></param>
        public void RespawnTo(Transform respawnPoint)
        {
            if (_isRespawning) return;
            StartCoroutine(RespawnRoutine(respawnPoint.position, respawnPoint.rotation));
        }

        /// <summary>
        /// Disables player actions, moves the player back to a target transform, then reenables player actions.
        /// </summary>
        /// <param name="targetPos"></param>
        /// <param name="targetRot"></param>
        /// <returns></returns>
        private IEnumerator RespawnRoutine(Vector3 targetPos, Quaternion targetRot)
        {
            _isRespawning = true;

            // freeze player
            _inputManager.DisableAllActions();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            bool prevKinematic = rb.isKinematic;
            rb.isKinematic = true;

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            Vector3 extraHeight = new Vector3(0f , _extraRespawnHeight, 0f);

            if (_liftTime > 0f && _liftHeight != 0f)
            {
                Vector3 liftPos = startPos + Vector3.up * _liftHeight;
                yield return MoveOverTime(startPos, liftPos, startRot, startRot, _liftTime);
                startPos = liftPos;
            }

            yield return MoveOverTime(startPos, targetPos + extraHeight, 
                                      startRot, targetRot, _moveTime);

            rb.isKinematic = prevKinematic;

            // reenable player actions
            _inputManager.EnablePlayerActions();

            _isRespawning = false;
        }

        /// <summary>
        /// A coroutine to move the player to a target transform.
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <param name="fromRot"></param>
        /// <param name="toRot"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        private IEnumerator MoveOverTime(Vector3 fromPos, Vector3 toPos, 
                                         Quaternion fromRot, Quaternion toRot, 
                                         float time)
        {
            if (time <= 0f)
            {
                transform.SetPositionAndRotation(toPos, toRot);
                yield break;
            }

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / time;
                float e = _ease.Evaluate(Mathf.Clamp01(t));
                transform.SetPositionAndRotation(
                    Vector3.Lerp(fromPos, toPos, e),
                    Quaternion.Slerp(fromRot, toRot, e)
                    );
                yield return null;
            }
        }
    }
}
