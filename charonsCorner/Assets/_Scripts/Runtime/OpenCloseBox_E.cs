using System.Collections;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class OpenCloseBox_E : MonoBehaviour
    {
        [SerializeField] GameObject OpenedBox;
        [SerializeField] GameObject ClosedBox;
        [SerializeField] GameObject[] SideColliders;

        [Tooltip("How long before platform opens")]
        [SerializeField] float TimeBeforeOpen = 3f; // seconds
        [Tooltip("How long box platform stays open before closing again")]
        [SerializeField] float OpenDuration = 6f; // seconds

        bool Opening = false;

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("Player") && !Opening)
            {
                Opening = true;
                StartCoroutine(OpenThenClose());
            }
        }

        IEnumerator OpenThenClose()
        {
            yield return new WaitForSeconds(TimeBeforeOpen);
            ClosedBox.SetActive(false);
            OpenedBox.SetActive(true);
            this.GetComponent<Collider>().enabled = false;
            for (int i = 0; i < SideColliders.Length; i++)
            {
                SideColliders[i].GetComponent<Collider>().enabled = false;
            }
            yield return new WaitForSeconds(OpenDuration);
            ClosedBox.SetActive(true);
            OpenedBox.SetActive(false);
            this.GetComponent<Collider>().enabled = true;
            for (int i = 0; i < SideColliders.Length; i++)
            {
                SideColliders[i].GetComponent<Collider>().enabled = true;
            }
            Opening = false;
        }
    }
}
