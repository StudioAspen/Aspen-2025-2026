using Unity.Cinemachine;
using UnityEngine;

public class CameraFollowRotation : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    public Transform target; // your ball

    [Header("Offset Settings")]
    public Vector3 baseOffset = new Vector3(0, 2.72f, -8.8f);

    private CinemachineFollow transposer;

    void Start()
    {
        if (virtualCamera == null)
            virtualCamera = GetComponent<CinemachineCamera>();

        // Get the follow component from the camera
        if (virtualCamera != null)
            transposer = virtualCamera.GetComponent<CinemachineFollow>();
    }

    void LateUpdate()
    {
        if (target == null || transposer == null) return;

        // Calculate a rotation that makes the camera stay behind the target
        // Negative Y rotation ensures we flip direction if needed
        Quaternion yRotation = Quaternion.Euler(0, target.localEulerAngles.y, 0);

        // Use the *target’s forward direction* for the offset orientation
        Vector3 rotatedOffset = yRotation * baseOffset;
        //rotatedOffset.z = -8.8f;
        // Apply to camera
        transposer.FollowOffset = rotatedOffset;
    }
}
