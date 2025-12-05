using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace CharonsCorner.Runtime
{
    public class SpawnPointManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _spawnPointText;
        private GameObject[] _spawnPoints;
        private int _currentSpawnPoint = 0;

        public event Action<Vector3> OnRespawn;
        
        void Start()
        {
            _spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            UpdateUI();
        }
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
            {
                _currentSpawnPoint--;
                
                if(_currentSpawnPoint < 0)
                    _currentSpawnPoint = _spawnPoints.Length - 1;
                
                UpdateUI();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.E))
            {
                _currentSpawnPoint = (_currentSpawnPoint + 1) % _spawnPoints.Length;
                UpdateUI();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnRespawn?.Invoke(_spawnPoints[_currentSpawnPoint].transform.position);
            }
        }

        private void UpdateUI()
        {
            _spawnPointText.text = _spawnPoints[_currentSpawnPoint].name;
        }
    }
}
