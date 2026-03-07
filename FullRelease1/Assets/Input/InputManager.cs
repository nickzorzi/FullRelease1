using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static Vector2 Look;
    public static Vector2 Move;
    public static bool SprintPressed { get; private set; }
    public static bool JumpPressed { get; private set; }
    public static bool JumpHeld { get; private set; }
    public static bool InteractPressed { get; private set; }
    public static bool EquipPressed { get; private set; }
    public static bool ThrowPressed { get; private set; }
    public static bool FocusPressed { get; private set; }
    public static bool PlayerZoomPressed { get; private set; }
    public static bool PlayerZoomHeld { get; private set; }
    public static bool Mouse1Pressed { get; private set; }
    public static bool Mouse1Held { get; private set; }



    private PlayerInput _playerInput;
    private InputAction _lookAction;
    private InputAction _moveAction;
    private InputAction _sprintAction;
    private InputAction _jumpAction;

    private InputAction _interactAction;
    private InputAction _equipAction;
    //private InputAction _throwAction;
    //private InputAction _focusAction;

    private InputAction _zoomAction;
    private InputAction _mouse1Action;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _lookAction = _playerInput.actions["Look"];
        _moveAction = _playerInput.actions["Move"];
        _sprintAction = _playerInput.actions["Sprint"];
        _jumpAction = _playerInput.actions["Jump"];
        _interactAction = _playerInput.actions["Interact"];
        _equipAction = _playerInput.actions["Equip"];
        //_throwAction = _playerInput.actions["Throw"];
        //_focusAction = _playerInput.actions["Focus"];
        _zoomAction = _playerInput.actions["PlayerZoom"];
        _mouse1Action = _playerInput.actions["Mouse1"];
    }

    private void Update()
    {
        Look = _lookAction.ReadValue<Vector2>();
        Move = _moveAction.ReadValue<Vector2>();

        SprintPressed = _sprintAction.ReadValue<float>() > 0f;

        JumpPressed = _jumpAction.triggered;
        JumpHeld = _jumpAction.ReadValue<float>() > 0f;

        InteractPressed = _interactAction.triggered;

        EquipPressed = _equipAction.triggered;

        //ThrowPressed = _throwAction.triggered;

        //FocusPressed = _focusAction.IsPressed();

        PlayerZoomPressed = _zoomAction.triggered;
        PlayerZoomHeld = _zoomAction.ReadValue<float>() > 0f;

        Mouse1Pressed = _mouse1Action.triggered;
        Mouse1Held = _mouse1Action.ReadValue<float>() > 0f;
    }
}
