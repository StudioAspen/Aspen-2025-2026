using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerDeath : MonoBehaviour
    {
        public GameObject myPlayer;
        float positionX;
        float positionY;
        float positionZ;
        // Start is called before the first frame update
        void Start()
        {
            positionX = 997.5716f;
            positionY = 61.77f;
            positionZ = 14.61484f;
            myPlayer = GameObject.Find("Player");
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnCollisionEnter(Collision other)
        {

            if (other.gameObject.tag == "Player")
            {

                myPlayer.transform.position = new Vector3(positionX, positionY, positionZ);

            }

        }
    }
}
