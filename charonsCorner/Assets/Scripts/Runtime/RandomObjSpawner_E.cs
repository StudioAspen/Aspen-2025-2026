using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharonsCorner.Runtime
{
    public class RandomObjSpawner_E : MonoBehaviour
    {
        [SerializeField] GameObject[] ObjectsToSpawn;
        [SerializeField] GameObject TargetPosition;

        [Header("Spawner Settings")]
        [Tooltip("How long spawner is turned off and not spawning before it starts to spawn objects")]
        [SerializeField] float TimeBeforeSpawn = 5f;
        [Tooltip("How long spawning happens")]
        [SerializeField] float SpawnDuration = 5f;
        [Tooltip("Minimum time before another object spawns")]
        [SerializeField] float minSpawnInterval = 1.35f;
        [Tooltip("Maximum time before another object spawns")]
        [SerializeField] float maxSpawnInterval = 1.7f;
        [Tooltip("If true, spawner never turns off and will spawn continuously without end")]
        [SerializeField] bool SpawnInfinitely = false;

        [Header("Spawned Object Settings")]
        [Tooltip("How long object stays before getting destroyed")]
        [SerializeField] float ObjectLifeTime = 5f;
        [Tooltip("Speed of Spawned Object")]
        [SerializeField] float ObjectSpeed = 25f;

        Transform spawnDest;
        float timebeforeSpawn;
        float spawnDuration;
        float spawnInterval;
        bool Spawning = false;

        void Start()
        {
            spawnDest = this.transform;
            timebeforeSpawn = TimeBeforeSpawn;
            spawnDuration = SpawnDuration;
            spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        private void Update()
        {
            if (Spawning == false && !SpawnInfinitely)
            {
                timebeforeSpawn -= Time.deltaTime;
                if (timebeforeSpawn <= 0)
                {
                    Spawning = true;
                }
            }
            else if (Spawning == true || !SpawnInfinitely)
            {
                if (spawnDuration > 0)
                {
                    spawnDuration -= Time.deltaTime;
                    SpawnObjects();
                }
                else
                {
                    Spawning = false;
                    timebeforeSpawn = TimeBeforeSpawn;
                    spawnDuration = SpawnDuration;
                }
            }
            else if (SpawnInfinitely)
            {
                 SpawnObjects();
            }
        }
        void SpawnObjects()
        {
            spawnInterval -= Time.deltaTime;

            if (spawnInterval <= 0)
            {
                int randomIndex = Random.Range(0, ObjectsToSpawn.Length);
                GameObject obj = ObjectsToSpawn[randomIndex];
                //Spawn object with same rotation as spawner (can be changed to whatever you want) at the position of the spawner
                obj = Instantiate(obj, spawnDest.position, spawnDest.rotation);

                //Tell spawned object to move towards player (if it has the script) and then destroy itself after 5 seconds (to prevent too many objects in the scene)
                if (obj.GetComponent<TrackTarget_E>() != null)
                {
                    obj.GetComponent<TrackTarget_E>().enabled = true;
                    //Give script the target position to move towards (the player)
                    obj.GetComponent<TrackTarget_E>().TargetObject = TargetPosition.transform;
                    //Set speed of object
                    obj.GetComponent<TrackTarget_E>().Speed = ObjectSpeed;
                    
                    Destroy(obj, ObjectLifeTime);
                }

                spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            }
        }
    }
}

