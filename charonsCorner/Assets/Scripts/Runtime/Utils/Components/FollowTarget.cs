using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class FollowTarget : MonoBehaviour
    {
        private Transform _target;
        private UpdateMode _updateMode;
    
        public enum UpdateMode
        {
            Default,
            Fixed,
            Late
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

        private void Follow()
        {
            if (_target == null)
                return;
        
            transform.position = _target.position;
        }
    }
}