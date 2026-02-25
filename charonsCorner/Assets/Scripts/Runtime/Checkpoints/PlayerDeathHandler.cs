using UnityEngine;
using UnityEngine.Events;

namespace CharonsCorner.Runtime
{
    public class PlayerDeathHandler : MonoBehaviour
    {
        [field: SerializeField] public GameObject CheckpointManagerObject;

        private float _timeToRespawn;
        private CheckpointManager _checkpointManager;
        private InputManager _inputManager;

        private void OnEnable() => DeathBox.OnPlayerDeath += TriggerRespawn;
        private void OnDisable() => DeathBox.OnPlayerDeath -= TriggerRespawn;
        

        private void Awake()
        {
            _checkpointManager = CheckpointManagerObject.GetComponent<CheckpointManager>();
            _inputManager = InputManager.Instance;
        }

        private void TriggerRespawn(DeathBox box)
        {
            _inputManager.DisableAllActions();
        }
    }
}
