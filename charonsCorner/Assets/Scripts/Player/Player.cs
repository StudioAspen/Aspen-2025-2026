using CharonsCorner.Runtime;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Refrences: ")]
    [SerializeField] private PlayerController playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;

    private InputActions _inputActions;

    void Start()
    {
        _inputActions = new InputActions();
        _inputActions.Enable();

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.getCameraTarget());

        cameraSpring.Initialize();
        cameraLean.Initialize();
    }

    void Update()
    {
        var input = _inputActions.Gameplay;

        //Get Camera + Update Rotation:
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
        playerCamera.UpdateRotation(cameraInput);

        //Get Character + Update:
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPressedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Sneak = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None

        };

        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody();
    }


    void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var cameraTarget = playerCharacter.getCameraTarget();
        var state = playerCharacter.GetState();

        playerCamera.UpdatePosition(cameraTarget);
        cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
        cameraLean.UpdateLean(deltaTime, state.Stance is Stance.Slide, state.Acceleration, cameraTarget.up);
    }
}
