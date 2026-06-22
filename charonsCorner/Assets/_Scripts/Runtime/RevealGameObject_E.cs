using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class RevealGameObject_E : MonoBehaviour
    {
        [SerializeField] GameObject[] TargetObjects;
        [SerializeField] float RevealTime = 10f; // seconds
        [SerializeField] bool PersistAfterShow = false; // Option to destroy objects after hiding

        private void Start()
        {
            for (int i = 0; i < TargetObjects.Length; i++)
            {
                TargetObjects[i].SetActive(false);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (PersistAfterShow)
                {
                    SwitchTargetActiveState(true);
                }
                else
                {
                    StartCoroutine(ShowThenHide());
                }
            }
        }

        IEnumerator ShowThenHide()
        {
            SwitchTargetActiveState(true);
            yield return new WaitForSeconds(RevealTime);
            SwitchTargetActiveState(false);
        }

        void SwitchTargetActiveState(bool status)
        {
            for (int i = 0; i < TargetObjects.Length; i++)
            {
                TargetObjects[i].SetActive(status);
                //TargetObjects[i].SetActive(!TargetObjects[i].activeSelf);
            }
        }
    }
  }
