using UnityEngine;
using TMPro;

namespace CharonsCorner.Runtime
{
    public class ZoneTimer : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI timerText;
        public Collider startTrigger;
        public Collider stopTrigger;
        public LayerMask playerLayerMask;

        private float timer = 0f;
        private bool isTiming = false;

        private void Update()
        {
            if (isTiming)
            {
                timer += Time.deltaTime;
                timerText.text = timer.ToString("F2") + "s";
            }
        }

        private void OnEnable()
        {
            if (startTrigger != null)
                startTrigger.gameObject.AddComponent<ZoneTimerTrigger>().Init(this, true, playerLayerMask);
            if (stopTrigger != null)
                stopTrigger.gameObject.AddComponent<ZoneTimerTrigger>().Init(this, false, playerLayerMask);
        }

        public void StartTimer()
        {
            timer = 0f;
            isTiming = true;
        }

        public void StopTimer()
        {
            isTiming = false;
            timerText.text = timer.ToString("F2") + "s";
        }

        // Helper trigger class
        private class ZoneTimerTrigger : MonoBehaviour
        {
            private ZoneTimer timerScript;
            private bool isStart;
            private LayerMask mask;

            public void Init(ZoneTimer script, bool start, LayerMask layerMask)
            {
                timerScript = script;
                isStart = start;
                mask = layerMask;
            }

            private void OnTriggerEnter(Collider other)
            {
                if (((1 << other.gameObject.layer) & mask.value) != 0)
                {
                    if (isStart)
                        timerScript.StartTimer();
                    else
                        timerScript.StopTimer();
                }
            }
        }
    }
}