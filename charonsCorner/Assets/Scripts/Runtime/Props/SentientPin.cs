using System.Collections;
using UnityEngine;

public class SentientPin : MonoBehaviour
{

    [Tooltip("Range where the pin detects player")]
    [SerializeField] private float DetectionRange = 10f;

    [Tooltip("max jumps before death/end")]
    [SerializeField] private float TotalJumps = 5f;
    private float currentJumps = 0f;


    [Tooltip("amount of time before the pin jumps off the map")]
    [SerializeField] private float DurationTillJump = 8f;

    [Tooltip("the distance the pin can jump")]
    [SerializeField] private float JumpDistance = 5f;
    [Tooltip("Range where the pin detects player")]
    [SerializeField] private float JumpHeight = 5f;
    [Tooltip("speed the player will move")]
    [SerializeField] private float speed = 4f;

    private SphereCollider sphereCollider;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = DetectionRange;
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            StartCoroutine(Jump());
        }
    }

    private IEnumerator Jump()
    {
        yield return new WaitForSeconds(15f);
        currentJumps++;

        // math and calc here 

        if (currentJumps >= TotalJumps)
        {
            yield return StartCoroutine(startDeath());
        }

    }


    private IEnumerator startDeath()
    {
        

        yield return new WaitForSeconds(15f);
        
    }
}
