using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : SingletonBehaviour<GameInput>
{
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    public Vector2 GetMovmentVector()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        return inputVector;
    }

    public Vector2 GetRotateVector() 
    { 
        Vector2 rotateVector = playerInputActions.Player.Look.ReadValue<Vector2>();
        return rotateVector;
    }
}
