using UnityEngine;
using KinematicCharacterController;

public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;
}

public enum CrouchInput
{
    None, Toggle
}

public enum Stance
{
    Stand,Crouch
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
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float crouchAcceleration = 20f;

    [Space]

    [Header("Aireal Settings: ")]
    [SerializeField] private float jumpSpeed = 20f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;


    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedSustainedJump;
    private bool _requestedCrouch;

    private Stance _stance;

    public void Initialize()
    {
        _stance = Stance.Stand;

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


        _requestedJump = _requestedJump || input.Jump;
        _requestedSustainedJump = input.JumpSustain;

        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch
        };
    }

    public void UpdateBody()
    {
        //FUNCTION USED TO UPDATE BALL ROTATION:
        // will use: "root.localrotation = "

        //t = "1f - Mathf.Exp(-speed * deltaTime)
    }

    //-----------------------------------------------------------------------------------------------------------------------------------------------
    //NOTE: Kinematic Charactor Motor Functions ARE CALLED EVERY PHYSICS TICK:

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //If Grounded:
        if (motor.GroundingStatus.IsStableOnGround)
        {
            //Get Movement Projected onto the Ground, to ensure Player is ALWAYS GROUNDED:
            //getDirectionTangentToSurface returntype = "Unit Vector"
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            var speed = _stance is Stance.Stand ? walkSpeed : crouchSpeed;
            var acceleration = _stance is Stance.Stand ? walkAcceleration : crouchAcceleration;

            var targetVelocity = groundedMovement * speed;
            currentVelocity = Vector3.Lerp
            (
                a: currentVelocity,
                b: targetVelocity,
                t: 1f - Mathf.Exp(-acceleration * deltaTime)
            );
        }
        //Not Grounded:
        else
        {
            //Aireal Movement:
            if(_requestedMovement.sqrMagnitude > 0f)
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
                var targetPlanarVelocity = currentPlanarVelocity + movementForce;

                //Limit Velocity to Air Speed:
                targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                //Air Steering Force:
                currentVelocity += targetPlanarVelocity - currentPlanarVelocity;
            }


            //Gravity Logic:
            var effectiveGravity = gravity;

            //Ensure When at peak jump height, jumpSustainGravity is disabled (ONLY WHEN MOVING UP):
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            if (_requestedSustainedJump && verticalSpeed > 0f) effectiveGravity *= jumpSustainGravity;

            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        //If Jumping:
        if(_requestedJump)
        {
            _requestedJump = false;

            //Unstick the Character Motor From the Ground:
            motor.ForceUnground(time: 0f);

            //Set Minimum Vertical Speed --> jumpSpeed:
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);
            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
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
        //Crouching Logic:
        if (_requestedCrouch && _stance is Stance.Stand)
        {
            _stance = Stance.Crouch;
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        //Uncrouching Logic:
        if(!_requestedCrouch && _stance is not Stance.Stand)
        {
            _stance = Stance.Stand;
        }

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

}
