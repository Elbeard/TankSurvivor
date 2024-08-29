using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAwarenessController : MonoBehaviour
{
    public bool AwareOfPlayer {  get; private set; }

    public Vector2 DirectionToPlayer { get; private set; }

    [SerializeField]
    private float _playerAwarenessDistance = 1f;
    private Transform _playerTransform;

    private void Awake()
    {
        _playerTransform = FindObjectOfType<Player>().transform;
    }

    private void FixedUpdate()
    {
        Vector2 enemyToPlayerVector = _playerTransform.position - transform.position;
        DirectionToPlayer = enemyToPlayerVector.normalized;

        if(enemyToPlayerVector.magnitude <= _playerAwarenessDistance )
        {
            AwareOfPlayer = true;
        }
        else
        {
            AwareOfPlayer = false;
        }
    }
}
