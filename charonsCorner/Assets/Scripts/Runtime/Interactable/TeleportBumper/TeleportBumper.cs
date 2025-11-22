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
        [SerializeField] private BoostDiection _boostDirection;
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

        private enum BoostDiection {
            Back,
            Down, 
            Forward,
            Left,
            Right,
            Up
        }

        void SelectedBoostDirection(BoostDiection direction)
        {
            switch (direction)
            {
                case BoostDiection.Back:
                    _boostDirectionVec = Vector3.back;
                    break;
                case BoostDiection.Down:
                    _boostDirectionVec = Vector3.down;
                    break;
                case BoostDiection.Forward:
                    _boostDirectionVec = Vector3.forward;
                    break;
                case BoostDiection.Left:
                    _boostDirectionVec = Vector3.left;
                    break;
                case BoostDiection.Right:
                    _boostDirectionVec = Vector3.right;
                    break;
                case BoostDiection.Up:
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
