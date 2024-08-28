using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerInputHandler _inputHandler;
    private TowerRotation _towerRotation;
    private PlayerMovement _playerMovement;

    private void Start()
    {
        _inputHandler = GetComponent<PlayerInputHandler>(); 
        _towerRotation = GetComponentInChildren<TowerRotation>();  
        _playerMovement = GetComponent<PlayerMovement>(); 
    }

    private void FixedUpdate()
    {
        MovePlayer();
        RotateTower();
    }

    private void MovePlayer()
    {
        _playerMovement.Move(_inputHandler.MovementInput);
    }

    private void RotateTower()
    {
        _towerRotation.Rotate(_inputHandler.MousePosition);
    }
}
