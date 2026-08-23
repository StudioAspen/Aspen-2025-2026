using UnityEngine;

public class JawOverride : MonoBehaviour
{
    [SerializeField] private Transform jawBone;
    [SerializeField] private Quaternion closedRotation = Quaternion.identity;

    void LateUpdate()
    {
        if (jawBone != null)
            jawBone.localRotation = closedRotation;
    }
}
