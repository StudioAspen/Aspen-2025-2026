using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DestoryGameObject : MonoBehaviour
    {
        [SerializeField] GameObject[] TargetObjects;
        [SerializeField] float hideTime = 10f; // seconds
        [SerializeField] bool HideIndefinitely = false; // Option to destroy objects after hiding

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (HideIndefinitely)
                {
                    ToggleObjectsActiveState(false);
                }
                else
                {
                    StartCoroutine(HideAndShow());
                }
            }
        }

        IEnumerator HideAndShow()
        {
            ToggleObjectsActiveState(false);
            yield return new WaitForSeconds(hideTime);
            ToggleObjectsActiveState(true);
        }

        void ToggleObjectsActiveState(bool status)
        {
            for (int i = 0; i < TargetObjects.Length; i++)
            {
                TargetObjects[i].SetActive(status);
            }
        }
    }
  }
