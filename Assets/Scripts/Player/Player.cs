using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private Transform playerTransform;
    [SerializeField]
    private float movingSpeed = 1f;
    [SerializeField]
    private float rotationSpeed = 1f;
    [SerializeField]
    private float smoothTime = 0.3F;
    private Vector2 smoothedMovementInput = Vector2.zero;
    private Vector2 movementInputSmoothVelocity = Vector2.zero;
    private Vector2 movementInput = Vector2.zero;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        playerTransform = GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        //HandleMovement();
        //TowerRotation();
        SetPlayerVelocity();
        RotateInDirectionOfInput();
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    private void RotateInDirectionOfInput()
    {
        if (movementInput != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, smoothedMovementInput);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            rigidBody.MoveRotation(rotation);
        }
    }

    private void SetPlayerVelocity()
    {
        smoothedMovementInput = Vector2.SmoothDamp(smoothedMovementInput, movementInput, ref movementInputSmoothVelocity, smoothTime * Time.fixedDeltaTime);
        rigidBody.velocity = smoothedMovementInput * movingSpeed;

    }

    private void HandleMovement()
    {
        Vector2 movenentVector = GameInput.Instance.GetMovmentVector();
        movenentVector = movenentVector.normalized;
        rigidBody.MovePosition(rigidBody.position + movenentVector * (movingSpeed * Time.fixedDeltaTime));
    }

    private void TowerRotation()
    {
        Vector3 rotateVector = GameInput.Instance.GetRotateVector();

        playerTransform.Rotate(Vector3.forward * rotateVector.x);
        Debug.Log(rotateVector);
    }
}
