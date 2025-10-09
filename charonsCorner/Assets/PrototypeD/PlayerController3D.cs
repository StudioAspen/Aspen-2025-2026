using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController3D : MonoBehaviour
{
    [Header("Forward movement (always moving forward)")]
    [Tooltip("Default cruising forward speed when not affected by brake/squash/boost.")]
    public float forwardCruiseSpeed = 12f;

    [Tooltip("Current runtime forward speed (internal).")]
    [SerializeField] private float currentForwardSpeed;

    [Header("Lateral movement (A/D or Left/Right)")]
    public float lateralBaseSpeed = 2.5f;     // immediate base left/right movement when key pressed
    public float lateralMaxSpeed = 6f;        // max lateral speed after holding
    public float lateralAccelTime = 0.5f;     // how long until lateral reaches max
    public AnimationCurve lateralAccelCurve = AnimationCurve.Linear(0, 0, 1, 1);

    // internal
    private float lateralHoldTime = 0f;
    private int lateralDir = 0; // -1 left, 0 none, 1 right
    private float currentLateralSpeed = 0f;

    [Header("Brake Dash (Hold Left Mouse Button)")]
    [Tooltip("How fast forward speed should be reduced while braking")]
    public float brakeTargetSpeed = 1.5f;
    [Tooltip("Time to interpolate from current speed down to brakeTargetSpeed")]
    public float brakeTime = 0.15f;
    public AnimationCurve brakeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Boost speed applied immediately when player releases brake")]
    public float boostSpeed = 20f;
    [Tooltip("How long it takes to go from boostSpeed back to cruise")]
    public float boostRecoverTime = 1f;
    public AnimationCurve boostRecoverCurve = AnimationCurve.Linear(0, 1, 1, 0); // evaluate 0..1 and Lerp(boost->cruise, curve)
    [Tooltip("Maximum time player has to hold break when low speed")] // Fonz
    public float brakeHoldMaxTime = 2f; // Fonz - The time a player can spend in dash aim mode at the lowest possible speed
    [Tooltip("Minimum time player has to hold break when high speed")] // Fonz
    public float brakeHoldMinTime = 0.5f; // Fonz - The time a player can spend in dash aim mode at the highest possible speed
    [Tooltip("Time player has to hold break")] // Fonz
    public float brakeHoldTime = 2f; // Fonz - time to hold brake, between min and max based on speed, starts at max
    [Tooltip("Permanent speed increase after Dash")] // Fonz
    public float speedPercentageIncrease = 1.4f; // Fonz - increase speed after each boost, base 40% increase
    [Tooltip("Number of Speed Increases applied")]
    public int numSpeedIncreases = 0; // Fonz - number of speed increases applied

    [Header("Boost FOV")]
    [Tooltip("FOV to zoom out to during boost burst")]
    public float boostFOV = 70f;
    [Tooltip("How quickly to reach boostFOV when boost begins")]
    public float boostFOVTime = 0.2f;
    private float brakeHoldElapsed = 0f;
    [Tooltip("Extra camera pull-in on Z axis during boost (negative brings camera closer).")]
    public float boostZOffsetDelta = -2f;
    // internal brake state
    private bool isBraking = false;
    private float brakeElapsed = 0f;
    private float preBrakeSpeed = 0f;
    private bool hasRotatedDuringBrake = false;
    // change back to private later
    public bool isBoostRecovering = false;
    // change back to private later
    public float boostRecoverElapsed = 0f;

    [Header("Squash / Charge Jump (Hold Right Mouse Button)")]
    [Tooltip("Y scale to use while squashed (e.g. 0.4)")]
    public float squashScaleY = 0.35f;
    [Tooltip("Time to reach squashScaleY")]
    public float squashScaleTime = 0.18f;
    public AnimationCurve squashScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("While squashed forward speed will interpolate to this value")]
    public float squashSlowTargetSpeed = 2.0f;
    [Tooltip("Time to reach squash slow speed")]
    public float squashSlowTime = 0.25f;
    public AnimationCurve squashSlowCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("How fast the jump charge grows (units per second) while squashed")]
    public float jumpChargeRate = 1.5f;
    [Tooltip("Charge value required to auto-trigger jump")]
    public float jumpMaxCharge = 3f;

    [Tooltip("Total time of the jump profile (up+down). The jumpCurve maps 0..1 -> 0..1 for height fraction")]
    public float jumpDuration = 0.7f;
    public AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 0);

    [Tooltip("If player releases right mouse before full charge, recover scale in this time")]
    public float squashRecoverTime = 0.15f;

    // internal squash state
    private bool isSquashing = false;
    private float squashScaleElapsed = 0f;
    private float squashSlowElapsed = 0f;
    private float jumpCharge = 0f;
    [Tooltip("Max seconds to fully charge jump while squashed")]
    public float maxJumpChargeTime = 2f;
    [Header("Misc")]
    [Tooltip("How quickly lateral input decays when the player releases the key (makes it less twitchy)")]
    public float lateralReleaseDamping = 20f;

    [Tooltip("Key/axis for left/right movement (uses Unity's Horizontal by default). You can also use A/D or arrows.")]
    public string lateralAxis = "Horizontal";

    [Tooltip("Rotation snap angle applied when rotating in brake (degrees). +/- 90 by default.")]
    public float brakeTurnAngle = 90f;
    [Header("Jump/Gravity Settings")]
    public float gravity = -20f;     // downward acceleration
    private float verticalVelocity;  // current vertical speed
    private CharacterController cc;
    private Vector3 storedMoveThisFrame = Vector3.zero;
    [Header("Collider Squash Settings (3D Ball)")]
    public float squashedRadius = 0.35f;   // radius while squished
    private float originalRadius;
    // scale bookkeeping
    private Vector3 originalLocalScale;
    // movement direction separate from transform.forward so brake-queued turns don't change movement immediately
    private Vector3 moveDirectionForward;
    private float pendingBrakeTurn = 0f; // -90, +90, or 0
    private int pendingLateralDir = 0; // -1 for left, +1 for right, 0 for none

    // jump bookkeeping
    private bool isJumping = false;
    private float jumpStartY = 0f;
    private float jumpTargetHeight = 0f;
    private float jumpElapsed = 0f;
    [Header("Camera Tilt")]
    public CinemachineCamera vcam;
    public float maxDutchAngle = 15f;      // how far to tilt left/right
    public float dutchLerpSpeed = 5f;      // how fast it lerps
    private float targetDutch = 0f;
    [Header("Camera FOV")]
    public float squashFOV = 40f;  // target FOV when fully squashed
    private float defaultFOV;
    [Tooltip("How long the FOV takes to recover after jump/release.")]
    public float fovRecoverTime = 0.25f;// saved at Start
  

    private Vector3 defaultFollowOffset;   // remember original follow offset

    [Header("Camera Turn")]
    public float cameraTurnAngle = 90f;       // how far left/right to rotate
    public float cameraTurnLerpSpeed = 5f;    // how fast the rotation lerps
    private float targetCameraYaw = 0f;       // current target yaw
    private float currentCameraYaw = 0f;      // smoothed yaw
    private CinemachineFollow followComp;
    [SerializeField] private float cameraYawAmount = 90f;
    [SerializeField] private float cameraYawLerpSpeed = 5f;
    private float targetYaw = 0f;
    private float currentYaw = 0f;
    // Add this at the top with other state flags
    [Header("Knockover Mode")]
    public bool knockoverMode = false;
    [Tooltip("How fast the brake angle rotates while holding A/D (degrees per second).")]
    public float brakeAngleAdjustSpeed = 60f; // <<< NEW

    private float currentBrakeAngle = 0f;     // <<< NEW
    [Header("Brake Preview")]
    public Transform brakePreview; // assign child object in inspector

    public float crouchHeight = 1f;
    public float normalHeight = 2f;


    public DashManagerScript dmScript;


    void Awake()
    {
        cc = GetComponent<CharacterController>();

        originalLocalScale = transform.localScale;
        currentForwardSpeed = forwardCruiseSpeed;

      

        moveDirectionForward = transform.forward.normalized;
        followComp = vcam.GetComponent<CinemachineFollow>();
        if (followComp != null)
            defaultFollowOffset = followComp.FollowOffset;
    }
    void Start()
    {
        
        // this has to be changed later not actually necessary
        forwardCruiseSpeed = dmScript.maxPlayerSpeed;
        boostSpeed = dmScript.DashSpeed;

        if (vcam != null)
        {
            defaultFOV = vcam.Lens.FieldOfView;
        }
    }
    void Update()
    {
        float dt = Time.deltaTime;

        HandleInputs(dt);
        UpdateSpeedsAndStates(dt);
        ApplyMovement(dt);

        if (vcam != null)
        {
            float currentDutch = vcam.Lens.Dutch;
            float newDutch = Mathf.Lerp(currentDutch, targetDutch, dutchLerpSpeed * Time.deltaTime);
            vcam.Lens.Dutch = newDutch;
        }
        if (verticalVelocity > 0f || isBoostRecovering)
            knockoverMode = true;
        else
            knockoverMode = false;
        UpdateCameraYaw(dt);

        boostSpeed = forwardCruiseSpeed * 1.4f;

        if (Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
       
        if (hit.gameObject.tag == "Death")
        {
            resetSpeed();
        }

        if (hit.gameObject.tag == "Pin")
        {
            if (knockoverMode)
            {
                forwardCruiseSpeed += 2;
                Destroy(hit.gameObject);
            }
            else
            {
                resetSpeed();
            }

        }
    }


    void UpdateCameraYaw(float dt)
    {
        // Decide target yaw based on input
        if (Input.GetKey(KeyCode.A))
            targetYaw = -cameraYawAmount;
        else if (Input.GetKey(KeyCode.D))
            targetYaw = cameraYawAmount;
        else
            targetYaw = 0f;

        // Smoothly interpolate the yaw
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, cameraYawLerpSpeed * dt);

        // Apply it as an additional rotation to the virtual camera (NOT the follow offset)
        Quaternion baseRot = Quaternion.LookRotation(vcam.Follow.forward, Vector3.up);
        Quaternion yawRot = Quaternion.AngleAxis(currentYaw, Vector3.up);

        vcam.transform.rotation = yawRot * baseRot;
    }
    private void HandleInputs(float dt)
    {
        // --- LATERAL INPUT (read axis only when NOT braking and NOT squashing and NOT jumping) ---
        if (!isBraking && !isSquashing && !isJumping)
        {
            float h = Input.GetAxisRaw(lateralAxis);
            int newDir = 0;
            if (h > 0.1f) newDir = 1;
            else if (h < -0.1f) newDir = -1;

            if (newDir != 0)
            {
                if (newDir == lateralDir)
                {
                    lateralHoldTime += dt;
                }
                else
                {
                    lateralDir = newDir;
                    lateralHoldTime = 0f;
                }
            }
            else
            {
                lateralDir = 0;
                lateralHoldTime = 0f;
            }
        }
        else
        {
            // while braking/squashing, don't read axis-driven lateral input
            lateralHoldTime = 0f;
        }

        // ---- BRAKE (Left Mouse) ----
        if (Input.GetKeyDown(KeyCode.LeftShift) && dmScript.chargeCounter != 0)
        {
            if (!isJumping && !isSquashing)
            {
                StartBrake();

                // mess with timescale
                // dmScript.slowTime();
            }
        }

        if (isBraking)
        {
            // --- Angle control ---
            float h = Input.GetAxisRaw("Horizontal"); // -1,0,1
            if (Mathf.Abs(h) > 0.01f)
            {
                currentBrakeAngle += h * brakeAngleAdjustSpeed * dt;
                currentBrakeAngle = Mathf.Clamp(currentBrakeAngle, -brakeTurnAngle, brakeTurnAngle);
            }

            // --- Camera dutch and yaw feedback ---
            // tilt camera proportionally to stored angle
            targetDutch = (currentBrakeAngle / brakeTurnAngle) * maxDutchAngle;

            // yaw camera proportionally too
            targetCameraYaw = (currentBrakeAngle / brakeTurnAngle) * cameraTurnAngle;

            // --- Release brake: apply exit angle & boost ---
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                EndBrakeAndStartBoost();
                targetDutch = 0f;
                targetCameraYaw = 0f;
                dmScript.resumeNormalTime();
            }

            if (brakePreview != null)
            {
                Quaternion previewRot = Quaternion.Euler(0f, currentBrakeAngle, 0f);
                brakePreview.localRotation = Quaternion.Euler(90f, currentBrakeAngle, 0f);
            }
        }

        // ---- SQUASH (Right Mouse) ----
        if (Input.GetMouseButtonDown(1))
        {
            if (!isJumping && !isBraking)
            {
                StartSquash();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (isSquashing && !isJumping)
            {
                if (jumpCharge > 0f)
                {
                    // compute height based on % of charge time
                    float chargePercent = Mathf.Clamp01(jumpCharge / maxJumpChargeTime);
                    float jumpHeight = chargePercent * jumpMaxCharge;

                    BeginJump(jumpHeight);
                    isSquashing = false;
                    if (vcam != null)
                        StartCoroutine(RecoverFOV(defaultFOV, fovRecoverTime));
                }
                else
                {
                    CancelSquashWithoutJump();
                }
            }
        }
    }
    private IEnumerator RecoverFOV(float target, float duration)
    {
        if (vcam == null) yield break;

        float startFOV = vcam.Lens.FieldOfView;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            vcam.Lens.FieldOfView = Mathf.Lerp(startFOV, target, t);
            yield return null;
        }

        vcam.Lens.FieldOfView = target;
    }
    private void StartBrake()
    {
        if (brakePreview != null) brakePreview.gameObject.SetActive(true);

        currentBrakeAngle = 0f;

        isBraking = true;
        brakeElapsed = 0f;
        brakeHoldElapsed = 0f; // reset here
        preBrakeSpeed = currentForwardSpeed;
        hasRotatedDuringBrake = false;
        isBoostRecovering = false;

        pendingBrakeTurn = 0f;
        pendingLateralDir = 0;

        moveDirectionForward = transform.forward.normalized;
        lateralDir = 0;
        currentLateralSpeed = 0f;
        lateralHoldTime = 0f;
    }

    // 2) Replace EndBrakeAndStartBoost()
    private void EndBrakeAndStartBoost()
    {
        if (brakePreview != null) brakePreview.gameObject.SetActive(false);

        if (!isBraking) return;

        isBraking = false;

        // Apply stored brake angle (incremental) to the transform
        if (Mathf.Abs(currentBrakeAngle) > 0.01f)
        {
            transform.Rotate(Vector3.up, currentBrakeAngle);
            currentBrakeAngle = 0f;
        }

        // Kick off FOV recovery
        if (vcam != null)
        {
            StopAllCoroutines(); // stop any other FOV routines
            StartCoroutine(BoostFOVSequence());
        }

        // After rotating, update movement-forward to follow the transform's forward
        moveDirectionForward = transform.forward.normalized;

        // Commit lateral movement choice (if you still want lateral dash effect)
        // With incremental angles you might not need this, but I kept it
        lateralDir = pendingLateralDir;
        lateralHoldTime = lateralAccelTime;  // makes UpdateLateralSpeed evaluate to max
        pendingLateralDir = 0;

        // Clear rotation flags
        hasRotatedDuringBrake = false;

        // Start boost & recovery as before
        currentForwardSpeed = dmScript.DashSpeed;
        isBoostRecovering = true;
        boostRecoverElapsed = 0f;
    }
    private IEnumerator BoostFOVSequence()
    {
        if (vcam == null || followComp == null) yield break;

        // Save current values
        float startFOV = vcam.Lens.FieldOfView;
        Vector3 startOffset = followComp.FollowOffset;

        // Target offset = same X/Y, but Z pulled in by boostZOffsetDelta
        Vector3 targetOffset = defaultFollowOffset + new Vector3(0f, 0f, boostZOffsetDelta);

        // --- Step 1: zoom out and pull offset ---
        float elapsed = 0f;
        while (elapsed < boostFOVTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / boostFOVTime);

            vcam.Lens.FieldOfView = Mathf.Lerp(startFOV, boostFOV, t);
            followComp.FollowOffset = Vector3.Lerp(startOffset, targetOffset, t);

            yield return null;
        }
        vcam.Lens.FieldOfView = boostFOV;
        followComp.FollowOffset = targetOffset;

        // --- Step 2: smoothly return both values ---
        elapsed = 0f;
        float startBoostFOV = vcam.Lens.FieldOfView;
        Vector3 startBoostOffset = followComp.FollowOffset;

        while (elapsed < boostRecoverTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / boostRecoverTime);

            vcam.Lens.FieldOfView = Mathf.Lerp(startBoostFOV, defaultFOV, t);
            followComp.FollowOffset = Vector3.Lerp(startBoostOffset, defaultFollowOffset, t);

            yield return null;
        }
        vcam.Lens.FieldOfView = defaultFOV;
        followComp.FollowOffset = defaultFollowOffset;
    }
    private void StartSquash()
    {
        isSquashing = true;
        squashScaleElapsed = 0f;
        squashSlowElapsed = 0f;
        jumpCharge = 0f;
        isBoostRecovering = false; // cannot be recovering while squashing
    }

    private void CancelSquashWithoutJump()
    {
        // Start restoration of scale & forward speed
        // We'll use coroutines to smoothly recover the scale and forward speed.
        StartCoroutine(RecoverScale(originalLocalScale.y, squashRecoverTime, squashScaleCurve));
        StartCoroutine(RecoverForwardSpeed(currentForwardSpeed, forwardCruiseSpeed, squashSlowTime, squashSlowCurve));
        isSquashing = false;
        jumpCharge = 0f;
    }

    private IEnumerator RecoverScale(float targetY, float duration, AnimationCurve curve)
    {
       
        float startY = transform.localScale.y;
        float elapsed = 0f;
        Vector3 baseScale = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eval = curve.Evaluate(t);
            float newY = Mathf.Lerp(startY, targetY, eval);
            transform.localScale = new Vector3(baseScale.x, newY, baseScale.z);
            yield return null;
        }
        transform.localScale = new Vector3(baseScale.x, targetY, baseScale.z);
    }

    private IEnumerator RecoverForwardSpeed(float start, float target, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eval = curve.Evaluate(t);
            currentForwardSpeed = Mathf.Lerp(start, target, eval);
            yield return null;
        }
        currentForwardSpeed = target;
    }

    private void UpdateSpeedsAndStates(float dt)
    {
      
        // If we are jumping we only update jump vertical and keep forward at cruise
        if (isJumping)
        {
            // Apply gravity
            verticalVelocity += gravity * dt;

            // Move vertically
            Vector3 verticalMove = Vector3.up * verticalVelocity * dt;
            cc.Move(verticalMove);

            // Check if grounded after moving
            // Check if grounded after moving
            if (cc.isGrounded && verticalVelocity <= 0f)
            {
                isJumping = false;
                verticalVelocity = -1f; // small downward to keep grounded
                knockoverMode = false;  //  landed, turn off knockover
            }
            // Still update lateral while in air
            UpdateLateralSpeed(dt);
            return;
        }

        // ----- BRAKE logic ----- //
        if (isBraking)
        {
            brakeElapsed += dt;
            brakeHoldElapsed += dt;

            // Fonz - determine brake hold time based on current speed
            float tempBrakeVal = (float)numSpeedIncreases / 4f; // 0 to 1 based on number of increases
            brakeHoldTime = Mathf.Lerp(brakeHoldMaxTime, brakeHoldMinTime, tempBrakeVal);

            float t = Mathf.Clamp01(brakeElapsed / Mathf.Max(0.0001f, brakeTime));
            float eval = brakeCurve.Evaluate(t);
            currentForwardSpeed = Mathf.Lerp(preBrakeSpeed, brakeTargetSpeed, eval);
            if (vcam != null)
            {
                float holdPercent = Mathf.Clamp01(brakeHoldElapsed / brakeHoldMaxTime);
                vcam.Lens.FieldOfView = Mathf.Lerp(defaultFOV, squashFOV, holdPercent);
            }
            // Auto-release if held too long
            if (brakeHoldElapsed >= brakeHoldTime)
            {
                EndBrakeAndStartBoost();
                targetDutch = 0f;

                if (vcam != null)
                    StartCoroutine(RecoverFOV(defaultFOV, fovRecoverTime));
            }
        }

        else if (isBoostRecovering)
        {
            boostRecoverElapsed += dt;
            float t = Mathf.Clamp01(boostRecoverElapsed / Mathf.Max(0.0001f, boostRecoverTime));
            float eval = boostRecoverCurve.Evaluate(t);
            // Lerp from boostSpeed -> forwardCruiseSpeed using curve (curve should go 0->1 mapping)
            currentForwardSpeed = Mathf.Lerp(boostSpeed, forwardCruiseSpeed, eval);
            if (boostRecoverElapsed >= boostRecoverTime && dmScript.reachedMaxSpeed == true)
            {
                Debug.Log("perma boost");
                isBoostRecovering = false;
                forwardCruiseSpeed = forwardCruiseSpeed * speedPercentageIncrease; //Fonz - increase speed after each boost
                numSpeedIncreases += 1; // Fonz - count number of speed increases
                currentForwardSpeed = forwardCruiseSpeed;
            }
        }
        // ----- SQUASH logic ----- //
        else if (isSquashing)
        {
            // scale transition
            squashScaleElapsed += dt;
            float tScale = Mathf.Clamp01(squashScaleElapsed / Mathf.Max(0.0001f, squashScaleTime));
            float scaleEval = squashScaleCurve.Evaluate(tScale);

            // visual squash (Y only)
            float newY = Mathf.Lerp(originalLocalScale.y, squashScaleY, scaleEval);
            transform.localScale = new Vector3(originalLocalScale.x, newY, originalLocalScale.z);

            // collider squash (uniform shrink)
            cc.height = crouchHeight;
            cc.center = new Vector3(0, crouchHeight / 2f, 0); // keep bottom at same spot

            // slow forward speed toward the squash slow target
            squashSlowElapsed += dt;
            float tSlow = Mathf.Clamp01(squashSlowElapsed / Mathf.Max(0.0001f, squashSlowTime));
            float slowEval = squashSlowCurve.Evaluate(tSlow);
            currentForwardSpeed = Mathf.Lerp(currentForwardSpeed, squashSlowTargetSpeed, slowEval);

            // build jump charge (time based)
            jumpCharge += dt;
            if (jumpCharge >= maxJumpChargeTime)
            {
                // clamp charge at max time
                jumpCharge = maxJumpChargeTime;

                // compute height from charge percent
                float chargePercent = jumpCharge / maxJumpChargeTime;
                float jumpHeight = chargePercent * jumpMaxCharge;

                BeginJump(jumpHeight);
                isSquashing = false;

                // restore collider on jump
            }

            jumpCharge += dt;
            if (jumpCharge >= maxJumpChargeTime)
            {
                jumpCharge = maxJumpChargeTime;
                float chargePercent = jumpCharge / maxJumpChargeTime;
                float jumpHeight = chargePercent * jumpMaxCharge;

                BeginJump(jumpHeight);
                isSquashing = false;
            }

            //  Sync camera FOV to squash charge progress
            if (vcam != null)
            {
                float chargePercent = Mathf.Clamp01(jumpCharge / maxJumpChargeTime);
                vcam.Lens.FieldOfView = Mathf.Lerp(defaultFOV, squashFOV, chargePercent);
            }
        }
        else
        {
            cc.height = normalHeight;
            cc.center = new Vector3(0, normalHeight / 2f, 0);
            // Normal cruising
            currentForwardSpeed = forwardCruiseSpeed;
        }
        // place near the end of UpdateSpeedsAndStates() (or after braking/boost/squash decision)
        if (!isBraking && !isJumping)
        {
            // ensure movement aligns with the transform normally
            moveDirectionForward = transform.forward.normalized;
        }
        if (isBraking)
        {
            // No sideways sliding while braking
            currentLateralSpeed = 0f;
        }
     

        // Lateral acceleration (works in all non-jump states too, and while jumping)
        UpdateLateralSpeed(dt);
    }
   
    private void UpdateLateralSpeed(float dt)
    {
        if (lateralDir != 0)
        {
            float t = Mathf.Clamp01(lateralHoldTime / Mathf.Max(0.0001f, lateralAccelTime));
            float eval = lateralAccelCurve.Evaluate(t);
            currentLateralSpeed = Mathf.Lerp(lateralBaseSpeed, lateralMaxSpeed, eval);
        }
        else
        {
            // decay lateral speed to zero for a smooth stop
            currentLateralSpeed = Mathf.MoveTowards(currentLateralSpeed, 0f, lateralReleaseDamping * dt);
        }
    }

    private void ApplyMovement(float dt)
    {
        // Use the stored movement-forward vector for forward motion (so it's unaffected while braking)
        Vector3 forward = moveDirectionForward * currentForwardSpeed;

        // Compute a right vector based on moveDirectionForward so lateral stays consistent with the movement direction.
        Vector3 right = Vector3.Cross(Vector3.up, moveDirectionForward).normalized;
        Vector3 lateral = right * (lateralDir * currentLateralSpeed);

        Vector3 movement = (forward + lateral) * dt;

        cc.Move(movement);
    }


    // ---------------- JUMP -------------------
    private void BeginJump(float chargeAmount)
    {
        if (isJumping) return;

        isJumping = true;
        jumpCharge = 0f;

        // Convert charge into a jump velocity. 
        verticalVelocity = Mathf.Sqrt(2f * -gravity * chargeAmount);

        // Restore scale when jumping
        StartCoroutine(RecoverScale(originalLocalScale.y, squashRecoverTime, squashScaleCurve));

        // Ensure forward speed is reset to normal during jump
        currentForwardSpeed = forwardCruiseSpeed;
    }

    private void UpdateJump(float dt)
    {
        if (!isJumping) return;

        jumpElapsed += dt;
        float t = Mathf.Clamp01(jumpElapsed / Mathf.Max(0.00001f, jumpDuration));
        float heightFraction = jumpCurve.Evaluate(t); // 0..1 -> height fraction (curve should do up then down)
        float desiredY = jumpStartY + jumpTargetHeight * heightFraction;
        float currentY = transform.position.y;
        float deltaY = desiredY - currentY;

        // Move vertical delta using CharacterController.Move
        cc.Move(Vector3.up * deltaY);

        // finish?
        if (jumpElapsed >= jumpDuration)
        {
            // Ensure landing exactly at ground Y
            float finalDelta = jumpStartY - transform.position.y;
            if (Mathf.Abs(finalDelta) > 0.0001f)
            {
                cc.Move(Vector3.up * finalDelta);
            }

            isJumping = false;
            jumpCharge = 0f;
            // ensure forward speed restored
            currentForwardSpeed = forwardCruiseSpeed;
        }
    }

    // (Optional) draw debug info in inspector
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * 2f);
    }

    // Fonz - added for my TestUI
    public float GetCurrentForwardSpeed()
    {
        return currentForwardSpeed;
    }

    // Fonz - added for my TestUI
    public float GetBrakeHoldElapsed()
    {
        return brakeHoldElapsed;
    }

    // Fonz - reset speed function, called on hitting an obstacle, used to reset speed to base value
    public void resetSpeed()
    {
        forwardCruiseSpeed = 12;
        numSpeedIncreases = 0; // reset speed increases
    }
}
