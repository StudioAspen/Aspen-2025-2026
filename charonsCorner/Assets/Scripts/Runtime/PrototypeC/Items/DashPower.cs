using UnityEngine;
using CharonsCorner.ItemPowers;
using CharonsCorner.Runtime;

namespace CharonsCorner.Runtime
{
    public class DashPower : ItemPower
    {
        public float dashForce;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //itemName = "Dash";
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override void itemPower(Rigidbody Rb)
        {
            GameObject go = Rb.gameObject;
            Rb.linearVelocity = new Vector3(0, 0, 0);
            Rb.AddForce(go.GetComponent<PrototypePlayerController>().Orientation.forward * dashForce, ForceMode.VelocityChange);
            go.GetComponent<PlayerAbility>().currentUses--;
        }
    }
}
