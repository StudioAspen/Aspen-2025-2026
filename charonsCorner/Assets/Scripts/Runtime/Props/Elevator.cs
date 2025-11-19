using System;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class Elevator : MonoBehaviour
    {
        [SerializeField] private Transform elevator;
        [SerializeField] private Transform startPos, endPos;
        [SerializeField] private float moveSpeed = 0.5f, arrivalRadius = 0.25f;
        [SerializeField] private bool playerOnElevator;
        private void Update()
        {
            if (Vector3.Distance(elevator.position, endPos.position) < arrivalRadius && playerOnElevator) return;

            if (Vector3.Distance(elevator.position, startPos.position) < arrivalRadius && !playerOnElevator) return;
            
            if (playerOnElevator)
            {
                elevator.position = Vector3.Lerp(elevator.position, endPos.position, moveSpeed * Time.deltaTime);
            }
            else
            {
                elevator.position = Vector3.Lerp(elevator.position, startPos.position, moveSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out GameplayPlayerController player))
            {
                playerOnElevator = true;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out GameplayPlayerController player))
            {
                playerOnElevator = false;
            }
        }

    }
}
