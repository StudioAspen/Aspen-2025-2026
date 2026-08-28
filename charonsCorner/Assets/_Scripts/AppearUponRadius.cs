using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

public class AppearUponRadius : MonoBehaviour, MMEventListener<MMGameEvent>
{
    [SerializeField] private MMSpringScale _springScale;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private LayerMask _playerLayer;

    [Header("Events")]
    [SerializeField] private string _eventName = "";

    private Vector3 _startingScale;
    private bool _activated = false;

    private void OnValidate()
    {
        if (_springScale == null)
        {
            _springScale = GetComponent<MMSpringScale>();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_springScale == null)
        {
            _springScale = GetComponent<MMSpringScale>();
        }

        if (_springScale != null)
        {
            _startingScale = _springScale.TargetVector3;
            Debug.Log($"[AppearUponRadius] {gameObject.name} Starting Scale: {_startingScale}");
            _springScale.TargetVector3 = Vector3.zero;
            _springScale.Stop();
        }
        else
        {
            Debug.LogWarning($"[AppearUponRadius] {gameObject.name} MMSpringScale not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_activated || _springScale == null) return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _playerLayer);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Player"))
            {
                TriggerAppear();
                break;
            }
        }
    }

    private void TriggerAppear()
    {
        if (_activated || _springScale == null) return;

        _springScale.MoveTo(_startingScale);
        _activated = true;
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (string.IsNullOrEmpty(_eventName)) return;

        if (gameEvent.EventName == _eventName)
        {
            TriggerAppear();
        }
    }

    private void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
