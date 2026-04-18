using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class PlayerCheckpoint : MonoBehaviour
    {
        public GameObject myPlayer;
        float positionX;
        float positionY;
        float positionZ;
        // Start is called before the first frame update
        void Start()
        {
            positionX = 981f;
            positionY = 62f;
            positionZ = 345f;
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

