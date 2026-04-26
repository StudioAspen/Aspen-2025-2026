using System;
using System.Collections.Generic;
using CharonsCorner.LevelEditor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    [RequireComponent(typeof(GameplayPlayerController))]
    public class PlayerWrongWayDetector : MonoBehaviour
    {
        private GameplayPlayerController _playerController;

        [FormerlySerializedAs("_directionChecker")]
        [SerializeField] private SplinePathDirection _legacyDirectionChecker;

        [SerializeField, Required] private List<SplinePathDirection> _directionCheckers = new List<SplinePathDirection>();

        [Header("Config")]
        [SerializeField] private float _checkRate = 1f;
        [SerializeField, Tooltip("Must be wrong-way for this long before setting IsWrongWay=true.")]
        private float _wrongWayConfirmTime = 0.35f;
        [SerializeField, Tooltip("Must be correct-way for this long before clearing IsWrongWay.")]
        private float _recoverConfirmTime = 0.2f;

        [field: SerializeField, ReadOnly] public bool IsWrongWay { get; private set; }
        [field: SerializeField] public UnityEvent<bool> OnWrongWayChanged { get; private set; } = new();
        private float _checkTimer;
        private float _wrongWayTimer;
        private float _recoverTimer;

        private void Awake()
        {
            if (_legacyDirectionChecker != null && !_directionCheckers.Contains(_legacyDirectionChecker))
            {
                _directionCheckers.Add(_legacyDirectionChecker);
            }

            if (_directionCheckers.Count == 0)
                Debug.LogError("Please make sure at least one SplinePathDirection is assigned to the PlayerWrongWayDetector");

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
            Vector3 velocity = _playerController.Rb.linearVelocity;
            if (velocity.WithY(0).magnitude < 1f)
            {
                EvaluateWrongWay(false);
                return;
            }

            if (!TryGetClosestChecker(transform.position, out SplinePathDirection closestChecker))
            {
                EvaluateWrongWay(false);
                return;
            }

            bool isWrongWayNow = closestChecker.CheckWrongWayFromPosition(transform.position, velocity);
            EvaluateWrongWay(isWrongWayNow);
        }

        private bool TryGetClosestChecker(Vector3 worldPosition, out SplinePathDirection closestChecker)
        {
            closestChecker = null;
            float nearestDist = float.MaxValue;

            for (int i = 0; i < _directionCheckers.Count; i++)
            {
                SplinePathDirection checker = _directionCheckers[i];
                if (checker == null)
                    continue;

                if (!checker.TryGetNearestDistanceSqr(worldPosition, out float distSqr))
                    continue;

                if (distSqr < nearestDist)
                {
                    nearestDist = distSqr;
                    closestChecker = checker;
                }
            }

            return closestChecker != null;
        }

        private void EvaluateWrongWay(bool isWrongWayNow)
        {
            if (isWrongWayNow)
            {
                _wrongWayTimer += _checkRate;
                _recoverTimer = 0f;

                if (!IsWrongWay && _wrongWayTimer >= _wrongWayConfirmTime)
                    ChangeWrongWay(true);
            }
            else
            {
                _recoverTimer += _checkRate;
                _wrongWayTimer = 0f;

                if (IsWrongWay && _recoverTimer >= _recoverConfirmTime)
                    ChangeWrongWay(false);

                if (!IsWrongWay)
                    ChangeWrongWay(false);
            }
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