using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PowManager : MonoBehaviour
    {
        [SerializeField] private PowEffectScript _powEffect;
        private bool _powActive = false;
        private float _currentStrength = 0f;

        private void OnEnable()
        {
            PinKnockback.OnPinHit += HandlePinHit;
        }

        private void OnDisable()
        {
            PinKnockback.OnPinHit -= HandlePinHit;
        }

        private void HandlePinHit(Vector3 position, float speedScale, Transform lookTarget)
        {
            if (!_powActive)
            {
                PlayPowEffect(position, speedScale, lookTarget);
                return;
            }else
                return;
        }

        private void PlayPowEffect(Vector3 position, float strength, Transform hitter)
        {
            _powEffect.gameObject.SetActive(true);
            _currentStrength = strength;
            _powActive = true;

            _powEffect.GrowPin(strength, position + Vector3.up * 1.5f, hitter);

            StartCoroutine(ResetAfterLifetime(_powEffect._growTimer));
        }

        private IEnumerator ResetAfterLifetime(float time)
        {
            yield return new WaitForSeconds(time);
            _powActive = false;
            _currentStrength = 0f;
        }
    }
}

