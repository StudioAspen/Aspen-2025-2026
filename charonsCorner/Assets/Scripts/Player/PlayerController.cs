using UnityEngine;
using KinematicCharacterController;

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
    public Vector3 Acceleration;
}

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Sneak;
}

public enum CrouchInput
{
    None, Toggle
}

public enum Stance
{
    Move, Sneak, Slide
}


public class PlayerController : MonoBehaviour, ICharacterController
{
    [Header("Refrences: ")]
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Transform root;
    [Space]

    [Header("Movement Settings: ")]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float walkAcceleration = 25f;
    [Space]
    [SerializeField] private float sneakSpeed = 7f;
    [SerializeField] private float sneakAcceleration = 20f;
    [Space]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;
    [Space]

    [Header("Aerial Settings: ")]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Space]

    [Header("Ball Visuals: ")]
    [SerializeField] private float ballRadius = 1f;
    [SerializeField] private float rotationSpeedMultiplier = 2f;
    [SerializeField] private float facingLerpSpeed = 2f;
    [Space]



    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;

    private bool _requestedJump;
    private bool _requestedSustainedJump;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequested;
    private bool _ungroundedDueToJump;

    private bool _requestedSneak;
    private bool _requestedSneakInAir;


    public CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;


    private Stance _stance;

    public void Initialize()
    {
        _state.Stance = Stance.Move;
        _lastState = _state;

        motor.CharacterController = this;
    }

    //Called Every Frame From Main Loop:
    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;


        //Take 2D Input Vector --> 3D Vector on the XZ-plane:
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        //Clamp Magnitude, Fixes Diagonal Movement being faster than vertical and horizontal:
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        //Orientate so that movement is relative to Player direction:
        _requestedMovement = input.Rotation * _requestedMovement;


        var wasRequesingJump = _requestedJump;
        _requestedJump = _requestedJump || input.Jump;
        if (_requestedJump && !wasRequesingJump) _timeSinceJumpRequested = 0f;

        _requestedSustainedJump = input.JumpSustain;


        var wasRequestingSneak = _requestedSneak;
        _requestedSneak = input.Sneak switch
        {
            CrouchInput.Toggle => !_requestedSneak,
            CrouchInput.None => _requestedSneak,
            _ => _requestedSneak
        };

        if (_requestedSneak && !wasRequestingSneak)
        {
            _requestedSneakInAir = !_state.Grounded;
        }
        else if (!_requestedSneak && wasRequestingSneak)
        {
            _requestedSneak = false;
        }
    }

    public void UpdateBody()
    {
        //FUNCTION USED TO UPDATE BALL ROTATION:

        //Get camera facing direction:
        Vector3 cameraForward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        ).normalized;

        //Allign Root Forward Orientation with Camera Forward:
        if (cameraForward.sqrMagnitude > 0.0001f)
        {
            Quaternion targetFacing = Quaternion.LookRotation(cameraForward, motor.CharacterUp);
            float t = 1f - Mathf.Exp(-facingLerpSpeed * Time.deltaTime);
            root.rotation = Quaternion.Slerp(root.rotation, targetFacing, t);
        }

        //Rolling Rotation based on Velocity:
        Vector3 velocity = motor.Velocity;
        Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, motor.CharacterUp);

        //When Moving:
        if (planarVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 rollAxis = Vector3.Cross(motor.CharacterUp, planarVelocity.normalized);
            float angularDistance = (planarVelocity.magnitude * Time.deltaTime / ballRadius) * Mathf.Rad2Deg;

            Quaternion rollRotation = Quaternion.AngleAxis(angularDistance * rotationSpeedMultiplier, rollAxis);

            //Apply Rotation:
            root.rotation = rollRotation * root.rotation;
        }

    }
    //-----------------------------------------------------------------------------------------------------------------------------------------------
    //NOTE: Kinematic Charactor Motor Functions ARE CALLED EVERY PHYSICS TICK:

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        _state.Acceleration = Vector3.zero;

        {
            ApplyBoostPanel(ref currentVelocity, deltaTime);

        }

        //If Grounded:
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0f;
            _ungroundedDueToJump = false;

            //Get Movement Projected onto the Ground, to ensure Player is ALWAYS GROUNDED:
            //getDirectionTangentToSurface returntype = "Unit Vector"
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;


            //Sloped Movement:
            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var sneaking = _state.Stance is Stance.Sneak;
                var wasMoveing = _lastState.Stance is Stance.Move;
                var wasInAir = !_lastState.Grounded;

                if (moving && (wasMoveing || wasInAir) && currentVelocity.y < 0f)
                {
                    _requestedSneak = true;
                    _state.Stance = Stance.Slide;

                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            vector: _lastState.Velocity,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        );
                    }

                    var effectiveSlideStartSpeed = slideStartSpeed;
                    if (!_state.Grounded && !_requestedSneakInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                        _requestedSneakInAir = false;
                    }
                    var slideSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);

                    //Snap currentVelocity to ground Surface:
                    currentVelocity = motor.GetDirectionTangentToSurface
                    (
                        direction: currentVelocity,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * slideSpeed;

                    Debug.DrawRay(transform.position, currentVelocity, Color.green, 5f);

                }
            }

            //General Movement:
            if (_state.Stance is Stance.Move or Stance.Sneak)
            {
                var speed = _state.Stance is Stance.Move ? walkSpeed : sneakSpeed;
                var acceleration = _state.Stance is Stance.Move ? walkAcceleration : sneakAcceleration;

                var targetVelocity = groundedMovement * speed;
                var moveVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-acceleration * deltaTime)
                );
                _state.Acceleration = moveVelocity - currentVelocity;

                currentVelocity = moveVelocity;
            }
            //Continue Sloped Movement:
            else
            {
                //Friction:
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                //Slope Acceleration:
                {
                    //downwards force, along characterUp Axis:
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: -motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;
                    currentVelocity -= force * deltaTime;

                }

                //Steering:
                {
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentSpeed;
                    var steerVelocity = currentVelocity;
                    var steerForce = (targetVelocity - steerVelocity) * slideSteerAcceleration * deltaTime;

                    //Add SteeringForce and Clamp Speed to avoid wrong acceleration increase:
                    steerVelocity += steerForce;
                    steerVelocity = Vector3.ClampMagnitude(steerVelocity, currentSpeed);

                    _state.Acceleration = (steerVelocity - currentVelocity) / deltaTime;
                    currentVelocity = steerVelocity;
                }

                //End Sloped Movement When below Speed threshold:
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    _state.Stance = Stance.Move;
                    _requestedSneak = false;
                }
            }

        }
        //Not Grounded:
        else
        {
            _timeSinceUngrounded += deltaTime;

            //Keep the Sliding State While in the air:
            if (_state.Stance == Stance.Slide)
            {
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    _state.Stance = Stance.Sneak;
                }
            }

            //Aerial Movement:
            if (_requestedMovement.sqrMagnitude > 0f)
            {
                //Requested Movement on XZ-plane:
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * _requestedMovement.magnitude;

                //Current Velocity on XZ-plane:
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                //Movement Force:
                var movementForce = planarMovement * airAcceleration * deltaTime;

                //Moving Slower than SlideSpeed:
                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                    //Limit Velocity to Air Speed:
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );
                    movementForce = constrainedMovementForce;
                }

                //Steep Slope Check:
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    //Note: if Dot > 0 --> Moving in Direction you are already moving.              [Bad]
                    //Note: if Dot == 0 or Dot < 0, --> Player is trying to steer in new direction. [Good]
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {
                        //Allows all movement directions besides forward:
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross
                            (
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )
                        ).normalized;

                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }


                currentVelocity += movementForce;
            }


            //Gravity Logic:
            var effectiveGravity = gravity;

            //Ensure When at peak jump height, jumpSustainGravity is disabled (ONLY WHEN MOVING UP):
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (_requestedSustainedJump && verticalSpeed > 0f) effectiveGravity *= jumpSustainGravity;

            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        //If Jumping:
        if (_requestedJump)
        {

            var grounded = motor.GroundingStatus.IsStableOnGround;
            var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;

            if (grounded || canCoyoteJump)
            {
                _requestedJump = false;

                if (_state.Stance != Stance.Slide)
                {
                    _requestedSneak = false;
                    _requestedSneakInAir = false;
                }

                //Unstick the Character Motor From the Ground:
                motor.ForceUnground(time: 0f);
                _ungroundedDueToJump = true;

                //Set Minimum Vertical Speed --> jumpSpeed:
                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            //NOT ALLOW JUMPING:
            else
            {
                _timeSinceJumpRequested += deltaTime;

                //Not allow jumping until coyoteTime Window has passed:
                var canJumpLater = _timeSinceJumpRequested < coyoteTime;
                _requestedJump = canJumpLater;
            }

        }
    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //Get The Direction Camera is Facing, Then Project on a FLAT PLANE:
        var forward = Vector3.ProjectOnPlane
        (
            _requestedRotation * Vector3.forward,
            motor.CharacterUp
        );

        if (forward != Vector3.zero) currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
        _tempState = _state;

        //Crouching Logic:
        if (_requestedSneak && _state.Stance is Stance.Move)
        {
            _state.Stance = Stance.Sneak;
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        //Uncrouching Logic:
        if (!_requestedSneak && _state.Stance is not Stance.Move)
        {
            _state.Stance = Stance.Move;
        }

        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;
        _lastState = _tempState;
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public bool IsColliderValidForCollisions(Collider coll) => true;

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }


    public Transform getCameraTarget() => cameraTarget;

    public CharacterState GetState() => _state;
    public CharacterState GetLastState() => _lastState;

    //-----------------------------------------------------------------------------------------------------------------------------------

    //Visualization Of Player Hit Box:
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(root.position, 1);
    }

    public void ApplyBoostPanel(ref Vector3 currentVelocity, float deltaTime)
    {
        Collider[] hitColliders = Physics.OverlapSphere(root.position, 1);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Boost Panel"))
            {
                BoostPanel boostPanel = hitCollider.GetComponent<BoostPanel>();
                if (boostPanel != null)
                {
                    Vector3 direction = boostPanel.transform.forward.normalized;
                    float force = walkSpeed * boostPanel.SpeedMultiplier;

                    currentVelocity += direction * force;

                    Debug.Log(_state.Stance);

                    if (_state.Stance is Stance.Move or Stance.Sneak)
                        _requestedSneak = true;
                    _state.Stance = Stance.Slide;

                    Debug.Log(_state.Stance);

                }
            }
        }
    }
}
