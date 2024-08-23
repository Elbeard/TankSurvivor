using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    private Rigidbody2D _rigidBody;
    [SerializeField]     
    private Transform _towerTransform;
    [SerializeField]
    private float _movingSpeed = 1f;
    [SerializeField]
    private float _rotationSpeed = 1f;
    [SerializeField]
    private float _smoothTime = 0.3F;
    private Vector2 _smoothedMovementInput = Vector2.zero;
    private Vector2 _movementInputSmoothVelocity = Vector2.zero;
    private Vector2 _movementInput = Vector2.zero;
    private Vector2 _mousePosition;
    private Vector3 _worldMousePosition;

    private void Awake()
    {
        _playerInputActions = new PlayerInputActions();
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        _playerInputActions.Enable();
    }

    private void OnDisable()
    {
        _playerInputActions.Disable();
    }

    private void FixedUpdate()
    {
        TowerRotation();
        SetPlayerVelocity();
        RotateInDirectionOfInput();
    }

    public void OnMove(InputValue value)
    {
        _movementInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        _mousePosition = value.Get<Vector2>();
    }

    private void RotateInDirectionOfInput()
    {
        if (_movementInput != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, _smoothedMovementInput);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            _rigidBody.MoveRotation(rotation);
        }
    }

    private void SetPlayerVelocity()
    {
        _smoothedMovementInput = Vector2.SmoothDamp(_smoothedMovementInput, _movementInput, ref _movementInputSmoothVelocity, _smoothTime * Time.fixedDeltaTime);
        _rigidBody.velocity = _smoothedMovementInput * _movingSpeed;
    }

    private void TowerRotation()
    {
        _worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, Camera.main.nearClipPlane));
        Vector3 direction = _worldMousePosition - transform.position;
        direction.z = 0; 
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _towerTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        Debug.Log($"_mousePosition ={_mousePosition}, _worldMousePosition = {_worldMousePosition}, direction = {direction}, angle = {angle}");
    }
}
