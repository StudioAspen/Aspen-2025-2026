
using System;
using UnityEditor;
using UnityEngine;
using CharonsCorner.ItemPowers;

namespace CharonsCorner.Runtime
{
    public class ShootPower : ItemPower
    {
        
        public GameObject player;
        public Vector3 playerOffset;
        public GameObject firePoint;
        public GameObject bullet;
        public float bulletForce;
        public float shootCoolDown;
        private float timer;
        [SerializeField]
        private PlayerAbility playerAbility;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = player.transform.position + playerOffset;
            Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            transform.LookAt(worldPos);

            if (Input.GetKeyDown(KeyCode.Space) && timer <= 0 && itemUses > 0)
            {
                GameObject newBullet = Instantiate(bullet, firePoint.transform.position, firePoint.transform.rotation);
                newBullet.GetComponent<Rigidbody>().AddForce(-newBullet.transform.forward * bulletForce, ForceMode.Impulse);
                timer = shootCoolDown;
                playerAbility.currentUses--;
                if (playerAbility.currentUses == 0)
                {
                    gameObject.SetActive(false);
                }
            }

            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
        }



        public override void itemPower(Rigidbody Rb)
        {

        }
    }
}
