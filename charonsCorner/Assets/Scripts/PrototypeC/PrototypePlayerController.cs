using System;
using UnityEngine;
using CharonsCorner.Runtime;
using UnityEngine.SceneManagement;

namespace CharonsCorner.PrototypeC
{
    [RequireComponent(typeof(Rigidbody))]
    public class PrototypePlayerController : MonoBehaviour
    {
        [field: SerializeField] public float Speed {get; private set;}
        [field:SerializeField] public Transform Orientation { get; private set; }

        public Rigidbody Rb { get; private set; }
        
        
        // [field: Header("State Machine")]
        // [field: SerializeField] public StateMachine<PrototypePlayerController> StateMachine { get; private set; }
        // [field: SerializeField] public GroundedSuperState Grounded { get; private set; } = new();
        // [field: SerializeField] public AirborneSuperState Airborne { get; private set; } = new();


        private void Awake()
        {
            Rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(2);
            }
        }
        
        private void FixedUpdate()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Rb.AddForce(Orientation.forward * (input.y * Speed) + Orientation.right * (input.x * Speed), ForceMode.VelocityChange);
        }
    }
}