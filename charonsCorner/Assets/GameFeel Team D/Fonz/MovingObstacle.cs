using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections.Editor.Data;

public class MovingObstacle : MonoBehaviour
{
    [Tooltip("Path Points for the Moving Obstacle")]
    public List<Transform> pathPoints;
    private List<Vector3> pathPositions = new List<Vector3>();
    private int currentPointIndex = 1;
    private bool rubberBanding = false;

    [Header("Movement Settings")]
    public float speed = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathPoints.Insert(0, new GameObject("StartPoint").transform);
        pathPoints[0].position = transform.position;

        foreach (Transform point in pathPoints)
        {
            pathPositions.Add(point.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pathPositions.Count < 2) // need to have points to go to
        {
            Debug.LogWarning(this.name + " does not have path points assigned! Will not move.");
            return;
        }

        // Move towards the next point
        transform.position = Vector3.MoveTowards(transform.position, pathPositions[currentPointIndex], speed*Time.deltaTime);

        if (Vector3.Distance(transform.position, pathPositions[currentPointIndex]) < 0.01f)
        {
            Debug.Log("Reached point " + currentPointIndex);
            if (currentPointIndex+1 < pathPoints.Count && !rubberBanding)
            {
                currentPointIndex++; // Move to the next point   
            }
            else
            {
                rubberBanding = true;
            }

            if (currentPointIndex-1>-1 && rubberBanding)
            {
                currentPointIndex--; // Move to the previous point
            }
            else
            {
                rubberBanding = false;
            }
        }
    }

}
