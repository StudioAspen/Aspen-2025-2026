using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Goal : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private CapsuleCollider capsuleCollider;
        [SerializeField]
        private GameObject winScreen;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            if (ScoreManager.Instance.numPinsKnocked < 10)
            {
                meshRenderer.material.color = Color.red;
                capsuleCollider.enabled = false;
            }
            else
            {
                meshRenderer.material.color = Color.green;
                capsuleCollider.enabled = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                winScreen.SetActive(true);
            }
        }
    }
}
