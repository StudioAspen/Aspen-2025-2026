using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class TeleportBumper : MonoBehaviour
    {
        [Header("Destination Parameters")]
        [SerializeField] private Transform _teleportDestination;
        [SerializeField] private Color _gizmoColor = Color.blue;

        [Header("Post-Destination Boost Parameters")]
        [SerializeField] private float _boostSpeedMultiplier = 2;
        [SerializeField] private BoostDirection _boostDirection;
        private Vector3 _boostDirectionVec = Vector3.back;

        [Header("Debug Parameters")]
        [SerializeField] private bool _debugModeOn = false;

        private void Start()
        {
            SelectedBoostDirection(_boostDirection);
        }

        private void Update()
        {
            if (_debugModeOn)
            {
                SelectedBoostDirection(_boostDirection);
            }
        }

        private enum BoostDirection {
            Back,
            Down, 
            Forward,
            Left,
            Right,
            Up
        }

        private void SelectedBoostDirection(BoostDirection direction)
        {
            switch (direction)
            {
                case BoostDirection.Back:
                    _boostDirectionVec = Vector3.back;
                    break;
                case BoostDirection.Down:
                    _boostDirectionVec = Vector3.down;
                    break;
                case BoostDirection.Forward:
                    _boostDirectionVec = Vector3.forward;
                    break;
                case BoostDirection.Left:
                    _boostDirectionVec = Vector3.left;
                    break;
                case BoostDirection.Right:
                    _boostDirectionVec = Vector3.right;
                    break;
                case BoostDirection.Up:
                    _boostDirectionVec = Vector3.up;
                    break;
            }
        }

        public Transform GetTeleportDestination() => _teleportDestination;
        public float GetBoostSpeedMultiplier() => _boostSpeedMultiplier;
        public Vector3 GetBoostDiection() => _boostDirectionVec;


        private void OnDrawGizmos()
        {
            if (_teleportDestination == null)
            {
                Debug.LogError("Missing teleport destination object");
                return;
            }

            const float GIZMO_RADIUS = 1.0f;

            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(_teleportDestination.position, GIZMO_RADIUS);
            Gizmos.DrawRay(_teleportDestination.position, _boostDirectionVec * 5f); 
        }

    }
}
