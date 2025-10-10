using CharonsCorner.Runtime;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerController playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;

    private InputActions _inputActions;

    void Start()
    {
        _inputActions = new InputActions();
        _inputActions.Enable();

        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.getCameraTarget());
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
            Crouch = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None

        };

        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody();
    }


    void LateUpdate()
    {
        playerCamera.UpdatePosition(playerCharacter.getCameraTarget());
    }
}
