using UnityEngine;
using CharonsCorner.ItemPowers;
using CharonsCorner.Runtime;


    public class JumpPower : ItemPower
    {
        public float jumpForce;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //itemName = "Jump";
    }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override void itemPower(Rigidbody Rb)
        {
            GameObject go = Rb.gameObject;

            if (Rb.linearVelocity.y < 0f)
            {
                Rb.linearVelocity = new Vector3(Rb.linearVelocity.x, 0, Rb.linearVelocity.z);
            }
            
            Rb.AddForce(go.GetComponent<PrototypePlayerController>().Orientation.up * jumpForce, ForceMode.VelocityChange);
            go.GetComponent<PlayerAbility>().currentUses--;
        }
    }

