using System;
using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    /// <summary>
    /// Helper component to easily hook up events for a gameobject
    /// </summary>
    public class MonobehaviourEvents : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent OnAwake { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnStart { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnOnEnable { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnOnDisable { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnOnDestroy { get; private set; } = new();
        [field: SerializeField] public UnityEvent OnUpdate { get; private set; } = new();

        private void Awake()
        {
            OnAwake?.Invoke();
        }

        private void Start()
        {
            OnStart?.Invoke();
        }

        private void OnEnable()
        {
            OnOnEnable?.Invoke();
        }

        private void OnDisable()
        {
            OnOnDisable?.Invoke();
        }

        private void OnDestroy()
        {
            OnOnDestroy?.Invoke();
        }

        private void Update()
        {
            OnUpdate?.Invoke();
        }
    }
}