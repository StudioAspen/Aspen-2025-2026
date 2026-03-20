using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Lvl2Player2Death1 : MonoBehaviour
    {
        public GameObject myPlayer;
        float positionX;
        float positionY;
        float positionZ;
        // Start is called before the first frame update
        void Start()
        {
            positionX = 1001.12f;
            positionY = 62.19f;
            positionZ = 51.09f;
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
