using UnityEngine;


namespace CharonsCorner.Runtime
{
    public class PowManager : MonoBehaviour
    {
        [SerializeField] private PowEffectScript powEffect;
        private bool powActive = false;
        private float currentStrength = 0f;

        

        void OnEnable()
        {
            PinKnockback.OnPinHit += handlePinHit;
        }

        void OnDisable()
        {
            PinKnockback.OnPinHit -= handlePinHit;
        }


        void handlePinHit(Vector3 position, float speedScale, Transform lookTarget)
        {
            if (!powActive)
            {
                playPowEffect(position, speedScale, lookTarget);
                return;
            }else
                return;
        }

        void playPowEffect(Vector3 position, float strength, Transform hitter)
        {
            powEffect.gameObject.SetActive(true);
            currentStrength = strength;
            powActive = true;

            powEffect.growPin(strength, position + Vector3.up * 1.5f, hitter);

            StartCoroutine(ResetAfterLifetime(powEffect.growTimer));
        }

        System.Collections.IEnumerator ResetAfterLifetime(float time)
        {
            yield return new WaitForSeconds(time);
            powActive = false;
            currentStrength = 0f;
        }

    }
}

