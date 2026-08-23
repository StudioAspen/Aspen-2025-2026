using UnityEngine;

public class PositionOverride : MonoBehaviour
{
    [SerializeField] private Transform targetBone;
    [SerializeField] private Vector3 targetPosition = Vector3.zero;

    void LateUpdate()
    {
        if (targetBone != null)
            targetBone.localPosition = targetPosition;
    }
}
