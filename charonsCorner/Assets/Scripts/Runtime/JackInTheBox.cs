using UnityEngine;
using CharonsCorner.Runtime;

public class JackInTheBox : MonoBehaviour
{
    [SerializeField] private GameObject _rotator;
    [SerializeField] private GameObject _head;
    [SerializeField] private bool _isStaring = true;
    [SerializeField] private bool _headRotateX = true;
    [SerializeField] private bool _headRotateY = true;
    [SerializeField] private bool _headRotateZ = true;
    
    private GameplayPlayerController _player;

    public void SetIsStaring(bool value) => _isStaring = value;

    void Start()
    {
        _player = Object.FindFirstObjectByType<GameplayPlayerController>();
    }

    void Update()
    {
        if (_player == null || !_isStaring) return;

        UpdateRotator();
        UpdateHead();
    }

    private void UpdateHead()
    {
        if (_head == null) return;
        Vector3 directionToPlayer = _player.transform.position - _head.transform.position;
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            
            // Adjust so 'right' axis points towards player (original behavior was transform.right = directionToPlayer)
            targetRotation *= Quaternion.Euler(0, -90, 0); 
            
            Vector3 targetEuler = targetRotation.eulerAngles;
            Vector3 currentEuler = _head.transform.eulerAngles;

            float newX = _headRotateX ? targetEuler.x : currentEuler.x;
            float newY = _headRotateY ? targetEuler.y : currentEuler.y;
            float newZ = _headRotateZ ? targetEuler.z : currentEuler.z;

            _head.transform.rotation = Quaternion.Euler(newX, newY, newZ);
        }
    }

    private void UpdateRotator()
    {
        if (_rotator == null) return;
        Vector3 directionToPlayer = _player.transform.position - _rotator.transform.position;
        
        Vector3 localDirection = _rotator.transform.parent != null 
            ? _rotator.transform.parent.InverseTransformDirection(directionToPlayer) 
            : directionToPlayer;

        if (localDirection.x != 0 || localDirection.y != 0)
        {
            float angleDeg = Mathf.Atan2(localDirection.y, localDirection.x) * Mathf.Rad2Deg - 90f;
            _rotator.transform.localRotation = Quaternion.Euler(0, 0, angleDeg);
        }
    }

    private void OnDrawGizmosSelected()
    {
    }
}
