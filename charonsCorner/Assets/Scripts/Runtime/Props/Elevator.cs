using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField] private Transform _elevator;
        [SerializeField] private Transform _startPos;
        [SerializeField] private Transform _endPos;
        [SerializeField] private float _moveSpeed = 0.5f;
        [SerializeField] private float _arrivalRadius = 0.25f;
        [SerializeField] private bool _playerOnElevator;
        private void Update()
        {
            if (Vector3.Distance(_elevator.position, _endPos.position) < _arrivalRadius && _playerOnElevator) return;

            if (Vector3.Distance(_elevator.position, _startPos.position) < _arrivalRadius && !_playerOnElevator) return;
            
            if (_playerOnElevator)
            {
                _elevator.position = Vector3.Lerp(_elevator.position, _endPos.position, _moveSpeed * Time.deltaTime);
            }
            else
            {
                _elevator.position = Vector3.Lerp(_elevator.position, _startPos.position, _moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GameplayPlayerController player))
            {
                _playerOnElevator = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GameplayPlayerController player))
            {
                _playerOnElevator = false;
            }
        }

    }
}
