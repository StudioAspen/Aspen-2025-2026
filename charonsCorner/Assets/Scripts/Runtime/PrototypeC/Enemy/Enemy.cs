using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

namespace CharonsCorner.Runtime
{

    public abstract class Enemy : MonoBehaviour
    {

        [SerializeField] private string enemyName;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float maxHealthPoint;
        [SerializeField] private float scorePoints;
        private float healthPointCurr;

        private void Start()
        {
            Introduction();
        }

        private void Update()
        {

        }

        private void End()
        {


        }

        private void Introduction()
        {
            Debug.Log("I'm " + enemyName + ", HP: " + maxHealthPoint + ", Speed: " + moveSpeed);
        }


    }
}
