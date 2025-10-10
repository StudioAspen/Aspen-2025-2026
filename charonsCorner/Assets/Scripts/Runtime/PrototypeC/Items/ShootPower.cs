using System;
using UnityEditor;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class ShootPower : MonoBehaviour
    {
        
        public GameObject player;
        public Vector3 playerOffset;
        public GameObject firePoint;
        public GameObject bullet;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = player.transform.position + playerOffset;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -(transform.position.x - Camera.main.transform.position.x);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.LookAt(worldPos);
        }


    }
}
