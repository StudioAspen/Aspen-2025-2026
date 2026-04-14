using UnityEngine;
using CharonsCorner.Runtime;

public class JackInTheBox : MonoBehaviour
{
    [SerializeField] private GameObject _rotator;
    [SerializeField] private GameObject _head;
    
    private GameplayPlayerController _player;

    void Start()
    {
        _player = Object.FindFirstObjectByType<GameplayPlayerController>();
    }

    void Update()
    {
        if (_player == null) return;

        UpdateRotator();
        UpdateHead();
    }

    private void UpdateHead()
    {
        if (_head == null) return;
        Vector3 directionToPlayer = _player.transform.position - _head.transform.position;
        if (directionToPlayer.sqrMagnitude > 0.001f)
        {
            _head.transform.right = directionToPlayer;
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
