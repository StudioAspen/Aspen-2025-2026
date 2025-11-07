using UnityEngine;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections.Editor.Data;

public class MovingObstacle : MonoBehaviour
{
    public enum MovementType
    {
        Lerping,
        MovingTowards,
        SmoothDamp
    }

    [Tooltip("Path Points for the Moving Obstacle")]
    public List<Transform> pathPoints;
    private List<Vector3> pathPositions = new List<Vector3>();
    private int currentPointIndex = 1;
    private bool rubberBanding = false;

    private GameObject startPoint;

    [Header("Movement Settings")]
    public float speed = 2.0f;
    public MovementType movementType = MovementType.Lerping;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPoint = new GameObject("StartPoint");
        pathPoints.Insert(0, startPoint.transform);
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

        // Move towards the current point
        DetermineMoveType();

        // Check if reached current point and switch to next/previous waypoint
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
        FaceTowards();
    }

    private void OnDestroy()
    {
        if (startPoint != null)
        {
            Destroy(startPoint);
        }
    }

    private void DetermineMoveType()
    {
        if (movementType == MovementType.Lerping)
        {
            transform.position = Vector3.Lerp(transform.position, pathPositions[currentPointIndex], Mathf.Clamp01(speed * Time.deltaTime));
        }
        else if (movementType == MovementType.MovingTowards)
        {
            transform.position = Vector3.MoveTowards(transform.position, pathPositions[currentPointIndex], speed * Time.deltaTime);
        }
        else if (movementType == MovementType.SmoothDamp)
        {
            Vector3 velocity = Vector3.zero;
            transform.position = Vector3.SmoothDamp(transform.position, pathPositions[currentPointIndex], ref velocity, speed * Time.deltaTime);
        }
    }

    // Make the obstacle face towards the next point it is moving to
    private void FaceTowards()
    {
        Vector3 direction = pathPositions[currentPointIndex] - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }

}
