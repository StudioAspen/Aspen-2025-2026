using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class RespawnPlayer_E : MonoBehaviour
    {
        GameObject myPlayer;
        [SerializeField] Vector3 respawnPoint;
        // Start is called before the first frame update
        void Start()
        {
            myPlayer = GameObject.Find("Player");
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Player")
            {
                myPlayer.transform.position = respawnPoint;

            }
        }
    }
}
