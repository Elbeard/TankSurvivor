using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed = 1f;

    [SerializeField]
    private float _rotationSpeed = 1f;

    private Rigidbody2D _rigidBody;
    private EnemyAwarenessController _enemyAwarenessController;
    private Vector2 _targetDirection;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _enemyAwarenessController = GetComponent<EnemyAwarenessController>();
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
    }

    private void UpdateTargetDirection()
    {
        if (_enemyAwarenessController.AwareOfPlayer)
        {
            _targetDirection = _enemyAwarenessController.DirectionToPlayer;
        }
        else
        {
            _targetDirection = Vector2.zero;
        }
    }

    private void RotateTowardsTarget()
    {
        if (_targetDirection == Vector2.zero)
        {
            return;
        }
        else
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, _targetDirection);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            _rigidBody.MoveRotation(rotation);
        }
    }

    private void SetVelocity()
    {
        if (_targetDirection == Vector2.zero)
        {
            _rigidBody.velocity = Vector2.zero;
        }
        else
        {
            _rigidBody.velocity = transform.up * _speed;
        }
    }
}
