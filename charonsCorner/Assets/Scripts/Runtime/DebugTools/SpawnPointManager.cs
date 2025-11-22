using System;
using Codice.Client.BaseCommands.Download;
using TMPro;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class SpawnPointManager : Singleton<SpawnPointManager>
    {
        [SerializeField] private TextMeshProUGUI spawnPointText;
        
        private GameObject[] spawnPoints;
        private int currentSpawnPoint = 0;

        public Action<Vector3> OnRespawn;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
            UpdateUI();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Q))
            {
                currentSpawnPoint--;
                
                if(currentSpawnPoint < 0)
                    currentSpawnPoint = spawnPoints.Length - 1;
                
                UpdateUI();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.E))
            {
                currentSpawnPoint = (currentSpawnPoint + 1) % spawnPoints.Length;
                UpdateUI();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                OnRespawn?.Invoke(spawnPoints[currentSpawnPoint].transform.position);
            }
        }

        private void UpdateUI()
        {
            spawnPointText.text = spawnPoints[currentSpawnPoint].name;
        }
    }
}
