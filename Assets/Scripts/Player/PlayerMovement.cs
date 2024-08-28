using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _movingSpeed = 1f;
    [SerializeField]
    private float _smoothTime = 0.3f;
    [SerializeField]
    private float _rotationSpeed = 1f;

    private Vector2 _smoothedMovementInput;
    private Vector2 _movementInputSmoothVelocity;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector2 movementInput)
    {
        _smoothedMovementInput = Vector2.SmoothDamp(_smoothedMovementInput, movementInput, ref _movementInputSmoothVelocity, _smoothTime);
        _rigidBody.velocity = _smoothedMovementInput * _movingSpeed;

        if (movementInput != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, _smoothedMovementInput);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            _rigidBody.MoveRotation(rotation);
        }
    }
}
