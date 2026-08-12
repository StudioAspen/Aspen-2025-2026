using UnityEngine;

public class JawOverride : MonoBehaviour
{
    [SerializeField] private Transform jawBone;
    [SerializeField] private Quaternion closedRotation = Quaternion.identity;

    private void Awake()
    {
        enabled = false;
    }

    void LateUpdate()
    {
        if (jawBone != null)
            jawBone.localRotation = closedRotation;
    }
}
