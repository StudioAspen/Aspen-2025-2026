using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class RotateObject_E : MonoBehaviour
    {
        [SerializeField] float rotateX = 100f;
        [SerializeField] float rotateY = 100f;
        [SerializeField] float rotateZ = 100f;
        private Transform itemTransform;

        // Start is called before the first frame update
        void Start()
        {
            itemTransform = this.GetComponent<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
            //Should always multiply your movement or rotation speeds by Time.deltaTime
            itemTransform.Rotate(rotateX * Time.deltaTime, rotateY * Time.deltaTime, rotateZ * Time.deltaTime);
        }

    }
  }
