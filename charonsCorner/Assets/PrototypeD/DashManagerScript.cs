using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DashManagerScript : MonoBehaviour
{


    [Header("UI variables")]
    [SerializeField] private float DashBarTotal = 200f;
    [SerializeField] private float DashCost = 100f;
    [SerializeField] private float PassiveChargeRate = 10f;
    [SerializeField] private float ActiveChargeRate = 1f;


    [SerializeField] private float minSlowTime;
    [Tooltip("rate at which time speeds back to normal")]
    [SerializeField] private float timeResumeSpeed;


    [Header("Re Worked Player Dash Veriables")]

    // speed cap tier ranges from 0-4 increases every succesful dash
    public int SpeedCap = 1;

    [Tooltip("Speed Cap Limit: How many stages of speedIncreases there are")] 
    [SerializeField] public int SpeedCapLimit = 5;
    [Tooltip("Max speed the player can reach based off of speed cap")]
    [SerializeField] private float MaxSpeed = 0f;
    [Tooltip("Default speed for player never changes in game]")]
    [SerializeField] private float BaseSpeed = 30f;
    [Tooltip("multiplier for how much speed increases per speed stage")]
    [SerializeField] private float multiplier = 0.5f;
    [Tooltip("how fast the player returns to max speed after a dash")]
    [SerializeField] private float PullBackSpeed = 1f;


    private bool canBoost = true;



    // CHANGE THIS LATER NOT NEEDED ONCE ORGANIZED
    [Header(" values referenced in PlayerController ")]
    public int chargeCounter = 2;
    public float maxPlayerSpeed = 0f;
    public float DashSpeed = 0f;

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



    /// <summary>
    /// Increases speed based off of current speed cap and 
    /// current forward cruise speed
    /// </summary>
    public float IncreaseSpeed()
    {
        if (SpeedCap <= SpeedCapLimit && canBoost == true)
        {
            float newMult = 0;
            canBoost = false;
            newMult = multiplier * SpeedCap;
            float finalMult = 1f + newMult;
            MaxSpeed = BaseSpeed * finalMult;
            Debug.Log("Current Max speed: " + MaxSpeed + "Current Mult: " + finalMult);
            SpeedCap += 2;
            lowerSpeed();
            // increment PlayerController3D counter if available
            if (PC3DScript != null)
            {
                PC3DScript.numSpeedIncreases += 1; // Fonz - count number of speed increases
            }
            return MaxSpeed;
        }
        return PC3DScript.forwardCruiseSpeed;
    }

    public void lowerSpeed()
    {
        SpeedCap -= 1;
        Debug.Log("speedcap after lowering: " + SpeedCap);
        if (SpeedCap <= SpeedCapLimit)
        {
            canBoost = true;
            float newMult = multiplier * SpeedCap;
            float finalMult = 1f + newMult; // <-- fixed: use 1 + newMult
            MaxSpeed = BaseSpeed * finalMult;

            Debug.Log("Current Max speed: " + MaxSpeed + " Current Mult: " + finalMult);

            // Gradually move forwardCruiseSpeed toward MaxSpeed.
            // Treat PullBackSpeed as units/second; multiply by Time.deltaTime.
            if (PC3DScript != null)
            {
                PC3DScript.forwardCruiseSpeed = Mathf.MoveTowards(
                    PC3DScript.forwardCruiseSpeed,
                    MaxSpeed,
                    PullBackSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            SpeedCap = 5;
        }
    }

}
