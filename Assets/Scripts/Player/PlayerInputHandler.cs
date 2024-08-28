using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 MovementInput { get; private set; }
    public Vector2 MousePosition { get; private set; }

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();
    }

    public void OnMove(InputValue value)
    {
        MovementInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        MousePosition = value.Get<Vector2>();
        //Debug.Log($"MousePosition_PIH = {MousePosition}");
    }
}
