using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private UpdateMode _updateMode;

        [field: SerializeField, ReadOnly] public Vector3 StartingPositionOffset { get; private set; }
        [field: SerializeField] public Vector3 PositionOffset { get; private set; }
    
        public enum UpdateMode
        {
            Default,
            Fixed,
            Late
        }

        private void Awake()
        {
            SetStartingPositionOffset(PositionOffset);
        }

        public void Init(Transform target, UpdateMode updateMode = default)
        {
            _target = target;
            _updateMode = updateMode;
        }
    
        private void Update()
        {
            if (_updateMode != UpdateMode.Default)
                return;

            Follow();
        }

        private void FixedUpdate()
        {
            if (_updateMode != UpdateMode.Fixed)
                return;
        
            Follow();
        }

        private void LateUpdate()
        {
            if (_updateMode != UpdateMode.Late)
                return;
        
            Follow();
        }

        [Button("Teleport to Follow Position", ButtonSizes.Large)]
        private void Follow()
        {
            if (_target == null)
                return;
        
            transform.position = _target.position + PositionOffset;
        }

        public void SetPositionOffset(Vector3 newOffset)
        {
            PositionOffset = newOffset;
        }

        public void SetStartingPositionOffset(Vector3 newStartingPositionOffset)
        {
            StartingPositionOffset = newStartingPositionOffset;
        }
    }
}