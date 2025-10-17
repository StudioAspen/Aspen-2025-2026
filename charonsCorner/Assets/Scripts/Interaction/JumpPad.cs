using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce; 
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Rigidbody Rb))
        {
            Rb.AddForce(transform.up * jumpForce);
        }
    }
}
