using System;
using System.Collections;
using UnityEngine;

public class SentientPin : MonoBehaviour
{

    [SerializeField] private float DetectionRange = 10f;
    [SerializeField] private float TotalJumps = 5f;
    private float currentJumps = 0f;
    [SerializeField] private float DurationTillJump = 8f;
    [SerializeField] private float JumpDistance = 5f;
    [SerializeField] private float JumpHeight = 5f;
    [SerializeField] private float Jumpspeed = 4f;

    private SphereCollider sphereCollider;

    [SerializeField] private Transform player;
    private bool isJumping = false;
    private Vector3 jumpDirection;
    private Vector3 JumpStartPosition;
    private float JumpProgress = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        // sphereCollider.radius = DetectionRange;

        

    }

    void Update()
    {


    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 0)
        {
            initiateJump();
        }
    }


    void initiateJump()
    {
        if (currentJumps >= TotalJumps)
        {
            // tell the pin to jump off 
            Debug.Log("max jumps reached");
            return;
        }
        isJumping = true;
        currentJumps++;
        JumpProgress = 0f;
        JumpStartPosition = transform.position;

        // calc 
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;

        directionFromPlayer.y = 0f;
        directionFromPlayer.Normalize();

        jumpDirection = directionFromPlayer * JumpDistance;

        ExecuteJump();
    }


    void ExecuteJump()
    {
        JumpProgress += Time.deltaTime * Jumpspeed;


        if (JumpProgress <= 1f)
        {
            float horizontalProgress = JumpProgress;
            float verticalProgress = Mathf.Sin(JumpProgress * Mathf.PI);

            Vector3 horizontalMovement = jumpDirection * horizontalProgress;
            float verticalMovement = JumpHeight * verticalProgress;


            transform.position = JumpStartPosition + horizontalMovement + Vector3.up * verticalMovement;
        }
        else
        {
            isJumping = false;
            JumpProgress = 0f;
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
