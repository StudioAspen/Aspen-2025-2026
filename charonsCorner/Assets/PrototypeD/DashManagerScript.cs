using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class DashManagerScript : MonoBehaviour
{

    [SerializeField] private float DashBarTotal = 200f;
    [SerializeField] private float DashCost = 100f;
    [SerializeField] private float PassiveChargeRate = 10f;
    [SerializeField] private float ActiveChargeRate = 1f;


    [SerializeField] private float minSlowTime;
    [Tooltip("rate at which time speeds back to normal")]
    [SerializeField] private float timeResumeSpeed;

    // CHANGE THIS LATER NOT NEEDED ONCE ORGANIZED
    [Header(" values referenced in PlayerController ")]
    public int chargeCounter = 2;
    public float maxPlayerSpeed = 0f;
    public float DashSpeed = 0f;
    public float PullBackSpeed = 0f;

    public bool reachedMaxSpeed = false;

    [SerializeField] private Slider slider1;
    [SerializeField] private PlayerController3D PC3DScript;


    private Vector3 lastPosition;
    private float targetDistance = 1f;
    private float currentSegment = 0f;
    private bool isTracking = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slider1.maxValue = DashBarTotal;
        slider1.minValue = 0f;
        slider1.value = DashBarTotal;
    }

    // Update is called once per frame
    void Update()
    {

        if (PC3DScript.forwardCruiseSpeed > maxPlayerSpeed && PC3DScript.isBoostRecovering == true)
        {
            PC3DScript.forwardCruiseSpeed -= PullBackSpeed;
            
        }
        if (PC3DScript.forwardCruiseSpeed <= maxPlayerSpeed &&  PC3DScript.boostRecoverElapsed >= PC3DScript.boostRecoverTime)
        {
            reachedMaxSpeed = true;
        }

        if (isTracking == true)
        {
            float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
            currentSegment += distanceThisFrame;


            if (currentSegment >= targetDistance)
            {
                currentSegment = 0f; // Reset for next meter
                isTracking = false;
                OnOneMeterPassed();
            }
        }



        // Allows the Player to Dash and counts how many charges are left
        if (Input.GetKeyDown(KeyCode.LeftShift) && chargeCounter > 0)
        {
            slider1.value -= DashCost;
            chargeCounter--;
            lastPosition = transform.position;
            isTracking = true;

        }


        if (slider1.value < 200)
        {
            slider1.value += PassiveChargeRate;
        }



        if (slider1.value == 100 && chargeCounter <= 0)
        {
            chargeCounter++;
        }
        else if (slider1.value == 200 && chargeCounter <= 1)
        {
            chargeCounter++;
            isTracking = false;
        }

    }

    /// <summary>
    /// Slows down game time to value of min speed.
    /// </summary>
    /// <remarks>
    /// This affects the entire game's time scale. 
    /// Time.timeScale values range from 0 (paused) to 1 (normal speed).
    /// </remarks>
    public void slowTime()
    {
        Time.timeScale = minSlowTime;
    }

    /// <summary>
    /// resume time to normal value 
    /// Default is 1
    /// </summary>
    public void resumeNormalTime()
    {
        if (Time.timeScale < 1)
        {
            Time.timeScale += timeResumeSpeed;
        }
    }
    

    /// <summary>
    /// after 1 meter passed resets to track a new meter
    /// </summary>
    void OnOneMeterPassed()
    {
        currentSegment = 0f;
        isTracking = true;
        lastPosition = transform.position;

        Debug.Log("Distance tracking reset");
    }

}
