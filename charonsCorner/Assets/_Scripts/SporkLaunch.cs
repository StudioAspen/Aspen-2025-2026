using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Events;
using CharonsCorner.Runtime;

public class SporkLaunch : MonoBehaviour
{
    [SerializeField] private SplineContainer _splineContainer;
    [SerializeField] private float _launchRate = 10f;
    [SerializeField] private Vector3 _exitDirection = Vector3.forward;
    [SerializeField] private float _exitForce = 10f;
    [SerializeField] private UnityEvent _onLaunch;
    [SerializeField] private UnityEvent _onLaunchEnd;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<GameplayPlayerController>(out var player))
        {
            if (_splineContainer != null)
            {
                Vector3 force = transform.TransformDirection(_exitDirection.normalized) * _exitForce;
                player.SplineLaunchState.SetLaunchParameters(_splineContainer, _launchRate, force, () => _onLaunchEnd?.Invoke());
                player.StateMachine.ChangeState(player.SplineLaunchState);
                _onLaunch?.Invoke();
            }
            else
            {
                Debug.LogWarning("SporkLaunch: SplineContainer is not assigned!", this);
            }
        }
    }
}
