using System;
using CharonsCorner.LevelEditor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class PlayerWrongWayDetector : MonoBehaviour
    {
        private GameplayPlayerController _playerController;
        [SerializeField, Required] private SplinePathDirection _directionChecker;

        [Header("Config")]
        [SerializeField] private float _checkRate = 1f;
        [field: SerializeField, ReadOnly] public bool IsWrongWay { get; private set; }
        [field: SerializeField] public UnityEvent<bool> OnWrongWayChanged { get; private set; } = new();
        private float _checkTimer;

        private void Awake()
        {
            if(_directionChecker == null) 
                Debug.LogError("Please make sure SplinePathDirection is assigned to the PlayerWrongWayDetector");
            
            _playerController = GetComponent<GameplayPlayerController>();
        }
        
        private void Update()
        {
            HandleCheckTick();
        }

        private void HandleCheckTick()
        {
            _checkTimer += Time.deltaTime;
            if (_checkTimer >= _checkRate)
            {
                _checkTimer = 0;
                OnTick();
            }
        }

        private void OnTick()
        {
            // If player is moving too slowly, don't care about wrong way yet
            if (_playerController.Rb.linearVelocity.WithY(0).magnitude < 1f)
            {
                ChangeWrongWay(false);
                return;
            }
            
            ChangeWrongWay(_directionChecker.CheckWrongWayFromPosition(transform.position, _playerController.Rb.linearVelocity.WithY(0)));
        }

        private void ChangeWrongWay(bool isWrongWay)
        {
            if (IsWrongWay == isWrongWay)
                return;
            
            IsWrongWay = isWrongWay;
            OnWrongWayChanged.Invoke(isWrongWay);
        }
    }
}