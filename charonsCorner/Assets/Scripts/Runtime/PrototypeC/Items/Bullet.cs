using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Bullet : MonoBehaviour
    {
        public int damage;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                collision.gameObject.GetComponent<Enemy>().takeDamage(damage);
                Destroy(gameObject);
            }
            if (collision.gameObject.CompareTag("Obstacle") || collision.gameObject.CompareTag("Pin"))
            {
                Destroy(gameObject);
            }
        }
    }
}
