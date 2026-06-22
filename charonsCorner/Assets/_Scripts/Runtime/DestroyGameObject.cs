using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class DestoryGameObject : MonoBehaviour
    {
        [SerializeField] GameObject[] TargetObjects;
        [SerializeField] float hideTime = 10f; // seconds
        [SerializeField] bool StayHidden = false; // Option to destroy objects after hiding

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && TargetObjects != null)
            {
                if (StayHidden)
                {
                    SwitchTargetActiveState(false);
                }
                else
                {
                    StartCoroutine(HideAndShow());
                }
            }
        }

        IEnumerator HideAndShow()
        {
            SwitchTargetActiveState(false);
            yield return new WaitForSeconds(hideTime);
            SwitchTargetActiveState(true);
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
