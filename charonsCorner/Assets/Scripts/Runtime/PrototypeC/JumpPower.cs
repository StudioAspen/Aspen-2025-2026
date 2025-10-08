using UnityEngine;
using CharonsCorner.ItemPowers;
using CharonsCorner.Runtime;


    public class JumpPower : ItemPower
    {
        public float jumpForce;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override void itemPower(Rigidbody Rb)
        {
            GameObject go = Rb.gameObject;
            //Vector3 myUp = go.transform.TransformDirection(Vector3.up);
            //Rb.AddForce(myUp * jumpForce, ForceMode.Impulse);
            Rb.AddForce(go.GetComponent<PrototypePlayerController>().Orientation.up * jumpForce, ForceMode.Impulse);
        }
    }

